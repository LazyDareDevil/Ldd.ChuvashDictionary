using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

[XmlRoot("Dictionary")]
public sealed class SerializableDictionary
{
    [XmlAttribute]
    public string LanguageFromCultureName { get; set; } = string.Empty;

    [XmlAttribute]
    public string LanguageToCultureName { get; set; } = string.Empty;

    [XmlElement]
    public string Description { get; set; } = string.Empty;

    [XmlArrayItem("Author")]
    public string[] Authors { get; set; } = [];

    [XmlArrayItem("Word")]
    public SerializableDictionaryWord[] Words { get; set; } = [];
}
