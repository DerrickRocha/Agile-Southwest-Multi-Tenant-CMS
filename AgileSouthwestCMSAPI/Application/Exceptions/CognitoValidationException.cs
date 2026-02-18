namespace AgileSouthwestCMSAPI.Application.Exceptions;

public class CognitoValidationException : Exception
{
    public CognitoValidationException(string message)
        : base(message) { }
}
