using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VVVV.Packs.VObject
{
    public static class Helper
    {
        private Helper() { }
        public static uint ReadUint(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 4);
            return BitConverter.ToUInt32(tmp, 0);
        }
        public static string ReadASCII(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.ASCII.GetString(tmp);
        }
        public static string ReadUTF8(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.UTF8.GetString(tmp);
        }
        public static string ReadUnicode(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.Unicode.GetString(tmp);
        }

        public static void WriteUint(this Stream input, uint data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteASCII(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.ASCII.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUTF8(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.UTF8.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUnicode(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }

        public static uint ASCIILength(this string s)
        {
            byte[] tmp = System.Text.Encoding.ASCII.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UTF8Length(this string s)
        {
            byte[] tmp = System.Text.Encoding.UTF8.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UnicodeLength(this string s)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(s);
            return (uint)tmp.Length;
        }
    }
}
