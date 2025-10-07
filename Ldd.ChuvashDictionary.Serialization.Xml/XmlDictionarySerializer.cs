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

        return new(ciFrom, ciTo, translations);
    }

    public static bool TrySerialize(TranslationDictionary dictionary, StreamWriter writer)
    {
        SerializableDictionary serializable = new()
        {
            LanguageFromCultureName = dictionary.SourceLanguage.Name,
            LanguageToCultureName = dictionary.TargetLanguage.Name,
            Words = [.. dictionary.Words.Select(w => new SerializableDictionaryWord()
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
