namespace Auction.Domain.Exceptions;

// Thrown when a user tries to do something they are not allowed to do.
// Maps to HTTP 403 in the global exception middleware.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
