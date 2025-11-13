namespace Ldd.ChuvashDictionary.Console;

readonly struct TranslationPair(string word, Guid translaiton)
{
    public string Word { get; } = word;

    public Guid Translation { get; } = translaiton;
}
