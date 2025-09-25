using System.Collections.ObjectModel;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class WordProForm(int index, string description, IEnumerable<WordMeaning> meanings)
{
    public int Index { get; } = index;

    public string Description { get; } = description;

    public ReadOnlyCollection<WordMeaning> Meanings { get; } = new([.. meanings]);
}
