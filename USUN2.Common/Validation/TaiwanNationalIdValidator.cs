namespace USUN2.Common.Validation;


public static class TaiwanNationalIdValidator
{
    
    private static ReadOnlySpan<int> LetterCodes =>
    [
        10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19,
        20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33
    ];

    public static string Normalize(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : input.Trim().ToUpperInvariant();
    }


    public static bool IsValid(string? input)
    {
        var id = Normalize(input);
        if (id.Length != 10)
            return false;

        var letter = id[0];
        if (letter is < 'A' or > 'Z')
            return false;

        for (var i = 1; i < 10; i++)
        {
            if (id[i] is < '0' or > '9')
                return false;
        }

        var gender = id[1];
        if (gender is not ('1' or '2' or '8' or '9'))
            return false;

        var letterIndex = letter - 'A';
        if (letterIndex < 0 || letterIndex >= LetterCodes.Length)
            return false;

        var code = LetterCodes[letterIndex];
        var sum = code / 10 + code % 10 * 9;
        for (var i = 1; i < 9; i++)
        {
            sum += (id[i] - '0') * (9 - i);
        }

        var check = (10 - sum % 10) % 10;
        var last = id[9] - '0';
        return check == last;
    }
}
