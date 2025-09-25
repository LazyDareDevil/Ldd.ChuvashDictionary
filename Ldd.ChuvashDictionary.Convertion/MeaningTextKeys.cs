using System.Text.RegularExpressions;

namespace Ldd.ChuvashDictionary.Convertion;

internal partial class MeaningTextKeys
{
    [GeneratedRegex("<p>|</p>")]
    public static partial Regex SplitParagraph();

    [GeneratedRegex(@"(?<romeNumber>[IVXLCDM]+)\.(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex PartWithRomeNumber();

    [GeneratedRegex(@"(?<arabNumber>[0-9]+)\.(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex PartWithArabNumber();

    [GeneratedRegex(@"<i>(?<meaning>.*)</i>.*<br>(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex MeaningI();

    [GeneratedRegex(@"<b>(?<meaning>\w*)</b>.*<br>(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex MeaningB();

    [GeneratedRegex(@"<b>(?<meaning1>\w*)</b>.*<i>(?<meaning2>.*)</i>.*<br>(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex MeaningBI();

    [GeneratedRegex(@"<i>(?<meaning1>\w*)</i>.*<b>(?<meaning2>.*)</b>.*<br>(?<content>.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    public static partial Regex MeaningIB();

    [GeneratedRegex("(<i>)|(</i>)|(<b>)|(</b>)")]
    public static partial Regex RemoveHtmlItems();

    [GeneratedRegex("<br>")]
    public static partial Regex SplitExamples();
}
