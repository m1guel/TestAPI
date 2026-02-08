namespace TestAPI.Domain.Exceptions
{
    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }

        public UnauthorizedException() : base("Unauthorized access.")
        {
        }
    }
}