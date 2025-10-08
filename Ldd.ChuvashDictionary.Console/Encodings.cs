using System.Text;

static class Encodings
{
    public static Encoding DataEncoding = new UTF8Encoding(true, false);
    public static Encoding InputEncoding = Encoding.Unicode;
    public static Encoding OutputEncoding = DataEncoding;
}
