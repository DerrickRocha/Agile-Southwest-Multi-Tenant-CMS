namespace AgileSouthwestCMSAPI.Application.Exceptions;

public class NotFoundException(string errorMessage): Exception(errorMessage);