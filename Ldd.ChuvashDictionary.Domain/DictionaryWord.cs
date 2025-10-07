namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryWord(Guid id, string word, WordTranslation translation)
{
    public Guid Id { get; } = id;

    public string Word { get; } = word;

    public WordTranslation Translation { get; } = translation;
}
