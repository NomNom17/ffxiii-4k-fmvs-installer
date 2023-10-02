using FFXIII4kMovieMod.SupportClasses;
using Spectre.Console; // Implemented Spectre Console Namespace.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FFXIII4kMovieMod
{
    internal class Core
    {
        static void Main()
        {

            // AnsiConsole works very similarly to System.Console / Console. You can mix both Console and AnsiConsole together without having conflicts or errors.

            //AnsiConsole.Write(new FigletText("FFXIII - 4K Remastered FMVs with PS3 Audio").Centered().Color(Color.Cyan1));

            AnsiConsole.Write(new Markup("[cyan1][bold]FINAL FANTASY XIII - 4K FMVs with PS3 Audio Tracks Installer.[/][/]").Centered());

            // Markup is used to apply format to the text, like centering the text.
            // AnsiConsole.MarkupLine is similar to Console.WriteLine, except we can dynamically adjust the text style, like the colour of the text by placing [red] [/] in between the text we want to recolour, almost like HTML tags.

            AnsiConsole.Write(new Markup("----------------------------------------------------------------------------------------").Centered());

            AnsiConsole.Write(new Markup("Installer written by: [link=https://www.nexusmods.com/finalfantasy13/users/46864853?tab=user+files&BH=0]Surihix[/]\n4K FMVs made and Installer modified by: [link=https://www.nexusmods.com/users/59768371?tab=user+files]NomNom[/]").Centered());

            AnsiConsole.Write(new Markup("\n[link=https://www.nexusmods.com/finalfantasy13/mods/24?tab=description]Nexus Mods Page[/]").Centered());

            AnsiConsole.Write(new Markup("[italic]Hold CTRL + Left Click to open links.[/]").Centered());

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
                    .Title("\nChoose the [green]FMVs voice over language[/] that you want to install.\n[grey]Use the Up or Down arrow keys to navigate the menu.[/]")
                    .PageSize(3) // Set this any lower than 3, will result in an exception.
                    .AddChoices(new[] { "English Voiceovers.", "Japanese Voiceovers.", "EXIT." })
                    .HighlightStyle(Style.WithDecoration(Decoration.RapidBlink).Foreground(Color.Cyan1))
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
                        //Console.WriteLine("");
                        //Console.WriteLine("Exiting the installer....");

                        AnsiConsole.Status().Start("\n[white]Exiting...[/]", LoadingStatus =>
                        {
                            LoadingStatus.Status("[white]Please wait...[/]");
                            LoadingStatus.Spinner(Spinner.Known.Star2);
                            LoadingStatus.SpinnerStyle(Style.Parse("green"));
                            Thread.Sleep(2000);

                            LoadingStatus.Status("[green]Done![/]");
                            Thread.Sleep(350);

                            AnsiConsole.Clear();
                            Console.Clear();
                            Environment.Exit(0);
                        });
                        break;

                    default:
                        Console.WriteLine("");
                        InstallerMethods.ErrorExit("Input key pressed was invalid");
                        break;
                }

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");


                AnsiConsole.MarkupLine("[italic]Adding Movies - Recommended for first time installation. Takes longer to complete.\nReAdding Movies - Recommended for applying Patches/Updates or to overwrite a corrupted movie file with a functioning movie file. Quicker to complete.[/]");

                // Set Patch type

                string PatchTypeSelection = AnsiConsole.Prompt(new SelectionPrompt<String>()
                .Title("\nChoose how you want to [green]install[/] the FMVs.")
                .PageSize(3) // Set this any lower than 3, will result in an exception.
                .AddChoices(new[] { "Adding Movies.", "ReAdding Movies.", "EXIT." })
                .HighlightStyle(Style.WithDecoration(Decoration.RapidBlink).Foreground(Color.Cyan1))
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
                        AnsiConsole.Status().Start("\n[white]Exiting...[/]", LoadingStatus =>
                        {
                            LoadingStatus.Status("[white]Please wait...[/]");
                            LoadingStatus.Spinner(Spinner.Known.Star2);
                            LoadingStatus.SpinnerStyle(Style.Parse("green"));
                            Thread.Sleep(2000);

                            LoadingStatus.Status("[green]Done![/]");
                            Thread.Sleep(350);

                            AnsiConsole.Clear();
                            Console.Clear();
                            Environment.Exit(0);
                        });
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

                /*Console.WriteLine("Crash exception recorded in Crash.txt file");
                Console.WriteLine("");*/

                //InstallerMethods.ErrorExit("" + ex);
                AnsiConsole.Foreground = Color.Red;

                AnsiConsole.WriteLine("Crash exception has been caught and recorded in the Crash.txt file!! Please try again. If installer still crashes, please report it on the Nexus Mods Page, or on the Discord Server.\n\nException information:\n" + ex);

                AnsiConsole.Foreground = Color.White;

                AnsiConsole.WriteLine("\n\nPress Enter to continue.");

                Console.ReadLine();

                AnsiConsole.MarkupLine("[white]Would you like to report this exception? If so, where do you want to file the report?[/]");

                AnsiConsole.MarkupLine("\n[italic]You will need a Nexus Mods account to comment or open a bug report.[/]\n[italic]You will need to join the Discord Server to report the crash exception.[/]\n[italic]Selecting either 'Yes' option will open your browser with the corresponding link.[/]");

                string ReportTypeSelection = AnsiConsole.Prompt(new SelectionPrompt<String>()
                .Title("\nChoose an [green]option[/] listed below.\n[grey]Use the Up or Down arrow keys to navigate the menu.[/]")
                .PageSize(3) // Set this any lower than 3, will result in an exception.
                .AddChoices(new[] { "Yes, on Nexus Mods.", "Yes, on the Discord Server.", "No." })
                .HighlightStyle(Style.WithDecoration(Decoration.RapidBlink).Foreground(Color.Cyan1))
                );

                switch (ReportTypeSelection)
                {
                    case "Yes, on Nexus Mods.":
                        Process.Start("https://www.nexusmods.com/finalfantasy13/mods/24?tab=files");
                        break;

                    case "Yes, on the Discord Server.":
                        Process.Start("https://discord.gg/KnsDNgFm2V");
                        break;

                    case "No.":
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}