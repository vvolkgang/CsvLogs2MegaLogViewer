using System;
using System.Diagnostics;
using System.IO;
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
                WriteLine("Insert the path of the .csv file you want to convert: ");
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


            Process.Start(path);
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

    }
}
