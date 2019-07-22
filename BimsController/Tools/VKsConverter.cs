using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BimsController.Tools
{
    public static class VKsConverter
    {
        private static Dictionary<char,int> keysConvertor = new Dictionary<char, int>();
        public static int TAB_VK = 0x09;
        public static int SHIFT_VK = 0xA0;
        public static int ENTER_VK = 0x0D;
        private static void FillConverter()
        {
            keysConvertor = new Dictionary<char, int>();

            keysConvertor.Add('0', 0x30);
            keysConvertor.Add('1', 0x31);
            keysConvertor.Add('2', 0x32);
            keysConvertor.Add('3', 0x33);
            keysConvertor.Add('4', 0x34);
            keysConvertor.Add('5', 0x35);
            keysConvertor.Add('6', 0x36);
            keysConvertor.Add('7', 0x37);
            keysConvertor.Add('8', 0x38);
            keysConvertor.Add('9', 0x39);
            keysConvertor.Add('A', 0x41);
            keysConvertor.Add('B', 0x42);
            keysConvertor.Add('C', 0x43);
            keysConvertor.Add('D', 0x44);
            keysConvertor.Add('E', 0x45);
            keysConvertor.Add('F', 0x46);
            keysConvertor.Add('G', 0x47);
            keysConvertor.Add('H', 0x48);
            keysConvertor.Add('J', 0x4A);
            keysConvertor.Add('K', 0x4B);
            keysConvertor.Add('I', 0x49);
            keysConvertor.Add('L', 0x4C);
            keysConvertor.Add('M', 0x4D);
            keysConvertor.Add('N', 0x4E);
            keysConvertor.Add('O', 0x4F);
            keysConvertor.Add('P', 0x50);
            keysConvertor.Add('Q', 0x51);
            keysConvertor.Add('R', 0x52);
            keysConvertor.Add('S', 0x53);
            keysConvertor.Add('U', 0x55);
            keysConvertor.Add('T', 0x54);
            keysConvertor.Add('V', 0x56);
            keysConvertor.Add('W', 0x57);
            keysConvertor.Add('X', 0x58);
            keysConvertor.Add('Y', 0x59);
            keysConvertor.Add('Z', 0x5A);
            keysConvertor.Add('.', 0xBE);
            keysConvertor.Add(',', 0xBC);
        }

        public static List<int> Convert(string str)
        {
            if (keysConvertor.Count == 0)
                FillConverter();

            List<int> VKs = new List<int>();

            for (int i = 0; i < str.Length; i++)
            {
                VKs.Add(keysConvertor[Char.ToUpper(str[i])]);
            }

            return VKs;
        }
    }
}
