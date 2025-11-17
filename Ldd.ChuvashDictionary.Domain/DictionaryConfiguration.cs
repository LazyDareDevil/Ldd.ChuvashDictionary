using System.Globalization;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryConfiguration(CultureInfo sourceLanguage, 
                                            CultureInfo targetLanguage,
                                            string[] authors,
                                            string description)
{
    public CultureInfo SourceLanguage { get; } = sourceLanguage;

    public CultureInfo TargetLanguage { get; } = targetLanguage;

    public string[] Authors { get; } = authors;

    public string Description { get; } = description;
}
