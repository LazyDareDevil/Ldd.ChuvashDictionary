namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryConfiguration(string[] authors,
                                            string otherInformation,
                                            IEnumerable<TranslationDictionary> dictionaries)
{
    public string[] Authors { get; } = authors;

    public string OtherInformation { get; } = otherInformation;

    public TranslationDictionary[] Dictionaries { get; } = [.. dictionaries];
}
