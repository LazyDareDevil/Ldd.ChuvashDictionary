namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryWord(string word, IEnumerable<WordMeaning> meanings, IEnumerable<string> linkedWords)
{
    public string Word { get; } = word;

    public WordMeaning[] Meanings { get; } = [.. meanings];

    public string[] LinkedWords { get; } = [.. linkedWords];
}
