using Ldd.ChuvashDictionary.Domain;

namespace Ldd.ChuvashDictionary.Console;

public sealed class TranslationDictionary(DictionaryConfiguration configuration, IEnumerable<DictionaryWord> words)
{
    public DictionaryConfiguration Configuration { get; } = configuration;

    public DictionaryWord[] Words { get; } = [.. words];
}
