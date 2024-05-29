using BinaryReaderEx;
using BinaryWriterEx;
using FFXIII4kMovieMod.SupportClasses;
using Ionic.Zip;
using Spectre.Console; // Implemented Spectre Console Namespace.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using WDBtool;
using WhiteBinTools.RepackClasses;
using WhiteBinTools.SupportClasses;
using WhiteBinTools.UnpackClasses;

namespace FFXIII4kMovieMod
{
    internal class MovieHelpers
    {
        public static void InstallMovies(string whitePath, InstallerEnums.VoiceOvers vo, DriveInfo[] drivesArray, int driveLetterIndex, InstallerEnums.PatchTypes patchType)
        {
            Console.Clear();
            Console.WriteLine("");

            var sysDir = Path.Combine(whitePath, "sys");
            var movieDir = Path.Combine(whitePath, "movie");
            var moddedMoviesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "movie_data");


            // Extract ncmp files
            Console.WriteLine("Checking and extracting ncmp files present in 'movie_data' folder....");
            Console.WriteLine("");
            Console.WriteLine("");

            if (!Directory.Exists(moddedMoviesDir))
            {
                InstallerMethods.ErrorExit("The 'movie_data' folder could not be found next to the installer program.");
            }

            // Get the drive letter of the drive
            // where the installer is present
            var installerDirDrive = Directory.GetDirectoryRoot(moddedMoviesDir);
            var installerDirDriveIndex = InstallerMethods.GetDriveIndex(drivesArray, installerDirDrive);

            var ncmpMoviesDir = Directory.GetFiles(moddedMoviesDir, "*.ncmp", SearchOption.TopDirectoryOnly);
            foreach (var ncmp in ncmpMoviesDir)
            {
                // Get the decompressed ncmp
                // file size
                long ncmpUncmpSize = 0;
                using (var zipArchive = new ZipFile(ncmp))
                {
                    foreach (var cmpFile in zipArchive)
                    {
                        ncmpUncmpSize += cmpFile.UncompressedSize;
                    }
                }

                // Determine whether to extract the 
                // ncmp file or not, according to 
                // the space present on the drive
                // where the 'movie_data' folder
                // is present
                var extractSpacePresent = false;
                var extractSpace = drivesArray[installerDirDriveIndex].AvailableFreeSpace;
                if (extractSpace > ncmpUncmpSize)
                {
                    extractSpacePresent = true;
                }
                else
                {
                    extractSpacePresent = false;
                }

                if (extractSpacePresent)
                {
                    Console.WriteLine($"Extracting {Path.GetFileName(ncmp)}....");

                    var ncmpOutDir = Path.Combine(moddedMoviesDir, "_" + Path.GetFileName(ncmp));
                    System.IO.Compression.ZipFile.ExtractToDirectory(ncmp, ncmpOutDir);

                    var moddedBiks = Directory.GetFiles(ncmpOutDir, "*.bik", SearchOption.AllDirectories);
                    foreach (var bik in moddedBiks)
                    {
                        var outBikFile = Path.Combine(moddedMoviesDir, Path.GetFileName(bik));
                        InstallerMethods.IfFileFolderExistsDel(outBikFile, InstallerEnums.DeleteType.file);

                        File.Move(bik, outBikFile);
                    }

                    InstallerMethods.IfFileFolderExistsDel(ncmpOutDir, InstallerEnums.DeleteType.folder);
                    InstallerMethods.IfFileFolderExistsDel(ncmp, InstallerEnums.DeleteType.file);
                }

                Console.WriteLine("");
            }


            // Store all of the movie files extracted
            // from the ncmp files into an array and
            // check if the 'movie_data' folder is
            // empty or not.
            Console.WriteLine("");
            Console.WriteLine("Checking extracted files....");

            var modMovies = Directory.GetFiles(moddedMoviesDir, "*.bik", SearchOption.TopDirectoryOnly);
            if (modMovies.Length == 0)
            {
                InstallerMethods.ErrorExit("There aren't any movie files present in the 'movie_data' folder. please run this installer only after adding the ncmp files into the 'movie_data' folder.");
            }

            //Console.WriteLine("Finished checking extracted files");

            AnsiConsole.MarkupLine("[green]Finished checking extracted files.[/]");

            Console.WriteLine("");
            Console.WriteLine("Proceeding to unpack archive....");
            Thread.Sleep(3000);


            // Unpack the archive file for db file
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("Unpacking archive for db file....");
            var archiveFileSuffix = vo == InstallerEnums.VoiceOvers.us ? "u" : "c";
            var movieFileSuffix = vo == InstallerEnums.VoiceOvers.us ? "_us" : "";

            var filelistFile = Path.Combine(sysDir, $"filelist{archiveFileSuffix}.win32.bin");
            var whiteImgFile = Path.Combine(sysDir, $"white_img{archiveFileSuffix}.win32.bin");
            var unpackedWhiteImgDir = Path.Combine(sysDir, "_" + Path.GetFileName(whiteImgFile));
            var movieItemsDbFile = Path.Combine(unpackedWhiteImgDir, "db", "resident", $"movie_items{movieFileSuffix}.win32.wdb");

