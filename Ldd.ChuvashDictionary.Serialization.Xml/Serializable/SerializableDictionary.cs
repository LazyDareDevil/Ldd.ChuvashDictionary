using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

[XmlRoot("Dictionary")]
public sealed class SerializableDictionary
{
    [XmlAttribute]
    public string LanguageFromCultureName { get; set; } = string.Empty;

    [XmlAttribute]
    public string LanguageToCultureName { get; set; } = string.Empty;

    [XmlArrayItem("Word")]
    public SerializableDictionaryWord[] Words { get; set; } = [];
}
