namespace UrlShortener.Api.Services;

public class ShortCodeService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private readonly Random _random = new();

    public string Generate(int length = 6)
    {
        return new string(
            Enumerable.Range(0, length)
            .Select(_=> Alphabet[_random.Next(Alphabet.Length)])
            .ToArray()
        );
    }
}