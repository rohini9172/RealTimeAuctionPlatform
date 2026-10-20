using Auction.Application.DTOs.Auctions;
using FluentValidation;

namespace Auction.Application.Validators;

public class UpdateAuctionRequestValidator : AbstractValidator<UpdateAuctionRequest>
{
    public UpdateAuctionRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A valid category is required.");

        RuleFor(x => x.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than zero.");

        RuleFor(x => x.MinIncrement)
            .GreaterThan(0).WithMessage("Minimum increment must be greater than zero.");

        RuleFor(x => x.StartTime)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");
    }
}
