using Auction.Application.DTOs.Auctions;
using Auction.Application.Interfaces;
using Auction.Api.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Auction.Api.Controllers;

// Auction management endpoints.
[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private const int MaxImageCount = 5;
    private const long MaxImageSize = 5 * 1024 * 1024;
    private readonly IAuctionService _auctionService;
    private readonly IValidator<CreateAuctionRequest> _createAuctionValidator;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuctionsController> _logger;

    public AuctionsController(
        IAuctionService auctionService,
        IValidator<CreateAuctionRequest> createAuctionValidator,
        IWebHostEnvironment environment,
        ILogger<AuctionsController> logger)
    {
        _auctionService = auctionService;
        _createAuctionValidator = createAuctionValidator;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Get all auctions with optional search, filter, and pagination.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctions(
        [FromQuery] AuctionQueryParams query,
        CancellationToken ct = default)
    {
        var result = await _auctionService.GetPagedAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get auction details by ID.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuction(int id, CancellationToken ct = default)
    {
        var auction = await _auctionService.GetByIdAsync(id, ct);
        return Ok(auction);
    }

    /// <summary>
    /// Create a new auction (Sellers only).
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateAuction(
        [FromBody] CreateAuctionRequest request,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _auctionService.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetAuction), new { id = result.Id }, result);
    }

    [HttpPost("with-images")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CreateAuctionWithImages(
        [FromForm] CreateAuctionFormRequest form,
        CancellationToken ct = default)
    {
        if (form.Images.Count is < 1 or > MaxImageCount)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid images",
                Detail = $"Upload between 1 and {MaxImageCount} images."
            });
        }

        var request = form.ToCreateAuctionRequest(Array.Empty<string>());
        var validation = await _createAuctionValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var extensions = new List<string>(form.Images.Count);
        foreach (var image in form.Images)
        {
            if (image.Length is <= 0 or > MaxImageSize)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid image size",
                    Detail = "Each image must be between 1 byte and 5 MB."
                });
            }

            var extension = await GetImageExtensionAsync(image, ct);
            if (extension == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unsupported image",
                    Detail = "Upload JPEG, PNG, or WebP images only."
                });
            }

            extensions.Add(extension);
        }

        var uploadDirectory = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads",
            "auctions");
        Directory.CreateDirectory(uploadDirectory);

        var savedFiles = new List<string>(form.Images.Count);
        try
        {
            var imageUrls = new List<string>(form.Images.Count);
            for (var index = 0; index < form.Images.Count; index++)
            {
                var fileName = $"{Guid.NewGuid():N}{extensions[index]}";
                var filePath = Path.Combine(uploadDirectory, fileName);
                savedFiles.Add(filePath);

                await using var fileStream = System.IO.File.Create(filePath);
                await form.Images[index].CopyToAsync(fileStream, ct);
                imageUrls.Add($"/uploads/auctions/{fileName}");
            }

            var requestWithImages = form.ToCreateAuctionRequest(imageUrls);
            var userId = GetCurrentUserId();
            var result = await _auctionService.CreateAsync(userId, requestWithImages, ct);
            return CreatedAtAction(nameof(GetAuction), new { id = result.Id }, result);
        }
        catch
        {
            foreach (var filePath in savedFiles)
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (IOException cleanupException)
                {
                    _logger.LogError(cleanupException, "Failed to remove an uploaded auction image at {Path}", filePath);
                }
                catch (UnauthorizedAccessException cleanupException)
                {
                    _logger.LogError(cleanupException, "Failed to remove an uploaded auction image at {Path}", filePath);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Get auctions created by the current seller.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetMyAuctions(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _auctionService.GetMyAuctionsAsync(userId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update an auction (owner only, Scheduled status only).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UpdateAuction(
        int id,
        [FromBody] UpdateAuctionRequest request,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var result = await _auctionService.UpdateAsync(id, userId, request, ct);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an auction (owner only, Scheduled status only).
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> CancelAuction(
        int id,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _auctionService.CancelAsync(id, userId, ct);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    User.FindFirst("sub");
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    private static async Task<string?> GetImageExtensionAsync(IFormFile image, CancellationToken ct)
    {
        var signature = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(signature.AsMemory(), ct);

        if (bytesRead >= 8 &&
            signature.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return ".png";
        }

        if (bytesRead >= 3 &&
            signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF)
        {
            return ".jpg";
        }

        if (bytesRead >= 12 &&
            Encoding.ASCII.GetString(signature, 0, 4) == "RIFF" &&
            Encoding.ASCII.GetString(signature, 8, 4) == "WEBP")
        {
            return ".webp";
        }

        return null;
    }
}