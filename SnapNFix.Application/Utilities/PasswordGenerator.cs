using System.Security.Cryptography;
using System.Text;

namespace SnapNFix.Application.Utilities;

public static class PasswordGenerator
{
    private static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
    private static readonly string Numbers = "0123456789";
    private static readonly string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    public static string GenerateStrongPassword(int length = 16)
    {
        if (length < 8)
            length = 8; 

        var password = new StringBuilder();

        password.Append(GetRandomChar(UppercaseLetters));
        password.Append(GetRandomChar(LowercaseLetters));
        password.Append(GetRandomChar(Numbers));
        password.Append(GetRandomChar(SpecialCharacters));

        var allChars = UppercaseLetters + LowercaseLetters + Numbers + SpecialCharacters;
        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        return new string(password.ToString().ToCharArray().OrderBy(x => RandomNumberGenerator.GetInt32(length)).ToArray());
    }

    private static char GetRandomChar(string characterSet)
    {
        return characterSet[RandomNumberGenerator.GetInt32(characterSet.Length)];
    }
}