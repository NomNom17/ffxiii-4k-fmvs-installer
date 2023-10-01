using FFXIII4kMovieMod.SupportClasses;
using System.IO;

namespace FFXIII4kMovieMod
{
    internal class Checks
    {
        public static void MinimumDriveSpace(DriveInfo[] drivesArray, int driveIndex)
        {
            var availableFreeSpace = drivesArray[driveIndex].AvailableFreeSpace;
            if (availableFreeSpace < 5368709120)
            {
                InstallerMethods.ErrorExit("Not enough space available in the drive where the game is installed.\nPlease ensure atleast 5gb of free space in the drive and then run this installer.");
            }
        }

        public static void CoreFiles(string whitePath, InstallerEnums.VoiceOvers vo)
        {
            var fileSuffix = vo == InstallerEnums.VoiceOvers.us ? "u" : "c";

            var fileListFile = Path.Combine(whitePath, "sys", $"filelist{fileSuffix}.win32.bin");
            var whiteImgFile = Path.Combine(whitePath, "sys", $"white_img{fileSuffix}.win32.bin");

            FileCheck(fileListFile);
            FileCheck(whiteImgFile);
        }

        static void FileCheck(string fileToCheck)
        {
            if (!File.Exists(fileToCheck))
            {
                InstallerMethods.ErrorExit($"Missing {fileToCheck} in the game directory.\nPlease ensure that this file is present in its respective location before running this installer.");
            }
        }    
    }
}