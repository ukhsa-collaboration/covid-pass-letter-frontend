namespace CovidLetter.Frontend.WebApp.Services
{
    public interface IObfuscationService
    {
        bool TryObfuscateEmail(string emailValue, out string obfuscatedEmail);
        string ObfuscatePhone(string phoneValue);
    }
}
