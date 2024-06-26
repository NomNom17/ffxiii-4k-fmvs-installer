﻿using System;
using System.IO;
using WhiteBinTools.FilelistClasses;
using WhiteBinTools.SupportClasses;
using static WhiteBinTools.SupportClasses.ProgramEnums;

namespace WhiteBinTools.RepackClasses
{
    public class RepackTypeC
    {
        public static void RepackMultiple(GameCodes gameCode, string filelistFile, string whiteBinFile, string whiteExtractedDir)
        {
            var filelistVariables = new FilelistVariables();
            var repackVariables = new RepackVariables();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFile);

            RepackProcesses.PrepareRepackVars(repackVariables, filelistFile, filelistVariables, whiteExtractedDir);

            filelistVariables.DefaultChunksExtDir.IfDirExistsDel();
            Directory.CreateDirectory(filelistVariables.DefaultChunksExtDir);

            repackVariables.NewChunksExtDir.IfDirExistsDel();
            Directory.CreateDirectory(repackVariables.NewChunksExtDir);

            RepackProcesses.CreateFilelistBackup(filelistFile, repackVariables);

            repackVariables.OldWhiteBinFileBackup = repackVariables.NewWhiteBinFile + ".bak";
            repackVariables.OldWhiteBinFileBackup.IfFileExistsDel();

            Console.WriteLine("Backing up Image bin file....");
            File.Copy(repackVariables.NewWhiteBinFile, repackVariables.OldWhiteBinFileBackup);


            FilelistProcesses.DecryptProcess(gameCode, filelistVariables);

            using (var filelistStream = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelistStream))
                {
                    FilelistChunksPrep.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistChunksPrep.UnpackChunks(filelistStream, filelistVariables.ChunkFile, filelistVariables);
                }
            }


            filelistVariables.ChunkFNameCount = 0;
            repackVariables.LastChunkFileNumber = filelistVariables.TotalChunks - 1;

            for (int ch = 0; ch < filelistVariables.TotalChunks; ch++)
            {
                var filesInChunkCount = FilelistProcesses.GetFilesInChunkCount(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount);

                using (var currentChunkStream = new FileStream(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount, FileMode.Open, FileAccess.Read))
                {
                    using (var chunkStringReader = new BinaryReader(currentChunkStream))
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
                                    if (convertedString == "end")
                                    {
                                        repackVariables.HasEndString = true;
                                        updChunkStringsWriter.Write("end\0");
                                        break;
                                    }

                                    RepackProcesses.GetPackedState(convertedString, repackVariables, whiteExtractedDir);

                                    repackVariables.AsciiFilePos = repackVariables.ConvertedOgStringData[0];
                                    repackVariables.AsciiUnCmpSize = repackVariables.ConvertedOgStringData[1];
                                    repackVariables.AsciiCmpSize = repackVariables.ConvertedOgStringData[2];

                                    // Repack a specific file
                                    var currentFileInProcess = Path.Combine(repackVariables.OgDirectoryPath, repackVariables.OgFileName);
                                    if (File.Exists(Path.Combine(whiteExtractedDir, currentFileInProcess)))
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

                                        Console.WriteLine(repackVariables.RepackState + " " + Path.Combine(repackVariables.NewWhiteBinFileName, repackVariables.RepackLogMsg)  + " " + packedAs);
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


            if (filelistVariables.IsEncrypted)
            {
                File.Delete(filelistFile);
            }

            RepackFilelist.CreateFilelist(filelistVariables, repackVariables, gameCode);

            if (filelistVariables.IsEncrypted)
            {
                FilelistProcesses.EncryptProcess(repackVariables);
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
            }

            Console.WriteLine("\nFinished repacking files into " + repackVariables.NewWhiteBinFileName);
        }
    }
}