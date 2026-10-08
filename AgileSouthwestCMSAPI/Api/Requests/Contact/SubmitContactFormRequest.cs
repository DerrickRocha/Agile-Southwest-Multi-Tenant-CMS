namespace AgileSouthwestCMSAPI.Api.Requests.Contact;

public record SubmitContactFormRequest(string FirstName, string LastName, string Email, string Message);