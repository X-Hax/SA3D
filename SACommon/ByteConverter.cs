﻿using System;
using System.Diagnostics;
using System.Text;

namespace SATools.SACommon
{
    /// <summary>
    /// Converts between numbers and bytes
    /// </summary>
    [DebuggerNonUserCode]
    public static class ByteConverter
    {
        public static bool BigEndian { get; set; }
        public static bool Reverse { get; set; }

        public static byte[] GetBytes(this ushort value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this short value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this uint value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[3], y[2], y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this int value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[3], y[2], y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this ulong value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this long value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this float value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[3], y[2], y[1], y[0] };
            return y;
        }

        public static byte[] GetBytes(this double value)
        {
            byte[] y = BitConverter.GetBytes(value);
            if(BigEndian)
                y = new byte[] { y[7], y[6], y[5], y[4], y[3], y[2], y[1], y[0] };
            return y;
        }

        public static ushort ToUInt16(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex + 1], value[startIndex] }
                : new byte[] { value[startIndex], value[++startIndex] };
            return BitConverter.ToUInt16(y, 0);
        }

        public static short ToInt16(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex + 1], value[startIndex] }
                : new byte[] { value[startIndex], value[++startIndex] };
            return BitConverter.ToInt16(y, 0);
        }

        public static uint ToUInt32(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToUInt32(y, 0);
        }

        public static int ToInt32(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToInt32(y, 0);
        }

        public static ulong ToUInt64(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToUInt64(y, 0);
        }

        public static long ToInt64(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToInt64(y, 0);
        }

        public static float ToSingle(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToSingle(y, 0);
        }

        public static double ToDouble(this byte[] value, uint startIndex)
        {
            byte[] y = BigEndian
                ? new byte[] { value[startIndex += 3], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex], value[--startIndex] }
                : new byte[] { value[startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex], value[++startIndex] };
            return BitConverter.ToDouble(y, 0);
        }

        public static string GetCString(this byte[] file, uint address, Encoding encoding, uint count)
        {
            return encoding.GetString(file, (int)address, (int)count);
        }

        public static string GetCString(this byte[] file, uint address, uint count)
            => file.GetCString(address, Encoding.UTF8, count);

        public static string GetCString(this byte[] file, uint address, Encoding encoding)
        {
            int count = 0;
            while(file[address + count] != 0)
                count++;
            return encoding.GetString(file, (int)address, count);
        }

        public static string GetCString(this byte[] file, uint address)
            => file.GetCString(address, Encoding.UTF8);

        public static uint GetPointer(this byte[] file, uint address, uint imageBase)
        {
            uint tmp = file.ToUInt32(address);
            return tmp == 0 ? 0 : tmp - imageBase;
        }

        public static bool CheckBigEndianInt16(this byte[] file, uint address)
        {
            bool bigEndState = BigEndian;

            BigEndian = false;
            uint little = file.ToUInt16(address);
            BigEndian = true;
            uint big = file.ToUInt16(address);

            BigEndian = bigEndState;

            return little > big;
        }

        public static bool CheckBigEndianInt32(this byte[] file, uint address)
        {
            bool bigEndState = BigEndian;

            BigEndian = false;
            uint little = file.ToUInt32(address);
            BigEndian = true;
            uint big = file.ToUInt32(address);

            BigEndian = bigEndState;

            return little > big;
        }
    }
}
