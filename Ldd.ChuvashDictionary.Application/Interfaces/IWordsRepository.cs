using Ldd.ChuvashDictionary.Domain;

namespace Ldd.ChuvashDictionary.Application.Interfaces;

public interface IWordsRepository
{
    public Task<IEnumerable<DictionaryWord>> GetAllWords(CancellationToken token = default);

    public Task<IEnumerable<DictionaryWord>> GetWordsBySearch(string searchWord, CancellationToken token = default);
}
