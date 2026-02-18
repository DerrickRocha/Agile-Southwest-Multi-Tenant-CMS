namespace AgileSouthwestCMSAPI.Application.Exceptions;

public class UserNotConfirmedAuthException(string message = "User is not confirmed.") : Exception(message);
