namespace Auction.Domain.Exceptions;

// Thrown when a requested resource (auction, user, etc.) does not exist.
// Maps to HTTP 404 in the global exception middleware.
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
