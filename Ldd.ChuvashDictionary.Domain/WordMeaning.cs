namespace Ldd.ChuvashDictionary.Domain;

public sealed class WordMeaning(int index, string meaning, IEnumerable<string> examples)
{
    public int Index { get; } = index;

    public string Meaning { get; } = meaning;

    public string[] Examples { get; } = [.. examples];
}
