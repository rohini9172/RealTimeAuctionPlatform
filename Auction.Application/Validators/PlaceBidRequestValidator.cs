using Auction.Application.DTOs.Bids;
using FluentValidation;

namespace Auction.Application.Validators;

public class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than zero.");
    }
}
