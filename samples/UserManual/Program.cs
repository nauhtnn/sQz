using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        const string RAW_FILE_PATH = "CodePage437_wikipedia.txt";
        const string TABLE_FILE_PATH = "cp437_unicode.txt";
        const string SQL_FILE_PATH = "SQL.txt";
        const string DECODED_SQL_FILE_PATH = "SQL_decoded.txt";
        static Dictionary<int, int> cp437_unicode = new Dictionary<int, int>();
        static void Main(string[] args)
        {
			// CreateCP437Table();
            ReadCP437Table();
            DecodeSQL();
        }

        static void ReadCP437Table()
        {
            if (!System.IO.File.Exists(TABLE_FILE_PATH))
            {
                System.Console.WriteLine("File not found: " + TABLE_FILE_PATH);
                return;
            }
            cp437_unicode.Clear();
            string[] values_keys = System.IO.File.ReadAllLines(TABLE_FILE_PATH);
            foreach(string s in values_keys)
            {
                string[] value_key = s.Split('\t');
                cp437_unicode.Add(int.Parse(value_key[1], System.Globalization.NumberStyles.HexNumber),
                    int.Parse(value_key[0], System.Globalization.NumberStyles.HexNumber));
            }
        }

        static void DecodeSQL()
        {
            if (!System.IO.File.Exists(SQL_FILE_PATH))
            {
                System.Console.WriteLine("File not found: " + SQL_FILE_PATH);
                return;
            }
            byte[] cp437_bytes = System.IO.File.ReadAllBytes(SQL_FILE_PATH);
            int[] utf16_bytes = new int[(cp437_bytes.Length - 2) / 2];
            for(int i = 2, j = 0; i < cp437_bytes.Length - 1; ++i, ++i, ++j)
                utf16_bytes[j] = cp437_bytes[i] + (cp437_bytes[i + 1] << 8);

            byte[] decoded = new byte[utf16_bytes.Length];
            for (int i = 0; i < decoded.Length; ++i)
            {
                if (cp437_unicode.ContainsKey(utf16_bytes[i]))
				{
					if(cp437_unicode[utf16_bytes[i]] > 0xff)
					{
						System.Console.WriteLine("Error code: " + cp437_unicode[utf16_bytes[i]]);
						return;
					}
					decoded[i] = (byte)cp437_unicode[utf16_bytes[i]];
				}
                else
				{
					if(utf16_bytes[i] > 0xff)
					{
						System.Console.WriteLine("Error code: " + utf16_bytes[i]);
						return;
					}
					decoded[i] = (byte)utf16_bytes[i];
				}
            }
            // List<byte> decoded_bytes = new List<byte>();
            // int k = 0;
            // foreach(int code in decoded)
            // {
                // decoded_bytes.Add((byte)(code & 0xff));
                // if((code & 0xff00) > 0)
                    // decoded_bytes.Add((byte)(code & 0xff00));
            // }
            System.IO.File.WriteAllBytes(DECODED_SQL_FILE_PATH, decoded);// decoded_bytes.ToArray());
        }

        static void CreateCP437Table()
        {
            if (!System.IO.File.Exists(RAW_FILE_PATH))
            {
                System.Console.WriteLine("File not found: " + RAW_FILE_PATH);
                return;
            }
            string rawTable = System.IO.File.ReadAllText(RAW_FILE_PATH);
            MatchCollection matches = Regex.Matches(rawTable, "[0-9A-F]_");
            if(matches.Count != 16)
            {
                System.Console.WriteLine("Matches count = " + matches.Count);
                foreach (Match m in matches)
                    System.Console.Write(m.Value + " ");
                return;
            }
            List<string> _16codesSegments = new List<string>();
            for (int i = 0; i < matches.Count - 1; ++i)
                _16codesSegments.Add(rawTable.Substring(matches[i].Index, matches[i + 1].Index - matches[i].Index));
            char index = '0';
            cp437_unicode.Clear();
            foreach (string _16codes in _16codesSegments)
            {
                if (index == '9' + 1)
                    index = 'A';
                if(!_16codes.StartsWith(index.ToString() + "_"))
                {
                    System.Console.WriteLine("Error in \"start with\" : " + _16codes.Substring(0, (_16codes.Length > 2) ? 2 : _16codes.Length));
                    return;
                }
                List<string> rawCodes = _16codes.Split('\t').ToList();
                if(rawCodes.Count != 17)
                {
                    System.Console.WriteLine("Error in \"Raw codes length\" = " + rawCodes.Count);
                    System.Console.WriteLine(rawCodes);
                    return;
                }
                char subIndex = '0';
                rawCodes.RemoveAt(0);
                foreach(string rawCode in rawCodes)
                {
                    string[] codes = rawCode.Split('\n');
                    if(codes.Length < 3)
                    {
                        System.Console.WriteLine("Error in rawCode length = " + rawCode);
                        return;
                    }
                    if (subIndex == '9' + 1)
                        subIndex = 'A';

                    int key = Convert.ToInt32(index.ToString() + subIndex, 16);
                    string s = codes[1].Trim();
                    //int value = Convert.ToInt32(s);
                    int value = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                    cp437_unicode.Add(key, value);
                    ++subIndex;
                }
                ++index;
            }

            if(cp437_unicode.Count > 0)
            {
                StringBuilder table = new StringBuilder();
                foreach (KeyValuePair<int, int> p in cp437_unicode)
                    table.Append(p.Key.ToString("X") + '\t' + p.Value.ToString("X") + '\n');
                System.IO.File.WriteAllText(TABLE_FILE_PATH, table.ToString());
            }
        }
    }
}