            Thread.Sleep(1000);
            Console.WriteLine("");
            UnpackTypeA.UnpackFull(ProgramEnums.GameCodes.ff131, filelistFile, whiteImgFile);


            // If patchType was set to ReAdd, then
            // change the size of the !!string file
            // to ensure only the unmodded wmp ids
            // are present when the db file is
            // unpacked.
            // After this, unpack the db file
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("Unpacking db file....");
            Console.WriteLine("");

            if (patchType.Equals(InstallerEnums.PatchTypes.ReAdd))
            {
                using (var dbUpdater = new FileStream(movieItemsDbFile, FileMode.Open, FileAccess.Write))
                {
                    using (var dbWriter = new BinaryWriter(dbUpdater))
                    {
                        dbWriter.BaseStream.Position = 36;
                        dbWriter.WriteBytesUInt32(90, true);
                    }
                }
            }

            WDB.UnpackWDB(movieItemsDbFile);


            // Get all wmp ids present in the old
            // !!string file and store it in a
            // list
            var unpackedWDBdir = Path.Combine(unpackedWhiteImgDir, "db", "resident", $"_movie_items{movieFileSuffix}.win32.wdb");
            var stringFile = Path.Combine(unpackedWDBdir, "!!string");
            var oldStringFileSize = new FileInfo(stringFile).Length;

            var oldWMPidList = new List<string>();

            using (var oldStringFileStream = new FileStream(stringFile, FileMode.Open, FileAccess.Read))
            {
                using (var oldStringReader = new BinaryReader(oldStringFileStream))
                {
                    uint readPos = 0;
                    while (readPos < oldStringFileSize)
                    {
                        oldStringReader.BaseStream.Position = readPos;
                        oldWMPidList.Add(oldStringReader.ReadStringTillNull());

                        readPos = (uint)oldStringReader.BaseStream.Position;
                    }
                }
            }


            Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("Processing movie files....");
            Console.WriteLine("");
            Console.WriteLine("");


            // Add all of the movie files from the
            // modMovies array to a copyList,
            // according to the selected voiceover
            var movieCopyList = new List<string>();
            foreach (var moddedFile in modMovies)
            {
                switch (vo)
                {
                    case InstallerEnums.VoiceOvers.us:
                        if (moddedFile.EndsWith("_us.bik"))
                        {
                            movieCopyList.Add(moddedFile);
                        }
                        break;

                    case InstallerEnums.VoiceOvers.jpn:
                        if (!moddedFile.EndsWith("_us.bik"))
                        {
                            movieCopyList.Add(moddedFile);
                        }
                        break;
                }
            }


