using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static System.Console;

namespace MslLogsFixer
{
    class Program
    {
        static readonly char[] Delimiters = { ',', ';', '|', '\t' };

        static void Main(string[] args)
        {
            const char newSeparator = '\t';

            string path = string.Empty;
            if (args.Length > 0)
            {
                path = args[0];
                WriteLine("using path: " + path);
            }
            else
            {
                WriteLine("Insert the logPath of the .csv file you want to convert: ");
                path = ReadLine();
            }

            var fileContent = File.ReadAllText(path);
            var oldSeparator = AutoDetectSeparator(fileContent);
            if (oldSeparator == '\t')
            {
                WriteLine("File's already tab based, won't process again.");
                return;
            }

            fileContent = fileContent.Replace(oldSeparator, newSeparator);
            path = Path.ChangeExtension(path, "msl");
            File.WriteAllText(path, fileContent);


            OpenLog(path);
        }

        private static void OpenLog(string logPath)
        {
            var mlvPath = FileExtentionInfo(AssocStr.Executable, ".msl");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = mlvPath,
                Arguments = "\""+logPath+"\""
            };

            WriteLine();
            WriteLine($"Opening Log using path: {logPath}");
            Process.Start(startInfo);
        }

        private static char AutoDetectSeparator(string fileContent)
        {
            var firstline = fileContent.Substring(0, fileContent.IndexOf(Environment.NewLine, StringComparison.Ordinal));

            var i = firstline.IndexOfAny(Delimiters);
            if (i == -1)
            {
                throw new Exception("Couldn't auto-detect the separator character. First line: " + firstline);
            }

            var delimiter = firstline[i];
            WriteLine($"Autodetected delimiter: {delimiter}");

            return delimiter;
        }

        #region FileAssociationRelatedCode

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        private static void TestFileAssociation()
        {
            // Contrary to what I found on the web, Process.Start("mylog.msl") doesn't open the file with the associated program
            // this method enables us to do it. 
            WriteLine(FileExtentionInfo(AssocStr.Command, ".msl"), "Command");
            WriteLine(FileExtentionInfo(AssocStr.DDEApplication, ".msl"), "DDEApplication");
            WriteLine(FileExtentionInfo(AssocStr.DDEIfExec, ".msl"), "DDEIfExec");
            WriteLine(FileExtentionInfo(AssocStr.DDETopic, ".msl"), "DDETopic");
            WriteLine(FileExtentionInfo(AssocStr.Executable, ".msl"), "Executable");
            WriteLine(FileExtentionInfo(AssocStr.FriendlyAppName, ".msl"), "FriendlyAppName");
            WriteLine(FileExtentionInfo(AssocStr.FriendlyDocName, ".msl"), "FriendlyDocName");
            WriteLine(FileExtentionInfo(AssocStr.NoOpen, ".msl"), "NoOpen");
            WriteLine(FileExtentionInfo(AssocStr.ShellNewValue, ".msl"), "ShellNewValue");

            //If MegaLogViewer is associated with .msl, should print:
            //"C:\Program Files\EFIAnalytics\MegaLogViewer\MegaLogViewer.exe" "%1"
            //MegaLogViewer
            //
            //System
            //C:\Program Files\EFIAnalytics\MegaLogViewer\MegaLogViewer.exe
            //MegaLogViewer.exe
            //MegaLogViewer
        }

        public static string FileExtentionInfo(AssocStr assocStr, string doctype)
        {
            uint pcchOut = 0;
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }

        [Flags]
        public enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        #endregion FileAssociationRelatedCode

    }
}
