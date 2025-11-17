using Ldd.ChuvashDictionary.Domain;
using System.Globalization;

namespace Ldd.ChuvashDictionary.Application.Interfaces;

public interface IWordDictionaryService
{
    public CultureInfo OriginCulture { get; }

    public CultureInfo TargetCulture { get; }

    public Task<IEnumerable<DictionaryWord>> GetWordsBySearch(string searchWord, CancellationToken token = default);
}
