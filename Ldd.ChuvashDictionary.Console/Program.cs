using Ldd.ChuvashDictionary.Convertion;
using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml;
using System.Globalization;
using System.Text;

Encoding encoding = new UTF8Encoding(false, false);

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
        string newFileName = Path.Combine(Environment.CurrentDirectory, $"{cultureFrom.Name}_{cultureTo.Name}.xml");
        using FileStream fs = new(newFileName, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new(fs, encoding);
        bool success = XmlDictionarySerializer.TrySerialize(dictionary, sw);
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
        string newFileName = Path.Combine(Environment.CurrentDirectory, $"{cultureFrom.Name}_{cultureTo.Name}.xml");
        using FileStream fs = new(newFileName, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new(fs, encoding);
        bool success = XmlDictionarySerializer.TrySerialize(dictionary, sw);
        sw.Flush();
    }
}

/*
static string RemoveBrokenSymbolsInText(string filePath, Encoding encoding)
{
    string result = "";
    byte[] fileBytes = File.ReadAllBytes(filePath);
    string fileText = encoding.GetString(fileBytes);
    byte[] convBytes = encoding.GetBytes(fileText);
    if (convBytes.Length != fileBytes.Length)
    {
        // need cleaning

    }
    else
    {
        int fileindex = 0;
        int convindex = 0;
        while (fileindex < convBytes.Length)
        {
            if (fileBytes[fileindex] != convBytes[convindex])
            {
                fileindex++;
            }
            else
            {
                result += BitConverter.ToChar(fileBytes[fileindex]);
            }
        }
    }
    
    return result;
}
*/