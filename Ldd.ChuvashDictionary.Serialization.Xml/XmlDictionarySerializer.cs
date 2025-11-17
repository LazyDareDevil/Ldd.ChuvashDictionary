using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml.Serializable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml;

public static class XmlDictionarySerializer
{
    public static bool TryDeserialize(StreamReader reader, [MaybeNullWhen(false)] out DictionaryConfiguration configuration, out DictionaryWord[] words)
    {
        object? deserialized;
        try
        {
            XmlSerializer serializer = new(typeof(SerializableDictionary), new XmlAttributeOverrides() { });
            deserialized = serializer.Deserialize(reader);
        }
        catch 
        {
            configuration = null;
            words = [];
            return false;
        }

        if (deserialized is not SerializableDictionary dictionary)
        {
            configuration = null;
            words = [];
            return false;
        }

        CultureInfo ciFrom;
        CultureInfo ciTo;
        try
        {
            ciFrom = new(dictionary.LanguageFromCultureName);
        }
        catch
        {
            configuration = null;
            words = [];
            return false;
        }

        try
        {
            ciTo = new(dictionary.LanguageToCultureName);
        }
        catch
        {
            configuration = null;
            words = [];
            return false;
        }

        List<DictionaryWord> translations = [];
        foreach (SerializableDictionaryWord item in dictionary.Words)
        {
            if (!Guid.TryParse(item.Id, out Guid id))
            {
                id = Guid.NewGuid();
            }

            translations.Add(new(
                id,
                item.Word,
                new WordTranslation(
                item.ProForms.Select(i =>
                    new WordProForm(
                        i.Index,
                        i.Description,
                        i.Meanings.Select(e =>
                            new WordMeaning(e.Index, e.Meaning, e.Examples)))),
                item.LinkedWords)
                {
                    Description = item.Description,
                })
            );
        }

        words = [.. translations];
        configuration = new(ciFrom, ciTo, dictionary.Authors, dictionary.Description);
        return true;
    }

    public static bool TrySerialize(DictionaryConfiguration configuration, DictionaryWord[] words, StreamWriter writer)
    {
        SerializableDictionary serializable = new()
        {
            LanguageFromCultureName = configuration.SourceLanguage.Name,
            LanguageToCultureName = configuration.TargetLanguage.Name,
            Authors = configuration.Authors,
            Description = configuration.Description,
            Words = [.. words.Select(w => new SerializableDictionaryWord()
            {
                Id = w.Id.ToString(),
                Word = w.Word,
                Description = w.Translation.Description,
                LinkedWords = [.. w.Translation.LinkedWords],
                ProForms = [.. w.Translation.ProForms.Select(p => new SerializableProForm()
                {
                    Index = p.Index,
                    Description = p.Description,
                    Meanings = [.. p.Meanings.Select(e => new SerializableWordMeaning()
                    {
                       Index = e.Index,
                       Meaning = e.Meaning,
                       Examples = [.. e.Examples]
                    })]
                })],
            })]
        };
        try
        {
            XmlWriterSettings settings = new()
            {
                Indent = true,
                OmitXmlDeclaration = true,
            };
            using XmlWriter xmlwriter = XmlWriter.Create(writer, settings);
            XmlSerializer serializer = new(typeof(SerializableDictionary));
            XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
            serializer.Serialize(xmlwriter, serializable, emptyNamespaces);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }
}
