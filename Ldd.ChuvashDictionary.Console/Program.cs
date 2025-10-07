using Ldd.ChuvashDictionary.Convertion;
using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml;
using System.Globalization;
using System.Text;

Encoding encoding = new UTF8Encoding(true, false);
string dictionariesFolder = Path.Combine(Environment.CurrentDirectory, "Dictionaries");
DirectoryInfo di;
if (!Directory.Exists(dictionariesFolder)) 
{
    di = Directory.CreateDirectory(dictionariesFolder);
}
else 
{
    di = new DirectoryInfo(dictionariesFolder);
}

//ConvertOldDictionary(di, encoding);
TranslationDictionary[] dictionaries = [.. LoadNewDictionaries(di, encoding)];
Dictionary<string, Guid> availableWords;
Dictionary<Guid, WordTranslation> wordTranslations;
Console.WriteLine("Welcome to CHUVASH dictionary prepared by LazyDareDevil <Кӗпер тепӗр енӗ>");
string? word;
while (true)
{
    if (!TrySelectDictionary(dictionaries, out int index))
    {
        Console.WriteLine("Application is closing...");
        return;
    }

    TranslationDictionary currentDictionary = dictionaries[index];
    availableWords = currentDictionary.Words.ToDictionary(w => w.Word, w => w.Id);
    wordTranslations = currentDictionary.Words.ToDictionary(w => w.Id, w => w.Translation);
    while (true)
    {
        Console.WriteLine($"Current dictionary: {currentDictionary.SourceLanguage.NativeName} -> {currentDictionary.TargetLanguage.NativeName}");
        Console.WriteLine("Input 0 to exit application.");
        Console.WriteLine("Input 1 to change dictionary.");
        Console.WriteLine("Input word or it's part:");
        word = Console.ReadLine();
        if (word is null)
        {
            Console.WriteLine("Word is empty.");
            continue;
        }

        if (int.TryParse(word, out int input))
        {
            if (input == 0)
            {
                Console.WriteLine("Application is closing...");
                return;
            }

            if (input == 1)
            {
                break;
            }
        }

        ShowWordTranslations(word, availableWords, wordTranslations);
    }
}

static void ShowWordTranslations(string searchWord, Dictionary<string, Guid> availableWords, Dictionary<Guid, WordTranslation> wordTranslations)
{
    string[] foundWords = [.. availableWords.Keys.Where(w => w.Contains(searchWord, StringComparison.OrdinalIgnoreCase)).Order()];
    string? wordIndexInput;
    while (true)
    {
        Console.WriteLine($"By search '{searchWord}' found words:");
        for (int i = 0; i < foundWords.Length; i++)
        {
            Console.WriteLine($"\t{i + 1}: {foundWords[i]}");
        }

        Console.WriteLine("Input 0 to do other search.");
        Console.WriteLine("Input index of file to show translation:");
        wordIndexInput = Console.ReadLine();
        if (!int.TryParse(wordIndexInput, out int wordIndex) ||
            wordIndex < 0 ||
            wordIndex > foundWords.Length)
        {
            Console.WriteLine("Dictionary with input index do not exist");
            continue;
        }

        if (wordIndex == 0)
        {
            return;
        }

        wordIndex--;
        string word = foundWords[wordIndex];
        string description = wordTranslations[availableWords[word]].Description;
        Console.WriteLine($"\t*{word}");
        foreach (string split in description.Split("\n"))
        {
            Console.WriteLine($"\t{split}");
        }
    }
}

