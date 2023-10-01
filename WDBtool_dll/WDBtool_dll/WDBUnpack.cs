using BinaryReaderEx;
using BinaryWriterEx;
using StreamExtension;
using System;
using System.IO;
using System.Text;

namespace WDBtool
{
    public partial class WDB
    {
        public static void UnpackWDB(string inWDBfile)
        {
            if (!File.Exists(inWDBfile))
            {
                CmnMethods.ErrorExit($"Error: Missing specified {Path.GetFileName(inWDBfile)} file.");
            }

            var wdbFileName = Path.GetFileName(inWDBfile);
            var wdbFileDir = Path.GetDirectoryName(inWDBfile);
            var extractWdbDir = Path.Combine(wdbFileDir, "_" + wdbFileName);

            DeleteDirIfExists(extractWdbDir);
            Directory.CreateDirectory(extractWdbDir);


            using (var wdbStream = new FileStream(inWDBfile, FileMode.Open, FileAccess.Read))
            {
                using (var wdbReader = new BinaryReader(wdbStream))
                {
                    wdbReader.BaseStream.Position = 0;
                    var wdbChars = wdbReader.ReadBytes(4);
                    var wdbHeader = Encoding.ASCII.GetString(wdbChars).Replace("\0", "");

                    if (!wdbHeader.Equals("WPD"))
                    {
                        CmnMethods.ErrorExit("Error: Not a valid WDB file");
                    }

                    wdbReader.BaseStream.Position = 4;
                    var totalRecords = wdbReader.ReadBytesUInt32(true);

                    Console.WriteLine("Writing record list....");
                    WriteRecordList(totalRecords, wdbReader, extractWdbDir);
                    Console.WriteLine("");


                    uint readStartPos = 16;
                    for (int f = 0; f < totalRecords; f++)
                    {
                        wdbReader.BaseStream.Position = readStartPos;
                        var currentRecordName = wdbReader.ReadStringTillNull();

                        wdbReader.BaseStream.Position = readStartPos + 16;
                        var currentRecordStart = wdbReader.ReadBytesUInt32(true);

                        wdbReader.BaseStream.Position = readStartPos + 20;
                        var currentRecordSize = wdbReader.ReadBytesUInt32(true);

                        var currentOutFile = Path.Combine(extractWdbDir, currentRecordName);
                        Console.WriteLine("Extracted " + currentRecordName);

                        using (var ofs = new FileStream(currentOutFile, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            wdbStream.ExCopyTo(ofs, currentRecordStart, currentRecordSize);
                        }

                        readStartPos += 32;
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Finished extracting " + wdbFileName);
        }


        static void DeleteDirIfExists(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
            }
        }


        static void WriteRecordList(uint totalRecords, BinaryReader readerName, string extractWdbDir)
        {
            using (var fs = new FileStream(Path.Combine(extractWdbDir, CmnMethods.RecordsList), FileMode.Append, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.BaseStream.Position = 0;
                    bw.WriteBytesUInt32(totalRecords, false);

                    using (var sw = new StreamWriter(fs))
                    {

                        uint readStartPos = 16;
                        for (int r = 0; r < totalRecords; r++)
                        {
                            readerName.BaseStream.Position = readStartPos;
                            sw.Write(readerName.ReadStringTillNull());
                            sw.Write("\0");

                            readStartPos += 32;
                        }
                    }
                }
            }
        }
    }
}