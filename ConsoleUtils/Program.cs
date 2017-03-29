using sQzLib;

namespace ConsoleUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            Txt p = new Txt();
            p.Scan(Utils.ReadFile("../../../samples/GUI-vi.txt"));
            p.WriteEnum("../../../sQzLib/TxI.cs");
            p.WriteByte("../../../samples/GUI-vi.bin");
            //p.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
        }
    }
}
