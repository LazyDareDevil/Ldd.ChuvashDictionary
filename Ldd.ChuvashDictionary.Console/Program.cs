using Ldd.ChuvashDictionary.Convertion;
using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml;
using System.Globalization;
using System.Text;
using System.Text.Unicode;

Console.InputEncoding = Encodings.InputEncoding;
Console.OutputEncoding = Encodings.OutputEncoding;

// TODO: problem with input encoding!!!! UTF-8 not support cyrillyc and special symbols
// Unicode encdong not pairs with data, loaded with UTF8 encoding

TestEnconding();

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

Console.WriteLine($"Input encoding: {Console.InputEncoding}");
Console.WriteLine($"Output encoding: {Console.OutputEncoding}");
TranslationDictionary[] dictionaries = [.. LoadNewDictionaries(di, Encodings.DataEncoding)];
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
        Console.WriteLine($"Current dictionary: {currentDictionary.SourceLanguage.DisplayName} -> {currentDictionary.TargetLanguage.DisplayName}");
        Console.WriteLine("Input 0 to exit application.");
        Console.WriteLine("Input 1 to change dictionary.");
        Console.WriteLine("Input word or it's part:");
        word = Console.ReadLine();
        if (word is null)
        {
            Console.WriteLine("Word is empty.");
            continue;
        }

        //word = UnicodeToUTF8(word, encoding);
        //word = ConvertInputString(word, inputEncoding, encoding);
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
    string[] foundWords = [.. availableWords.Keys.Where(w => w.Length / 2 < searchWord.Length && w.ToLower().Contains(searchWord, StringComparison.InvariantCultureIgnoreCase)).OrderBy(e => e.StartsWith(searchWord))];
    string? wordIndexInput;
    while (true)
    {
        Console.WriteLine($"By search '{searchWord}' found words:");
        for (int i = 0; i < foundWords.Length; i++)
        {
            Console.WriteLine($"\t{i + 1}: {foundWords[i]}");
        }

        Console.WriteLine("Input 0 to do other search.");
        Console.WriteLine("Input index of word to show translation:");
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
            Console.WriteLine($"\t{i + 1}: {dictionaries[i].SourceLanguage.DisplayName} -> {dictionaries[i].TargetLanguage.DisplayName}");
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
    foreach (FileInfo fileInfo in inputDirectory.EnumerateFiles("*.xml"))
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

    ParseDictionaryFile(dictionaryFolder, cultureFrom, cultureTo, wordsFile, transFile, encoding);

    langFrom = lines[4];
    langTo = lines[5];
    wordsFile = lines[6];
    transFile = lines[7];
    cultureFrom = new(langFrom);
    cultureTo = new(langTo);

    ParseDictionaryFile(dictionaryFolder, cultureFrom, cultureTo, wordsFile, transFile, encoding);
}

static void ParseDictionaryFile(DirectoryInfo dictionaryFolder,
                                CultureInfo cultureFrom,
                                CultureInfo cultureTo,
                                string wordsFile,
                                string transFile,
                                Encoding encoding)
{
    using FileStream wordsfs = new(wordsFile, FileMode.Open, FileAccess.Read);
    using StreamReader wordsReader = new(wordsfs, encoding);

    using FileStream transfs = new(transFile, FileMode.Open, FileAccess.Read);
    using StreamReader transReader = new(transfs, encoding);

    DictionaryWord[] loadedWords = [.. OldDictionaryConverter.LoadDictionary(wordsReader, transReader, out string[] duplicatedWords)];
    if (duplicatedWords.Length > 0)
    {
        string wordsText = string.Join("\n", duplicatedWords);
        string fileName = Path.Combine(dictionaryFolder.FullName, $"{cultureFrom.Name}_{cultureTo.Name}_duplicates.txt");
        File.WriteAllText(fileName, wordsText);
    }

    if (loadedWords.Length > 0)
    {
        TranslationDictionary dictionary = new(cultureFrom, cultureTo, loadedWords);
        string newFileName = Path.Combine(dictionaryFolder.FullName, $"{cultureFrom.Name}_{cultureTo.Name}_1.xml");
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

static void TestEnconding()
{
    // copy from utf-8 encoded text
    string t_utf8 = "тăван";
    // input from keyboard
    string t_unicode = "тӑван";

    Console.WriteLine(t_utf8);
    PrintChars(t_utf8);
    PrintBytes(t_utf8, Encoding.UTF8);
    PrintBytes(t_utf8, Encoding.Unicode);
    Console.WriteLine(nameof(ConvertInputString));
    string utf8toUni = ConvertInputString(t_utf8, Encoding.UTF8, Encoding.Unicode);
    Console.WriteLine(utf8toUni);
    PrintBytes(utf8toUni, Encoding.UTF8);
    PrintBytes(utf8toUni, Encoding.Unicode);
    bool equals = string.Equals(utf8toUni, t_unicode);

    Console.WriteLine();
    Console.WriteLine(t_unicode);
    PrintChars(t_unicode);
    PrintBytes(t_unicode, Encoding.UTF8);
    PrintBytes(t_unicode, Encoding.Unicode);
    Console.WriteLine(nameof(ConvertInputString));
    string unitoutf8 = ConvertInputString(t_unicode, Encoding.Unicode, Encoding.UTF8);
    Console.WriteLine(unitoutf8);
    PrintBytes(unitoutf8, Encoding.UTF8);
    PrintBytes(unitoutf8, Encoding.Unicode);
    equals = string.Equals(unitoutf8, t_utf8);
    Console.WriteLine(nameof(Utf16ToUtf8));
    unitoutf8 = Utf16ToUtf8(t_unicode);
    Console.WriteLine(unitoutf8);
    PrintBytes(unitoutf8, Encoding.UTF8);
    PrintBytes(unitoutf8, Encoding.Unicode);
    equals = string.Equals(unitoutf8, t_utf8);

    Console.ReadLine();
}

static void PrintChars(string s)
{
    for (int i = 0; i < s.Length; i++)
    {
        Console.WriteLine("s[{0,1}] = '{1,-2}' {2,-50}", i, s[i], $"('\\u{(int)s[i]:x4}')");
    }
}

static void PrintBytes(string text, Encoding encoding)
{
    byte[] b = encoding.GetBytes(text);
    Console.WriteLine("{0,-20} {1,10}: [{2,-50}]", encoding.EncodingName, text, string.Join(", ", b));
}

static string Utf16ToUtf8(string utf16String)
{
    string utf8String = string.Empty;
    byte[] utf16Bytes = Encodings.InputEncoding.GetBytes(utf16String);
    byte[] utf8Bytes = Encoding.Convert(Encodings.InputEncoding, Encodings.DataEncoding, utf16Bytes);
    for (int i = 0; i < utf8Bytes.Length; i++)
    {
        byte[] utf8Container = [utf8Bytes[i], 0];
        utf8String += BitConverter.ToChar(utf8Container, 0);
    }

    return utf8String;
}

static string ConvertInputString(string text, Encoding input, Encoding output)
{
    byte[] data = input.GetBytes(text);
    byte[] resData = Encoding.Convert(input, output, data);
    return output.GetString(resData);
}
