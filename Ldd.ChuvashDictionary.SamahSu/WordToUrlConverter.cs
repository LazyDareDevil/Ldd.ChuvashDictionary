using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Ldd.ChuvashDictionary.SamahSu;

public static class WordToUrlConverter
{
    public static bool TryGetUrlEncodedWord(string originalWord, [MaybeNullWhen(false)] out string urlString)
    {
        urlString = HttpUtility.UrlEncode(originalWord);
        return !string.IsNullOrEmpty(urlString);
    }
}