static bool TrySelectDictionary(TranslationDictionary[] dictionaries, out int dictionaryIndex)
{
    string? dictionaryIndexInput;
    while (true)
    {
        Console.WriteLine("Input 0 to exit application.");
        Console.WriteLine("Currently available translations:");
        for (int i = 0; i < dictionaries.Length; i++)
        {
            Console.WriteLine($"\t{i + 1}: {dictionaries[i].SourceLanguage.NativeName} -> {dictionaries[i].TargetLanguage.NativeName}");
        }

        Console.WriteLine("Select dictionary by input index of selected one:");
        dictionaryIndexInput = Console.ReadLine();
        if (!int.TryParse(dictionaryIndexInput, out dictionaryIndex) ||
            dictionaryIndex < 0 ||
            dictionaryIndex > dictionaries.Length)
        {
            Console.WriteLine("Dictionary with input index do not exist");
            continue;
        }

        if (dictionaryIndex == 0)
        {
            return false;
        }

        dictionaryIndex--;
        return true;
    }
}

static IEnumerable<TranslationDictionary> LoadNewDictionaries(DirectoryInfo inputDirectory, Encoding encoding)
{
    foreach (FileInfo fileInfo in inputDirectory.EnumerateFiles())
    {
        using FileStream fs = new(fileInfo.FullName, FileMode.Open, FileAccess.Read);
        using StreamReader sr = new(fs, encoding);
        TranslationDictionary? dictionary = XmlDictionarySerializer.Deserialize(sr);
        if (dictionary is not null)
        {
            yield return dictionary;
        }
        else
        {
            Console.WriteLine($"{fileInfo.FullName} not loaded");
        }
    }
}

static void ConvertOldDictionary(DirectoryInfo dictionaryFolder, Encoding encoding)
{
    string[] lines = File.ReadAllLines("userdata.txt", encoding);
    string langFrom = lines[0];
    string langTo = lines[1];
    string wordsFile = lines[2];
    string transFile = lines[3];
    CultureInfo cultureFrom = new(langFrom);
    CultureInfo cultureTo = new(langTo);

    {
        using FileStream wordsfs = new(wordsFile, FileMode.Open, FileAccess.Read);
        using StreamReader wordsReader = new(wordsfs, encoding);

        using FileStream transfs = new(transFile, FileMode.Open, FileAccess.Read);
        using StreamReader transReader = new(transfs, encoding);

        IEnumerable<DictionaryWord> loadedWords = OldDictionaryConverter.LoadDictionary(wordsReader, transReader);
        if (loadedWords.Any())
        {
            TranslationDictionary dictionary = new(cultureFrom, cultureTo, loadedWords);
            string newFileName = Path.Combine(dictionaryFolder.FullName, $"{cultureFrom.Name}_{cultureTo.Name}.xml");
            using FileStream fs = new(newFileName, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs, encoding);
            bool success = XmlDictionarySerializer.TrySerialize(dictionary, sw);
            if (!success)
            {
                Console.WriteLine($"{cultureFrom.Name}_{cultureTo.Name} not saved");
            }

            sw.Flush();
        }
    }

    langFrom = lines[4];
    langTo = lines[5];
    wordsFile = lines[6];
    transFile = lines[7];
    cultureFrom = new(langFrom);
    cultureTo = new(langTo);

    {
        using FileStream wordsfs = new(wordsFile, FileMode.Open, FileAccess.Read);
        using StreamReader wordsReader = new(wordsfs, encoding);

        using FileStream transfs = new(transFile, FileMode.Open, FileAccess.Read);
        using StreamReader transReader = new(transfs, encoding);
        IEnumerable<DictionaryWord> loadedWords = OldDictionaryConverter.LoadDictionary(wordsReader, transReader);
        if (loadedWords.Any())
        {
            TranslationDictionary dictionary = new(cultureFrom, cultureTo, loadedWords);
            string newFileName = Path.Combine(dictionaryFolder.FullName, $"{cultureFrom.Name}_{cultureTo.Name}.xml");
            using FileStream fs = new(newFileName, FileMode.Create, FileAccess.Write);
            using StreamWriter sw = new(fs, encoding);
            bool success = XmlDictionarySerializer.TrySerialize(dictionary, sw);
            if (!success)
            {
                Console.WriteLine($"{cultureFrom.Name}_{cultureTo.Name} not saved");
            }

            sw.Flush();
        }
    }
}
