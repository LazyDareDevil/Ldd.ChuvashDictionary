using Ldd.ChuvashDictionary.Domain;
using Ldd.ChuvashDictionary.Serialization.Xml.Serializable;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml;

public static class XmlDictionarySerializer
{
    public static TranslationDictionary? Deserialize(StreamReader reader)
    {
        object? deserialized;
        try
        {
            XmlSerializer serializer = new(typeof(SerializableDictionary), new XmlAttributeOverrides() { });
            deserialized = serializer.Deserialize(reader);
        }
        catch 
        {
            return null;
        }

        if (deserialized is not SerializableDictionary dictionary)
        {
            return null;
        }

        CultureInfo ciFrom;
        CultureInfo ciTo;
        try
        {
            ciFrom = new(dictionary.LanguageFromCultureName);
        }
        catch
        {
            return null;
        }

        try
        {
            ciTo = new(dictionary.LanguageToCultureName);
        }
        catch
        {
            return null;
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
                item.ProForms.Select(i =>
                    new WordProForm(
                        i.Index,
                        i.Description,
                        i.Meanings.Select(e =>
                            new WordMeaning(e.Index, e.Meaning, e.Examples)))),
                item.LinkedWords)
            {
                Description = item.Description,
            });
        }

        return new(ciFrom, ciTo, translations);
    }

    public static bool TrySerialize(TranslationDictionary dictionary, StreamWriter writer)
    {
        XmlSerializerNamespaces emptyNamespaces = new([XmlQualifiedName.Empty]);
        XmlWriterSettings settings = new()
        {
            Indent = true,
            OmitXmlDeclaration = true,
        };
        SerializableDictionary serializable = new()
        {
            LanguageFromCultureName = dictionary.SourceLanguage.Name,
            LanguageToCultureName = dictionary.TargetLanguage.Name,
            Words = [.. dictionary.Words.Select(w => new SerializableDictionaryWord()
            {
                Id = w.Id.ToString(),
                Word = w.Word,
                Description = w.Description,
                LinkedWords = [.. w.LinkedWords],
                ProForms = [.. w.ProForms.Select(p => new SerializableProForm()
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
            XmlSerializer serializer = new(typeof(SerializableDictionary));
            using XmlWriter xmlwriter = XmlWriter.Create(writer, settings);
            serializer.Serialize(writer, serializable, emptyNamespaces);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
