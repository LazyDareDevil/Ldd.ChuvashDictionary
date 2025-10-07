using System.Collections.ObjectModel;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class WordTranslation(IEnumerable<WordProForm> proForms, IEnumerable<Guid> linkedWords)
{
    public ReadOnlyCollection<WordProForm> ProForms { get; } = new([.. proForms]);

    public ReadOnlyCollection<Guid> LinkedWords { get; } = new([.. linkedWords]);

    public string Description { get; set; } = string.Empty;
}
