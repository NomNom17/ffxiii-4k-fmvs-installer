using FFXIII4kMovieMod.SupportClasses;
using System;
using System.IO;
using System.Threading;

namespace FFXIII4kMovieMod
{
    internal class Core
    {
        static void Main()
        {
            Console.WriteLine("");
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.WriteLine("                  FFXIII - 4K Remastered FMVs with PS3 Audio");
            Console.WriteLine("");
            Console.WriteLine("                             Mod by: NomNom");
            Console.WriteLine("                             Installer written by: Surihix");
            Console.WriteLine("------------------------------------------------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("");


            try
            {
                // Set Game path
                InstallerMethods.DisplayMessageBox("Select the 'FFXiiiLauncher.exe' file present in the FINAL FANTASY XIII game folder.", "Locate launcher", InstallerEnums.MsgBoxIcon.Info);
                var whitePath = InstallerMethods.SetPath("FFXiiiLauncher.exe");

                if (whitePath.Equals(""))
                {
                    InstallerMethods.ErrorExit("Path selection cancelled or a valid path was not selected.");
                }

                whitePath = Path.Combine(Path.GetDirectoryName(whitePath), "white_data");
                if (!Directory.Exists(whitePath))
                {
                    InstallerMethods.ErrorExit("Selected file path is not a valid FINAL FANTASY XIII game directory.");
                }


                // Set voiceovers to patch
                Console.WriteLine("Select the voiceover with which you are playing the game:");
                Console.WriteLine("");
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("Press 'u' key and then 'ENTER' key for English voiceover");
                Console.WriteLine("Press 'j' key and then 'ENTER' key for Japanese voiceover");
                Console.WriteLine("Press 'x' key and then 'ENTER' key to exit this installer");
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine("Your Input:");
                var setVO = Console.ReadLine();
                var vo = new InstallerEnums.VoiceOvers();
                switch (setVO)
                {
                    case "u":
                    case "U":
                        vo = InstallerEnums.VoiceOvers.us;
                        break;

                    case "j":
                    case "J":
                        vo = InstallerEnums.VoiceOvers.jpn;
                        break;

                    case "x":
                    case "X":
                        Console.WriteLine("");
                        Console.WriteLine("Exiting....");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("");
                        InstallerMethods.ErrorExit("Input key pressed was invalid");
                        break;
                }

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");


                // Set Patch type
                Console.WriteLine("Would you like to Add or ReAdd movies?");
                Console.WriteLine("");
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("Press 'a' key and then 'ENTER' key for Adding movies");
                Console.WriteLine("Press 'r' key and then 'ENTER' key for ReAdding movies");
                Console.WriteLine("Press 'x' key and then 'ENTER' key to exit this installer");
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("");
                Console.WriteLine("Your Input:");
                var setPatchType = Console.ReadLine();
                var patchType = new InstallerEnums.PatchTypes();
                switch (setPatchType)
                {
                    case "a":
                    case "A":
                        patchType = InstallerEnums.PatchTypes.Add;
                        break;

                    case "r":
                    case "R":
                        patchType = InstallerEnums.PatchTypes.ReAdd;
                        break;

                    case "x":
                    case "X":
                        Console.WriteLine("");
                        Console.WriteLine("Exiting....");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("");
                        InstallerMethods.ErrorExit("Input key pressed was invalid");
                        break;
                }

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Proceeding to patch....");


                // Get the drive letter for the directory
                // where the game is installed
                var drivesArray = DriveInfo.GetDrives();
                var driveLetter = Directory.GetDirectoryRoot(whitePath);             
                var driveLetterIndex = InstallerMethods.GetDriveIndex(drivesArray, driveLetter);


                // Perform checks before patching
                Checks.MinimumDriveSpace(drivesArray, driveLetterIndex);
                Checks.CoreFiles(whitePath, vo);


                // Start patching
                Thread.Sleep(2000);
                MovieHelpers.InstallMovies(whitePath, vo, drivesArray, driveLetterIndex, patchType);
            }
            catch (Exception ex)
            {
                Console.WriteLine("");

                using (var crashLogWriter = new StreamWriter("Crash.txt"))
                {
                    crashLogWriter.WriteLine("Error: " + ex);
                }

                Console.WriteLine("Crash exception recorded in Crash.txt file");
                Console.WriteLine("");
                InstallerMethods.ErrorExit("" + ex);
            }
        }
    }
}