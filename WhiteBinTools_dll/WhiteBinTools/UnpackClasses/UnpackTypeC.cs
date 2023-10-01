﻿using System;
using System.IO;
using WhiteBinTools.FilelistClasses;
using WhiteBinTools.SupportClasses;

namespace WhiteBinTools.UnpackClasses
{
    public class UnpackTypeC
    {
        public static void UnpackFilelistPaths(CmnEnums.GameCodes gameCodeVar, string filelistFileVar)
        {
            filelistFileVar.CheckFileExists("Error: Filelist file specified in the argument is missing");

            var filelistVariables = new FilelistProcesses();

            FilelistProcesses.PrepareFilelistVars(filelistVariables, filelistFileVar);

            var filelistOutName = Path.GetFileName(filelistFileVar);
            filelistVariables.DefaultChunksExtDir = filelistVariables.MainFilelistDirectory + "\\_chunks";
            filelistVariables.ChunkFile = filelistVariables.DefaultChunksExtDir + "\\chunk_";
            var outChunkFile = filelistVariables.MainFilelistDirectory + "\\" + filelistOutName + ".txt";


            filelistVariables.DefaultChunksExtDir.IfDirExistsDel();
            Directory.CreateDirectory(filelistVariables.DefaultChunksExtDir);

            outChunkFile.IfFileExistsDel();


            FilelistProcesses.DecryptProcess(gameCodeVar, filelistVariables);

            using (var filelist = new FileStream(filelistVariables.MainFilelistFile, FileMode.Open, FileAccess.Read))
            {
                using (var filelistReader = new BinaryReader(filelist))
                {
                    FilelistProcesses.GetFilelistOffsets(filelistReader, filelistVariables);
                    FilelistProcesses.UnpackChunks(filelist, filelistVariables.ChunkFile, filelistVariables);
                }
            }

            if (filelistVariables.IsEncrypted.Equals(true))
            {
                filelistVariables.TmpDcryptFilelistFile.IfFileExistsDel();
                filelistVariables.MainFilelistFile = filelistFileVar;
            }


            // Write all file paths strings
            // to a text file
            filelistVariables.ChunkFNameCount = 0;
            for (int cf = 0; cf < filelistVariables.TotalChunks; cf++)
            {
                var filesInChunkCount = FilelistProcesses.GetFilesInChunkCount(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount);

                // Open a chunk file for reading
                using (var currentChunk = new FileStream(filelistVariables.ChunkFile + filelistVariables.ChunkFNameCount, FileMode.Open, FileAccess.Read))
                {
                    using (var chunkStringReader = new BinaryReader(currentChunk))
                    {

                        using (var outChunk = new FileStream(outChunkFile, FileMode.Append, FileAccess.Write))
                        {
                            using (var outChunkWriter = new StreamWriter(outChunk))
                            {

                                var chunkStringReaderPos = (uint)0;
                                for (int f = 0; f < filesInChunkCount; f++)
                                {
                                    var convertedString = chunkStringReader.BinaryToString(chunkStringReaderPos);

                                    outChunkWriter.WriteLine(convertedString);

                                    chunkStringReaderPos = (uint)chunkStringReader.BaseStream.Position;
                                }
                            }
                        }
                    }
                }

                filelistVariables.ChunkFNameCount++;
            }

            Directory.Delete(filelistVariables.DefaultChunksExtDir, true);

            Console.WriteLine("\nExtracted filepaths to " + filelistOutName + ".txt file");
        }
    }
}