namespace Ldd.ChuvashDictionary.Domain;

public sealed class WordMeaning(int index, string meaning, string description)
{
    public int Index { get; } = index;

    public string Meaning { get; } = meaning;

    public string Description { get; } = description;
}