            // Start processing all of the movie
            // files in the copyList
            var copyCounter = 0;
            foreach (var movieFile in movieCopyList)
            {
                var currentMovieSize = (uint)new FileInfo(movieFile).Length;
                var currentMovieName = Path.GetFileNameWithoutExtension(movieFile);
                var currentMovieId = currentMovieName.Replace("_us", "");

                var currentWMPid = "null";
                if (NamesDict.WMPsDict.ContainsValue(currentMovieId))
                {
                    currentWMPid = GetWMP(NamesDict.WMPsDict, currentMovieId);
                }


                // Check for available free space in
                // the drive where the game directory
                // is present to copy the movie file
                // If space is present, then set a 
                // spacePresent bool
                var spacePresent = false;
                var availableFreeSpace = drivesArray[driveLetterIndex].AvailableFreeSpace;
                if (availableFreeSpace > currentMovieSize + 16)
                {
                    spacePresent = true;
                }
                else
                {
                    spacePresent = false;
                }


                // If the wmp id already exists
                // in the db file then set a 
                // copiedAlready bool
                var copiedAlready = false;
                foreach (var oldWMPid in oldWMPidList)
                {
                    if (oldWMPid.Equals(currentWMPid))
                    {
                        copiedAlready = true;
                        break;
                    }
                }


                // If the wmp id is null, then
                // set a invalidWMPid bool
                var invalidWMPid = false;
                if (currentWMPid.Equals("null"))
                {
                    invalidWMPid = true;
                }


                // According to the conditions,
                // determine whether to copy 
                // the copy the file
                var toCopy = false;
                if (spacePresent && !copiedAlready && !invalidWMPid)
                {
                    toCopy = true;
                }


                // Open the string file in a filestream
                // along with a streamwriter
                using (var stringFileStream = new FileStream(stringFile, FileMode.Append, FileAccess.Write))
                {
                    using (var stringFileWriter = new StreamWriter(stringFileStream))
                    {
                        switch (toCopy)
                        {
                            case true:
                                Console.WriteLine($"Processing movie file '{currentMovieName}.bik'....");
                                Console.WriteLine("Determined enough space for this movie file. Copying....");
                                copyCounter++;


                                // Update the the info offsets in
                                // the movie's info file
                                var wmpIdPos = (uint)stringFileStream.Length;
                                stringFileWriter.Write(currentWMPid + "\0");

                                using (var movieRecordStream = new FileStream(Path.Combine(unpackedWDBdir, currentMovieId), FileMode.Open, FileAccess.Write))
                                {
                                    using (var movieRecordWriter = new BinaryWriter(movieRecordStream))
                                    {
                                        movieRecordWriter.BaseStream.Position = 0;
                                        movieRecordWriter.WriteBytesUInt32(wmpIdPos, true);

                                        movieRecordWriter.BaseStream.Position = 4;
                                        movieRecordWriter.WriteBytesUInt32(currentMovieSize, true);

                                        movieRecordWriter.BaseStream.Position = 12;
                                        movieRecordWriter.WriteBytesUInt32(16, true);
                                    }
                                }


                                // Copy the modded movie file into
                                // its associated wmp file
                                var currentWMPfile = Path.Combine(movieDir, currentWMPid + movieFileSuffix + ".win32.wmp");
                                var currentMovieFile = Path.Combine(moddedMoviesDir, currentMovieId + movieFileSuffix + ".bik");

                                InstallerMethods.IfFileFolderExistsDel(currentWMPfile, InstallerEnums.DeleteType.file);

                                using (var wmpHeaderWriter = new StreamWriter(currentWMPfile, true))
                                {
                                    wmpHeaderWriter.Write("WMP\0");
                                    wmpHeaderWriter.Write("Ver:0.01");
                                    wmpHeaderWriter.Write("\0\0\0\0");
                                }

                                using (var wmpStream = new FileStream(currentWMPfile, FileMode.Append, FileAccess.Write))
                                {
                                    using (var movieStream = new FileStream(currentMovieFile, FileMode.Open, FileAccess.Read))
                                    {
                                        movieStream.CopyToWithProgress(wmpStream, currentMovieSize);
                                    }
                                }

                                Console.WriteLine("");
                                break;

                            case false:
                                if (!spacePresent && !copiedAlready && !invalidWMPid)
                                {
                                    Console.WriteLine($"Processing movie file '{currentMovieName}.bik'....");
                                    //Console.WriteLine("Enough space not available for this movie file. skipped to next file.");

                                    AnsiConsole.MarkupLine("[red]Enough space not available for this movie file. skipped to next file.[/]");
                                }

                                Console.WriteLine("");
                                break;
                        }
                    }
                }
            }


            // If no movie files were copied, then set a
            // packArchive bool to not repack the db
            // file into the archive
            var packArchive = true;
            if (copyCounter == 0)
            {
                packArchive = false;
                Console.WriteLine("No movie files were copied");
                Thread.Sleep(2000);
            }
            else
            {
                packArchive = true;
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Finished copying files. Proceeding to repacking db file....");
                Thread.Sleep(2000);
            }


            // According to the packArchive bool,
            // determine whether to repack the
            // db file and then pack
            // the file into the archive
            switch (packArchive)
            {
                case true:
                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("Repacking db file....");

                    WDB.RepackWDB(unpackedWDBdir);
                    Thread.Sleep(1000);

                    Console.WriteLine("");
                    Console.WriteLine("");
                    RepackTypeA.RepackAll(ProgramEnums.GameCodes.ff131, filelistFile, unpackedWhiteImgDir);

                    InstallerMethods.IfFileFolderExistsDel(filelistFile + ".bak", InstallerEnums.DeleteType.file);
                    InstallerMethods.IfFileFolderExistsDel(whiteImgFile + ".bak", InstallerEnums.DeleteType.file);
                    InstallerMethods.IfFileFolderExistsDel(unpackedWhiteImgDir, InstallerEnums.DeleteType.folder);

                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("Finished installing movies");
                    InstallerMethods.DisplayMessageBox("Successfully installed 4k movies into the game files.", "Success", InstallerEnums.MsgBoxIcon.Info);
                    break;

                case false:
                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("Deleting unpacked archive folder....");
                    InstallerMethods.IfFileFolderExistsDel(unpackedWhiteImgDir, InstallerEnums.DeleteType.folder);

                    Console.Clear();
                    Console.WriteLine("");
                    Console.WriteLine("No movies were installed");
                    InstallerMethods.DisplayMessageBox("No movie files were installed into the game files.", "Warning", InstallerEnums.MsgBoxIcon.Warning);
                    break;
            }
        }

        static wmp GetWMP<wmp, movie>(Dictionary<wmp, movie> namesDict, movie movieId)
        {
            wmp wmpId = namesDict.Keys.Last();

            foreach (KeyValuePair<wmp, movie> pairs in namesDict)
            {
                if (pairs.Value.Equals(movieId))
                {
                    wmpId = pairs.Key;
                    break;
                }
            }

            return wmpId;
        }
    }
}