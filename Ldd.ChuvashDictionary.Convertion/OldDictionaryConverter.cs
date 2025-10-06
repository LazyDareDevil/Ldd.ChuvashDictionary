//#define SPLIT

using Ldd.ChuvashDictionary.Domain;
using System.Diagnostics;
using System.Text;
#if SPLIT
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
#endif

namespace Ldd.ChuvashDictionary.Convertion;

public static class OldDictionaryConverter
{
    public static readonly char TranslationsSeparator = '*';

    public static IEnumerable<DictionaryWord> LoadDictionary(StreamReader wordListReader, StreamReader wordTranslationsReader)
    {
        List<DictionaryWord> result = [];
        string translations = wordTranslationsReader.ReadToEnd();
        int translationFileLength = translations.Length;
        string? line = wordListReader.ReadLine();
        Encoding wordsEncoding = wordListReader.CurrentEncoding;
        while (line is not null)
        {
            wordsEncoding.GetBytes(line);
            string[] parts = line.Split(':');
            line = wordListReader.ReadLine();
            if (parts.Length != 2)
            {
                continue;
            }

            string word = parts[0];
            if (!int.TryParse(parts[1], out int currentTextPosition)
                || currentTextPosition < 0
                || currentTextPosition >= translationFileLength)
            {
                continue;
            }

            string currentWordTranslation = translations[currentTextPosition..];
            int nextLocation = currentWordTranslation.IndexOf(TranslationsSeparator);
            if (nextLocation > 0)
            {
                currentWordTranslation = currentWordTranslation[..nextLocation];
            }
            else
            {
                break;
            }

            if (currentWordTranslation.Contains("его и пуля не берет"))
            {
                Debug.WriteLine("");
            }

            string description = currentWordTranslation.Replace("<br>", "\n");
            description = description.Replace("<i>", "");
            description = description.Replace("</i>", "");
            description = description.Replace("<b>", "");
            description = description.Replace("</b>", "");
            description = description.Replace("<p>", "");
            description = description.Replace("</p>", "\n");
            result.Add(new(Guid.NewGuid(), word, [])
            {
                Description = description,
            });

#if SPLIT
            List<ParsedProForm> allProForms = [];
            ParsedProForm? currentProform = null;
            ParsedProForm emptyProForm = new();
            string[] split = MeaningTextKeys.SplitParagraph().Split(currentWordTranslation);
            foreach (string paragraph in split)
            {
                if (string.IsNullOrEmpty(paragraph))
                {
                    continue;
                }
                
                Match paragraphMatch = MeaningTextKeys.PartWithRomeNumber().Match(paragraph);
                if (paragraphMatch.Success
                    && paragraphMatch.Groups.TryGetValue("romeNumber", out Group? romeNumber)
                    && paragraphMatch.Groups.TryGetValue("content", out Group? contentValue))
                {
                    if (currentProform is not null)
                    {
                        allProForms.Add(currentProform);
                        currentProform = null;
                    }

                    if (TryParseContent(contentValue.Value, out string meaningText, out string content)
                        && TryConvertRomanToInteger(romeNumber.Value, out int index))
                    {
                        currentProform = new()
                        {
                            Index = index,
                            Description = meaningText
                        };
                        if (TryParseMeaning(content, out ParsedMeaning? meaning))
                        {
                            currentProform.Meanings.Add(meaning);
                        }
                        else
                        {
                            currentProform.Description += content;
                        }
                    }
                }
                else
                {
                    if (TryParseMeaning(paragraphMatch.Value, out ParsedMeaning? meaning))
                    {
                        if (currentProform is null)
                        {
                            emptyProForm.Meanings.Add(meaning);
                        }
                        else
                        {
                            currentProform.Meanings.Add(meaning);
                        }
                    }
                }
            }

            result.Add(new(Guid.NewGuid(), word, allProForms.Select(f => new WordProForm(f.Index, f.Description, f.Meanings.Select(m => new WordMeaning(m.Index, m.Meaning, m.Examples))))));
#endif
        }

        return result;
    }

#if SPLIT
    private static bool TryParseMeaning(string text, [MaybeNullWhen(false)] out ParsedMeaning meaning)
    {
        meaning = null;
        Match meaningMatch = MeaningTextKeys.PartWithRomeNumber().Match(text);
        if (!meaningMatch.Success
            || !meaningMatch.Groups.TryGetValue("arabNumber", out Group? indexValue)
            || !int.TryParse(indexValue.Value, out int index)
            || !meaningMatch.Groups.TryGetValue("content", out Group? content)
            || !TryParseContent(content.Value, out string meaningText, out string meaningContent))
        {
            return false;
        }

        meaning = new()
        {
            Index = index,
            Meaning = meaningText,
            Examples = MeaningTextKeys.SplitExamples().Split(meaningContent)
        };
        return true;
    }

    internal class ParsedProForm()
    {
        public int Index { get; set; }

        public string Description { get; set; } = string.Empty;

        public List<ParsedMeaning> Meanings { get; } = [];
    }

    internal class ParsedMeaning()
    {
        public int Index { get; set; }

        public string Meaning { get; set; } = string.Empty;

        public string[] Examples { get; set; } = [];
    }

    private static bool TryParseContent(string text, out string meaningValue, out string meaningContent)
    {
        meaningValue = string.Empty;
        meaningContent = string.Empty;
        Match match = MeaningTextKeys.MeaningI().Match(text);
        if (match.Success
            && match.Groups.TryGetValue("meaning", out Group? meaning)
            && match.Groups.TryGetValue("content", out Group? content))
        {
            meaningValue = meaning.Value;
            meaningContent = content.Value;
            return true;
        }

        match = MeaningTextKeys.MeaningB().Match(text);
        if (match.Success
            && match.Groups.TryGetValue("meaning", out meaning)
            && match.Groups.TryGetValue("content", out content))
        {
            meaningValue = meaning.Value;
            meaningContent = content.Value;
            return true;
        }

        match = MeaningTextKeys.MeaningIB().Match(text);
        if (match.Success
            && match.Groups.TryGetValue("meaning1", out meaning)
            && match.Groups.TryGetValue("meaning2", out Group? meaning1)
            && match.Groups.TryGetValue("content", out content))
        {
            meaningValue = meaning.Value + meaning1.Value;
            meaningContent = content.Value;
            return true;
        }

        match = MeaningTextKeys.MeaningBI().Match(text);
        if (match.Success
            && match.Groups.TryGetValue("meaning1", out meaning)
            && match.Groups.TryGetValue("meaning2", out meaning1)
            && match.Groups.TryGetValue("content", out content))
        {
            meaningValue = meaning.Value + meaning1.Value;
            meaningContent = content.Value;
            return true;
        }

        return false;
    }

    private static readonly ReadOnlyDictionary<char, int> RomanMap =
        new(new Dictionary<char, int>()
    {
        {'I', 1},
        {'V', 5},
        {'X', 10},
        {'L', 50},
        {'C', 100},
        {'D', 500},
        {'M', 1000}
    });

    private static bool TryConvertRomanToInteger(string roman, out int value)
    {
        value = 0;
        char previousChar = roman[0];
        foreach (char currentChar in roman)
        {
            if (!RomanMap.TryGetValue(currentChar, out int number))
            {
                return false;
            }

            value += number;
            if (RomanMap[previousChar] < number)
            {
                value -= RomanMap[previousChar] * 2;
            }

            previousChar = currentChar;
        }

        return true;
    }
#endif
}
