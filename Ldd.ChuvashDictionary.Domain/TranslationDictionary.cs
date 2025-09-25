using System.Globalization;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class TranslationDictionary(CultureInfo sourceLanguage, CultureInfo targetLanguage, IEnumerable<Guid> words)
{
    public CultureInfo SourceLanguage { get; } = sourceLanguage;

    public CultureInfo TargetLanguage { get; } = targetLanguage;

    public Guid[] Words { get; } = [.. words];
}
