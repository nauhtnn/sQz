using sQzLib;

namespace ConsoleUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            Txt p = new Txt();
            p.Scan(Utils.ReadFile(Txt.sRPath + "samples/GUI-vi.txt"));
            p.WriteEnum(Txt.sRPath + "sQzLib/TxI.cs");
            p.WriteByte(Txt.sRPath + "samples/GUI-vi.bin");
            //p.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
        }
    }
}
