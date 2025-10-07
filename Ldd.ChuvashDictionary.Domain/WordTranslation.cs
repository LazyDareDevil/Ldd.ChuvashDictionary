using System.Collections.ObjectModel;

namespace Ldd.ChuvashDictionary.Domain;

public sealed class WordTranslation(IEnumerable<WordProForm> proForms, IEnumerable<Guid> linkedWords)
{
    public ReadOnlyCollection<WordProForm> ProForms { get; } = new([.. proForms]);

    public ReadOnlyCollection<Guid> LinkedWords { get; } = new([.. linkedWords]);

    public string Description { get; set; } = string.Empty;

    public override bool Equals(object? obj)
    {
        if (obj is not WordTranslation translation)
        {
            return false;
        }

        if (!string.Equals(translation.Description, Description)
            || ProForms.Count != translation.ProForms.Count
            || LinkedWords.Any(e => !translation.LinkedWords.Contains(e))
            || translation.LinkedWords.Any(e => !LinkedWords.Contains(e)))
        {
            return false;
        }

        // TODO: check equality of ProForms???
        return true;
    }

    public override int GetHashCode() => base.GetHashCode();
}
