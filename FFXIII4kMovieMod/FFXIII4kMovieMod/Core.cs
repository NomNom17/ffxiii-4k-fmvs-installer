using FFXIII4kMovieMod.SupportClasses;
using Spectre.Console; // Implemented Spectre Console Namespace.
using System;
using System.IO;
using System.Threading;

namespace FFXIII4kMovieMod
{
    internal class Core
    {
        static void Main()
        {

            // AnsiConsole works very similarly to System.Console / Console. You can mix both Console and AnsiConsole together without having conflicts or errors.

            AnsiConsole.Write(new FigletText("FFXIII - 4K Remastered FMVs with PS3 Audio").Centered().Color(Color.Cyan1));

            // Markup is used to apply format to the text, like centering the text.
            // AnsiConsole.MarkupLine is similar to Console.WriteLine, except we can dynamically adjust the text style, like adjust the colour of the text by placing [red] [/] in between the text we want to recolour, almost like HTML tags.

            AnsiConsole.Write(new Markup("----------------------------------------------------------------------------------------").Centered());

            AnsiConsole.Write(new Markup("Installer written by: Surihix.\n4K FMVs made and Installer modified by: NomNom").Centered());

            AnsiConsole.Write(new Markup("----------------------------------------------------------------------------------------").Centered());

            try
            {
                // Set Game path
                InstallerMethods.DisplayMessageBox("Select the 'FFXiiiLauncher.exe' file present in the FINAL FANTASY XIII game folder.", "Locate FINAL FANTASY XIII launcher", InstallerEnums.MsgBoxIcon.Info);
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

                string VOSelection = AnsiConsole.Prompt(new SelectionPrompt<String>()
                    .Title("\nChoose the [green]Voice Over FMVs[/] that you want to install.")
                    .PageSize(3) // Set this any lower than 3, will result in an exception.
                    .AddChoices(new[] { "English Voiceovers.", "Japanese Voiceovers.", "EXIT." })
                    );


                // Set voiceovers to patch
                /*
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
                */

                var vo = new InstallerEnums.VoiceOvers();
                switch (VOSelection)
                {
                    //case "u":
                    case "English Voiceovers.":
                        vo = InstallerEnums.VoiceOvers.us;
                        break;

                    //case "j":
                    case "Japanese Voiceovers.":
                        vo = InstallerEnums.VoiceOvers.jpn;
                        break;

                    //case "x":
                    case "EXIT.":
                        Console.WriteLine("");
                        Console.WriteLine("Exiting the installer....");
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

                string PatchTypeSelection = AnsiConsole.Prompt(new SelectionPrompt<String>()
                .Title("\nChoose the [green]Voice Over FMVs[/] that you want to install.")
                .PageSize(3) // Set this any lower than 3, will result in an exception.
                .AddChoices(new[] { "Adding Movies.", "ReAdding Movies.", "EXIT." })
                );


                /*
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
                */

                var patchType = new InstallerEnums.PatchTypes();
                switch (PatchTypeSelection)
                {
                    //case "a":
                    case "Adding Movies.":
                        patchType = InstallerEnums.PatchTypes.Add;
                        break;

                    //case "r":
                    case "ReAdding Movies.":
                        patchType = InstallerEnums.PatchTypes.ReAdd;
                        break;

                    //case "x":
                    case "EXIT.":
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