namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryWord(Guid id, string word, IEnumerable<WordMeaning> meanings)
{
    public DictionaryWord(Guid id, string word, IEnumerable<WordMeaning> meanings, IEnumerable<Guid> linkedWords) : this(id, word, meanings)
    {
        LinkedWords = [.. linkedWords];
    }

    public Guid Id { get; } = id;

    public string Word { get; } = word;

    public WordMeaning[] Meanings { get; } = [.. meanings];

    public Guid[] LinkedWords { get; } = [];
}
