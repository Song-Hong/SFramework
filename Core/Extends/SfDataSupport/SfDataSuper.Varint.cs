using System;
using System.IO;

namespace SFramework.Core.Extends.SfDataSupport
{
    internal static class SfDataSuperVarint
    {
        public static void WriteU64(Stream s, ulong v)
        {
            while (v >= 0x80)
            {
                s.WriteByte((byte)(v | 0x80));
                v >>= 7;
            }
            s.WriteByte((byte)v);
        }
        public static void WriteS64(Stream s, long v)
        {
            ulong zz = (ulong)((v << 1) ^ (v >> 63));
            WriteU64(s, zz);
        }
        public static ulong ReadU64(Stream s)
        {
            int shift = 0;
            ulong result = 0;
            while (true)
            {
                int b = s.ReadByte();
                if (b < 0) throw new EndOfStreamException();
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }
        public static long ReadS64(Stream s)
        {
            ulong zz = ReadU64(s);
            long v = (long)((zz >> 1) ^ (ulong)-(long)(zz & 1));
            return v;
        }
    }
}
