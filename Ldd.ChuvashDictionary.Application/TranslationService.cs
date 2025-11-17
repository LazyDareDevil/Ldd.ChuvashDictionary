using Ldd.ChuvashDictionary.Application.Interfaces;
using Ldd.ChuvashDictionary.Domain;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Ldd.ChuvashDictionary.Application;

public class TranslationService
{
    private readonly ReadOnlyDictionary<CultureInfo, IWordDictionaryService[]> _availableDictionaries;

    private IWordDictionaryService[] _activeDictionaries = [];

    public TranslationService(IEnumerable<IWordDictionaryService> dictionaryServices)
    {
        Dictionary<CultureInfo, IWordDictionaryService[]> dictionaries = [];
        foreach (CultureInfo originCulture in dictionaryServices.Select(d => d.OriginCulture).Distinct())
        {
            dictionaries.Add(originCulture, [..dictionaryServices.Where(d => d.OriginCulture == originCulture)]);
        }
        
        _availableDictionaries = dictionaries.AsReadOnly();
    }

    public void SetCurrentDictionaries(CultureInfo sourceCulture, CultureInfo targetCulture)
    {
        if (_availableDictionaries.TryGetValue(sourceCulture, out IWordDictionaryService[]? dictionaries))
        {
            _activeDictionaries = [.. dictionaries.Where(e => e.TargetCulture == targetCulture)];
        }
        else
        {
            _activeDictionaries = [];
        }
    }

    public async Task<IEnumerable<DictionaryWord>> GetTranslations(string word, CancellationToken token)
    {
        List<DictionaryWord> result = [];
        foreach (IWordDictionaryService dictionaryService in _activeDictionaries)
        {
            result.AddRange(await dictionaryService.GetWordsBySearch(word, token));
        }

        return result;
    }
}
