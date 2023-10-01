using BinaryReaderEx;
using BinaryWriterEx;
using StreamExtension;
using System;
using System.IO;

namespace WDBtool
{
    public partial class WDB
    {
        public static void RepackWDB(string inWDBextractedDir)
        {
            if (!Directory.Exists(inWDBextractedDir))
            {
                CmnMethods.ErrorExit($"Error: Missing specified {Path.GetFileName(inWDBextractedDir)} directory.");
            }

            var inWDBextractedDirName = Path.GetDirectoryName(inWDBextractedDir);

            var outWDBfileName = Path.GetFileName(inWDBextractedDir);
            outWDBfileName = outWDBfileName.Remove(0, 1);

            var outWDBfile = Path.Combine(inWDBextractedDirName, outWDBfileName);
            var recordsListFile = Path.Combine(inWDBextractedDir, CmnMethods.RecordsList);

            if (!File.Exists(recordsListFile))
            {
                CmnMethods.ErrorExit($"Error: Missing file '{CmnMethods.RecordsList}' in extracted directory. Please ensure that the wdb file is unpacked properly.");
            }

            if (File.Exists(outWDBfile))
            {
                File.Move(outWDBfile, outWDBfile + ".old");
            }


            using (var recordsList = new FileStream(recordsListFile, FileMode.Open, FileAccess.Read))
            {
                using (var recordsListReader = new BinaryReader(recordsList))
                {
                    recordsListReader.BaseStream.Position = 0;
                    var totalRecords = recordsListReader.ReadUInt32();
                    Console.WriteLine("");


                    // Write all record names into
                    // the new wdb file
                    using (var outWDBrecordsStream = new FileStream(outWDBfile, FileMode.Append, FileAccess.Write))
                    {
                        using (var outWDBrecordsWriter = new StreamWriter(outWDBrecordsStream))
                        {
                            outWDBrecordsWriter.Write("WPD");
                            PadNullBytes(outWDBrecordsWriter, 13);


                            uint recordsListReadPos = 4;
                            for (int r = 0; r < totalRecords; r++)
                            {
                                recordsListReader.BaseStream.Position = recordsListReadPos;
                                var currentRecordName = recordsListReader.ReadStringTillNull();
                                recordsListReadPos = (uint)recordsListReader.BaseStream.Position;

                                outWDBrecordsWriter.Write(currentRecordName);
                                uint bytesToPad = 16 - (uint)currentRecordName.Length;
                                PadNullBytes(outWDBrecordsWriter, bytesToPad);
                                PadNullBytes(outWDBrecordsWriter, 16);
                            }
                        }
                    }


                    uint recordDataStartPos = 0;

                    using (var outWDBdataStream = new FileStream(outWDBfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using (var outWDBoffsetStream = new FileStream(outWDBfile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            using (var outWDBoffsetReader = new BinaryReader(outWDBoffsetStream))
                            {
                                using (var outWDBoffsetWriter = new BinaryWriter(outWDBoffsetStream))
                                {
                                    outWDBoffsetWriter.BaseStream.Position = 4;
                                    outWDBoffsetWriter.WriteBytesUInt32(totalRecords, true);


                                    uint readStartPos = 16;
                                    uint writeStartPos = 32;
                                    for (int o = 0; o < totalRecords; o++)
                                    {
                                        outWDBoffsetReader.BaseStream.Position = readStartPos;
                                        var currentRecordName = outWDBoffsetReader.ReadStringTillNull();

                                        recordDataStartPos = (uint)outWDBdataStream.Length;
                                        outWDBoffsetWriter.BaseStream.Position = writeStartPos;
                                        outWDBoffsetWriter.WriteBytesUInt32(recordDataStartPos, true);

                                        var currentFile = Path.Combine(inWDBextractedDir, currentRecordName);
                                        var currentFileSize = (uint)new FileInfo(currentFile).Length;

                                        outWDBoffsetWriter.BaseStream.Position = writeStartPos + 4;
                                        outWDBoffsetWriter.WriteBytesUInt32(currentFileSize, true);

                                        using (var currentFileStream = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
                                        {
                                            currentFileStream.ExCopyTo(outWDBdataStream, 0, currentFileSize);
                                        }

                                        var currentPos = outWDBdataStream.Length;
                                        if (currentPos % 4 != 0)
                                        {
                                            var remainder = currentPos % 4;
                                            var increaseBytes = 4 - remainder;
                                            var newPos = currentPos + increaseBytes;
                                            var nullBytesAmount = newPos - currentPos;

                                            outWDBdataStream.Seek(currentPos, SeekOrigin.Begin);
                                            for (int p = 0; p < nullBytesAmount; p++)
                                            {
                                                outWDBdataStream.WriteByte(0);
                                            }
                                        }

                                        Console.WriteLine("Repacked " + currentRecordName);

                                        recordDataStartPos += currentFileSize;
                                        readStartPos += 32;
                                        writeStartPos += 32;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Finished repacking files to " + outWDBfileName);
        }


        static void PadNullBytes(StreamWriter streamName, uint padding)
        {
            for (int b = 0; b < padding; b++)
            {
                streamName.Write("\0");
            }
        }
    }
}