﻿using System;
using System.IO;

namespace BinaryWriterEx
{
    internal static class WriterHelpers
    {
        public static void WriteBytesUInt32(this BinaryWriter writerName, uint valueToWrite, bool isBigEndian)
        {
            var writeValueBuffer = BitConverter.GetBytes(valueToWrite);
            ReverseIfBigEndian(isBigEndian, writeValueBuffer);

            writerName.Write(writeValueBuffer);
        }

        static void ReverseIfBigEndian(bool isBigEndian, byte[] writeValueBuffer)
        {
            if (isBigEndian)
            {
                Array.Reverse(writeValueBuffer);
            }
        }
    }
}