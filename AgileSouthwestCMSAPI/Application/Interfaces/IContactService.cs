using AgileSouthwestCMSAPI.Api.Requests.Contact;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IContactService
{
    public Task SubmitContactForm(SubmitContactFormRequest request);
}