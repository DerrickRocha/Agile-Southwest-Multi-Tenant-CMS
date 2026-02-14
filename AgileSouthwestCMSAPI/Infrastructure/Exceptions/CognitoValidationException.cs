namespace AgileSouthwestCMSAPI.Infrastructure.Exceptions;

public class CognitoValidationException : Exception
{
    public CognitoValidationException(string message)
        : base(message) { }
}
