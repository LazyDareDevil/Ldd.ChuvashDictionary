using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

[XmlRoot("Meaning")]
public class SerializableWordMeaning
{
    public int Index { get; set; }

    public string Meaning { get; set; } = string.Empty;

    [XmlArrayItem("Example")]
    public string[] Examples { get; set; } = [];
}
