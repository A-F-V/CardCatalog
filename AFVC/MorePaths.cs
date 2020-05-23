using System;
using System.Threading;
using System.Windows.Forms;

namespace AFVC
{
    internal class MorePaths
    {
        public static string getFolderPath()
        {
            string selectedPath = null;
            Thread t = new Thread(() =>
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.Cancel)
                    return;

                selectedPath = fbd.SelectedPath;
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return selectedPath;
        }
    }
}