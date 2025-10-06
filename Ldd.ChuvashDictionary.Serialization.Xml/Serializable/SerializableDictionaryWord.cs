using System.Xml.Serialization;

namespace Ldd.ChuvashDictionary.Serialization.Xml.Serializable;

public sealed class SerializableDictionaryWord
{
    [XmlAttribute]
    public string Id { get; set; } = string.Empty;

    [XmlElement]
    public string Word { get; set; } = string.Empty;

    public SerializableProForm[] ProForms { get; set; } = [];

    public Guid[] LinkedWords { get; set; } = [];

    public string Description { get; set; } = string.Empty;
}
