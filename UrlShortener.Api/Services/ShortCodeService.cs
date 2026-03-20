namespace UrlShortener.Api.Services;

public class ShortCodeService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Encode(int id)
    {
        if (id == 0)
            return Alphabet[0].ToString();

            var chars = new List<char>();

            while (id > 0)
        {
            chars.Add(Alphabet[id % Alphabet.Length]);
            id /= Alphabet.Length;
        }

        chars.Reverse();
        return new string(chars.ToArray());
        }
    }
