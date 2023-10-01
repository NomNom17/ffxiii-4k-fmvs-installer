using LocateFile;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FFXIII4kMovieMod.SupportClasses
{
    internal class InstallerMethods
    {
        public static void DisplayMessageBox(string msg, string msgTitle, InstallerEnums.MsgBoxIcon msgType)
        {
            var msgTypeIcon = MessageBoxIcon.Information;

            switch (msgType)
            {
                case InstallerEnums.MsgBoxIcon.Info:
                    msgTypeIcon = MessageBoxIcon.Information;
                    break;

                case InstallerEnums.MsgBoxIcon.Error:
                    msgTypeIcon = MessageBoxIcon.Error;
                    break;

                case InstallerEnums.MsgBoxIcon.Warning:
                    msgTypeIcon = MessageBoxIcon.Warning;
                    break;
            }

            MessageBox.Show(msg, msgTitle, MessageBoxButtons.OK, msgTypeIcon);
        }


        public static string SetPath(string fileToFind)
        {
            var foundPath = "";
            var t = new Thread(() =>
            {
                foundPath = Locator.FindFilePath(fileToFind);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return foundPath;
        }


        public static void ErrorExit(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            DisplayMessageBox(errorMsg, "Error", InstallerEnums.MsgBoxIcon.Error);
            Environment.Exit(0);
        }


        public static int GetDriveIndex(DriveInfo[] drivesArray, string driveLetter)
        {
            var driveLetterIndex = 0;
            for (int d = 0; d < drivesArray.Length; d++)
            {
                var drive = drivesArray[d];
                if (drive.Name.Equals(driveLetter))
                {
                    driveLetterIndex = d;
                    break;
                }
            }
            return driveLetterIndex;
        }


        public static void IfFileFolderExistsDel(string fileFolderToCheck, InstallerEnums.DeleteType delType)
        {
            switch (delType)
            {
                case InstallerEnums.DeleteType.file:
                    if (File.Exists(fileFolderToCheck))
                    {
                        File.Delete(fileFolderToCheck);
                    }
                    break;

                case InstallerEnums.DeleteType.folder:
                    if (Directory.Exists(fileFolderToCheck))
                    {
                        Directory.Delete(fileFolderToCheck, true);
                    }
                    break;
            }
        }
    }
}