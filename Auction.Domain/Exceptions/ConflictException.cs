namespace Auction.Domain.Exceptions;

// Thrown when two concurrent requests conflict (e.g., two bids at the same time).
// Maps to HTTP 409 in the global exception middleware.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
