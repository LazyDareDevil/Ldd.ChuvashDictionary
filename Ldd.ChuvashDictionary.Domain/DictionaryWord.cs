using System.Collections.ObjectModel;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class DictionaryWord(Guid id, string word, IEnumerable<WordProForm> proForms)
{
    public DictionaryWord(Guid id, string word, IEnumerable<WordProForm> meanings, IEnumerable<Guid> linkedWords) : this(id, word, meanings)
    {
        LinkedWords = new([.. linkedWords]);
    }

    public Guid Id { get; } = id;

    public string Word { get; } = word;

    public ReadOnlyCollection<WordProForm> ProForms { get; } = new([.. proForms]);

    public ReadOnlyCollection<Guid> LinkedWords { get; } = new([]);
}
