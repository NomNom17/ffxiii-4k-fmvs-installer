using System.IO;
using System.Windows.Forms;

namespace LocateFile
{
    public class Locator
    {
        public static string FindFilePath(string fileToFind)
        {
            var pathSelect = new OpenFileDialog
            {
                FileName = fileToFind,
                Filter = fileToFind + $"|{fileToFind}",
                RestoreDirectory = true
            };

            var foundPath = "";
            if (pathSelect.ShowDialog() == DialogResult.OK)
            {
                foundPath = Path.GetFullPath(pathSelect.FileName);
            }

            return foundPath;
        }
    }
}