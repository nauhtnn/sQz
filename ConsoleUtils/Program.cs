using sQzLib;

namespace ConsoleUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            Txt p = new Txt();
            string filePath = "../../../samples/GUI-vi.txt";
            if (System.IO.File.Exists(filePath))
                p.Scan(System.IO.File.ReadAllText(filePath));
            else
            {
                System.Console.WriteLine("File not found: " + filePath);
                return;
            }
            filePath = "../../../sQzLib/TxI.cs";
            if (System.IO.File.Exists(filePath))
                p.WriteEnum(filePath);
            else
            {
                System.Console.WriteLine("File not found: " + filePath);
                return;
            }
            filePath = "../../../samples/GUI-vi.bin";
            if (System.IO.File.Exists(filePath))
                p.WriteByte(filePath);
            else
            {
                System.Console.WriteLine("File not found: " + filePath);
                return;
            }
            
            //p.ReadByte(Txt.sRPath + "samples/GUI-vi.bin");
        }
    }
}
