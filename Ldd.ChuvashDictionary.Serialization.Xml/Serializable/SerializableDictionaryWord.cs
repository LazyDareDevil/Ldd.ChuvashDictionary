using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

public sealed class SerializableDictionaryWord
{
    [XmlElement]
    public string Word { get; set; } = string.Empty;

    [XmlArrayItem("LinkedWord")]
    public string[] LinkedWords { get; set; } = [];

    [XmlArrayItem("Meaning")]
    public SerializableWordMeaning[] Meanings { get; set; } = [];
}
