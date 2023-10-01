﻿using System;
using System.IO;
using WhiteBinTools.FilelistClasses;
using WhiteBinTools.SupportClasses;

namespace WhiteBinTools.RepackClasses
{
    public class RepackTypeC
    {
        public static void RepackMultiple(CmnEnums.GameCodes gameCodeVar, string filelistFileVar, string whiteBinFileVar, string whiteExtractedDirVar)
        {
            filelistFileVar.CheckFileExists("Error: Filelist file specified in the argument is missing");
            whiteBinFileVar.CheckFileExists("Error: Image bin file specified in the argument is missing");

            var filelistVariables = new FilelistProcesses();
            var repackVariables = new RepackProcesses();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFileVar);

            RepackProcesses.PrepareRepackVars(repackVariables, filelistFileVar, filelistVariables, whiteExtractedDirVar);

            filelistVariables.DefaultChunksExtDir.IfDirExistsDel();
            Directory.CreateDirectory(filelistVariables.DefaultChunksExtDir);

            repackVariables.NewChunksExtDir.IfDirExistsDel();
            Directory.CreateDirectory(repackVariables.NewChunksExtDir);

            RepackProcesses.CreateFilelistBackup(filelistFileVar, repackVariables);

            repackVariables.OldWhiteBinFileBackup = repackVariables.NewWhiteBinFile + ".bak";
            repackVariables.OldWhiteBinFileBackup.IfFileExistsDel();

            Console.WriteLine("Backing up Image bin file....");
            File.Copy(repackVariables.NewWhiteBinFile, repackVariables.OldWhiteBinFileBackup);


            FilelistProcesses.DecryptProcess(gameCodeVar, filelistVariables);

            using (var filelist = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelist))
                {
                    FilelistProcesses.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistProcesses.UnpackChunks(filelist, filelistVariables.ChunkFile, filelistVariables);
                }
            }


            filelistVariables.ChunkFNameCount = 0;
            repackVariables.LastChunkFileNumber = 0;
            for (int ch = 0; ch < filelistVariables.TotalChunks; ch++)
            {
                var filesInChunkCount = FilelistProcesses.GetFilesInChunkCount(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount);

                using (var currentChunk = new FileStream(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount, FileMode.Open, FileAccess.Read))
                {
                    using (var chunkStringReader = new BinaryReader(currentChunk))
                    {

                        using (var updChunkStrings = new FileStream(repackVariables.NewChunkFile + filelistVariables.ChunkFNameCount, FileMode.Append, FileAccess.Write))
                        {
                            using (var updChunkStringsWriter = new StreamWriter(updChunkStrings))
                            {

                                var chunkStringReaderPos = (uint)0;
                                var packedAs = "";
                                for (int f = 0; f < filesInChunkCount; f++)
                                {
                                    var convertedString = chunkStringReader.BinaryToString(chunkStringReaderPos);
                                    if (convertedString.Equals("end"))
                                    {
                                        updChunkStringsWriter.Write("end\0");
                                        repackVariables.LastChunkFileNumber = filelistVariables.ChunkFNameCount;
                                        break;
                                    }

                                    RepackProcesses.GetPackedState(convertedString, repackVariables, whiteExtractedDirVar);

                                    repackVariables.AsciiFilePos = repackVariables.ConvertedOgStringData[0];
                                    repackVariables.AsciiUnCmpSize = repackVariables.ConvertedOgStringData[1];
                                    repackVariables.AsciiCmpSize = repackVariables.ConvertedOgStringData[2];

                                    // Repack a specific file
                                    var currentFileInProcess = repackVariables.OgDirectoryPath + "\\" + repackVariables.OgFileName;
                                    if (File.Exists(whiteExtractedDirVar + "\\" + currentFileInProcess))
                                    {
                                        switch (repackVariables.WasCompressed)
                                        {
                                            case true:
                                                RepackProcesses.CleanOldFile(repackVariables.NewWhiteBinFile, repackVariables.OgFilePos, repackVariables.OgCmpSize);

                                                var zlibTmpCmpData = repackVariables.OgFullFilePath.ZlibCompress();
                                                var zlibCmpFileSize = (uint)zlibTmpCmpData.Length;

                                                if (zlibCmpFileSize < repackVariables.OgCmpSize || zlibCmpFileSize == repackVariables.OgCmpSize)
                                                {
                                                    RepackProcesses.InjectProcess(repackVariables, ref packedAs);
                                                }
                                                else
                                                {
                                                    RepackProcesses.AppendProcess(repackVariables, ref packedAs);
                                                }
                                                break;

                                            case false:
                                                RepackProcesses.CleanOldFile(repackVariables.NewWhiteBinFile, repackVariables.OgFilePos, repackVariables.OgUnCmpSize);

                                                var dummyFileSize = (uint)new FileInfo(repackVariables.OgFullFilePath).Length;

                                                if (dummyFileSize < repackVariables.OgUnCmpSize || dummyFileSize == repackVariables.OgUnCmpSize)
                                                {
                                                    RepackProcesses.InjectProcess(repackVariables, ref packedAs);
                                                }
                                                else
                                                {
                                                    RepackProcesses.AppendProcess(repackVariables, ref packedAs);
                                                }
                                                break;
                                        }

                                        Console.WriteLine(repackVariables.RepackState + " " + repackVariables.NewWhiteBinFileName + "\\" + repackVariables.RepackLogMsg + " " + packedAs);
                                    }

                                    updChunkStringsWriter.Write(repackVariables.AsciiFilePos + ":");
                                    updChunkStringsWriter.Write(repackVariables.AsciiUnCmpSize + ":");
                                    updChunkStringsWriter.Write(repackVariables.AsciiCmpSize + ":");
                                    updChunkStringsWriter.Write(repackVariables.RepackPathInChunk + "\0");

                                    chunkStringReaderPos = (uint)chunkStringReader.BaseStream.Position;
                                }
                            }
                        }
                    }
                }

                filelistVariables.ChunkFNameCount++;
            }

            filelistVariables.DefaultChunksExtDir.IfDirExistsDel();


            if (filelistVariables.IsEncrypted.Equals(true))
            {
                File.Delete(filelistFileVar);
            }

            RepackProcesses.CreateFilelist(filelistVariables, repackVariables, gameCodeVar);

            if (filelistVariables.IsEncrypted.Equals(true))
            {
                FilelistProcesses.EncryptProcess(repackVariables);
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
            }

            Console.WriteLine("\nFinished repacking files into " + repackVariables.NewWhiteBinFileName);
        }
    }
}