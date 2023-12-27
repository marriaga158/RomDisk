using SevenZip;
using System.Diagnostics;
using System.IO;

namespace RomDisk
{
    internal static class Program
    {
        static ulong GetUncompressedSize(string path)
        {
            SevenZipExtractor.SetLibraryPath("7z.dll");
            using SevenZipExtractor extractor = new SevenZipExtractor(path);
            ulong uncompressedSize = 0;
            foreach (ArchiveFileInfo entry in extractor.ArchiveFileData)
            {
                uncompressedSize += entry.Size;
            }
            return uncompressedSize;
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // launch a dialog to select only a .zip/.7z file
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Zip Files|.zip|7zip Files|*.7z";
            fileDialog.ShowDialog();

            var filePath = fileDialog.FileName;

            // get the size of the uncompressed contents
            ulong unzippedSize = GetUncompressedSize(filePath);
            ulong unzippedSizeMb = (unzippedSize + 1000000 - 1) / 1000000;

            // create the ram disk with this size
            try
            {
                string initializeDisk = "imdisk -a ";
                string imdiskSize = "-s " + unzippedSizeMb.ToString() + "M ";
                string mountPoint = "-m Z: ";


                ProcessStartInfo procStartInfo = new ProcessStartInfo();
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                procStartInfo.FileName = "cmd";
                procStartInfo.Arguments = "/C " + initializeDisk + imdiskSize + mountPoint;
                Process.Start(procStartInfo);
            }
            catch (Exception objException)
            {
                Console.WriteLine("There was an Error, while trying to create a ramdisk! Do you have imdisk installed?");
                Console.WriteLine(objException);
            }
            Debug.WriteLine("yippee");

            // format the ram disk
            string cmdFormatHDD = "format Z: /Q /FS:NTFS";

            ProcessStartInfo formatRAMDiskProcess = new ProcessStartInfo();
            formatRAMDiskProcess.UseShellExecute = true;
            formatRAMDiskProcess.CreateNoWindow = true;
            formatRAMDiskProcess.FileName = "cmd";
            formatRAMDiskProcess.Verb = "runas";
            formatRAMDiskProcess.Arguments = "/C " + cmdFormatHDD;
            Process process = Process.Start(formatRAMDiskProcess);

            Debug.WriteLine("formatted");

            // unzip the .zip into the root of the ram disk under a folder of the zip name
            if (process.WaitForExit(20000))
            {
                // TODO: add a window that shows when it's extracting
                using SevenZipExtractor extractor = new SevenZipExtractor(filePath);
                extractor.ExtractArchive("Z:\\");
            } else
            {
                Debug.WriteLine("format timed out!");
            }

            // finish!
        }
    }
}