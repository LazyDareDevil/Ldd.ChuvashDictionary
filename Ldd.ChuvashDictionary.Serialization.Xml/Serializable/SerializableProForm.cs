using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

[XmlRoot("ProForm")]
public class SerializableProForm
{
    [XmlAttribute]
    public int Index { get; set; }

    public string Description { get; set; } = string.Empty;

    [XmlArrayItem("Meaning")]
    public SerializableWordMeaning[] Meanings { get; set; } = [];
}
