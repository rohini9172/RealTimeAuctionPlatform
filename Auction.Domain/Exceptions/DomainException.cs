namespace Auction.Domain.Exceptions;

// Thrown when a business rule is violated (e.g., bidding on your own auction).
// Maps to HTTP 400 in the global exception middleware.
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
