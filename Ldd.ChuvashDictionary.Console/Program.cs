using Ldd.ChuvashDictionary.Console;
using Ldd.ChuvashDictionary.Convertion;
using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml;
using System.Globalization;
using System.Text;

Console.InputEncoding = Encodings.InputEncoding;
Console.OutputEncoding = Encodings.OutputEncoding;

//TestEnconding();

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

//ConvertOldDictionary(di);

Console.WriteLine($"Input encoding: {Console.InputEncoding}");
Console.WriteLine($"Output encoding: {Console.OutputEncoding}");
TranslationDictionary[] dictionaries = [.. LoadNewDictionaries(di, Encodings.DataEncoding)];
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
    while (true)
    {
        Console.WriteLine($"Current dictionary: {currentDictionary.Configuration.SourceLanguage.DisplayName} -> {currentDictionary.Configuration.TargetLanguage.DisplayName}");
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

        ShowWordTranslations(word, currentDictionary.Words);
    }
}

static void ShowWordTranslations(string searchWord, DictionaryWord[] words)
{
    DictionaryWord[] foundWords = [.. words.Where(w => w.Word.Length / 2 < searchWord.Length && w.Word.Contains(searchWord, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(e => e.Word.StartsWith(searchWord))];
    string? wordIndexInput;
    while (true)
    {
        Console.WriteLine($"By search '{searchWord}' found words:");
        for (int i = 0; i < foundWords.Length; i++)
        {
            Console.WriteLine($"\t{i + 1}: {foundWords[i].Word}");
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

        DictionaryWord selectedWord = foundWords[wordIndex - 1];
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(selectedWord.Word);
        foreach (WordMeaning meaning in selectedWord.Meanings)
        {
            Console.WriteLine($"\t{meaning.Meaning.Replace("\n", "\t\n")}");
            Console.WriteLine($"\t{meaning.Description.Replace("\n", "\t\n")}");
        }

        Console.ForegroundColor = ConsoleColor.White;
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
            Console.WriteLine($"\t{i + 1}: {dictionaries[i].Configuration.SourceLanguage.DisplayName} -> {dictionaries[i].Configuration.TargetLanguage.DisplayName}");
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
        if (XmlDictionarySerializer.TryDeserialize(sr, out DictionaryConfiguration? configuration, out DictionaryWord[] words))
        {
            yield return new(configuration, words);
        }
        else
        {
            Console.WriteLine($"{fileInfo.FullName} not loaded");
        }
    }
}

static void ConvertOldDictionary(DirectoryInfo dictionaryFolder)
{
    string[] lines = File.ReadAllLines("userdata.txt");
    string langFrom = lines[0];
    string langTo = lines[1];
    string wordsFile = lines[2];
    string transFile = lines[3];
    CultureInfo cultureFrom = new(langFrom);
    CultureInfo cultureTo = new(langTo);

    ParseDictionaryFile(dictionaryFolder, cultureFrom, cultureTo, wordsFile, transFile, Encoding.UTF8);

    langFrom = lines[4];
    langTo = lines[5];
    wordsFile = lines[6];
    transFile = lines[7];
    cultureFrom = new(langFrom);
    cultureTo = new(langTo);

    ParseDictionaryFile(dictionaryFolder, cultureFrom, cultureTo, wordsFile, transFile, Encoding.UTF8);
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

    DictionaryWord[] loadedWords = [.. OldDictionaryConverter.LoadDictionary(wordsReader, transReader)];
    if (loadedWords.Length > 0)
    {
        DictionaryConfiguration configuration = new(cultureFrom, cultureTo, [], string.Empty);
        string newFileName = Path.Combine(dictionaryFolder.FullName, $"{cultureFrom.Name}_{cultureTo.Name}.xml");
        using FileStream fs = new(newFileName, FileMode.CreateNew, FileAccess.Write);
        using StreamWriter sw = new(fs, encoding);
        bool success = XmlDictionarySerializer.TrySerialize(configuration, loadedWords, sw);
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
    //string t_utf8 = "тăван";
    string t_utf8 = "ăĕÿçĂĔŸÇ";
    // input from keyboard
    //string t_unicode = "тӑван";
    string t_unicode = "ӑӗӳҫӐӖӲҪ";

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
