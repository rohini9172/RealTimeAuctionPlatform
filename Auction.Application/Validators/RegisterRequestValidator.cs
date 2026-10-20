using Auction.Application.DTOs.Auth;
using FluentValidation;

namespace Auction.Application.Validators;

// Validates the registration request before it reaches the service layer.
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    // Allowed roles for public registration — Admin cannot self-register
    private static readonly string[] AllowedRoles = { "Seller", "Bidder" };

    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => AllowedRoles.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Role must be 'Seller' or 'Bidder'. Admin accounts cannot be created through registration.");
    }
}
