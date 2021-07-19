using System;
using System.IO;
using System.Linq;

namespace VMTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = args switch
            {
                string[] a when a.Length == 0 => Directory.GetFiles(Directory.GetCurrentDirectory(), "*.vm"),
                string[] a when a.Length == 1 => args,
                _ => new string[0]
            };

            files = files.Where(x => Char.IsUpper(Path.GetFileName(x)[0])).ToArray();

            foreach (var f in files)
            {
                var translator = new Translator();

                var fileNameNoExtension = Path.GetFileNameWithoutExtension(f);
                var outputFileName = Path.Combine(Path.GetDirectoryName(f), fileNameNoExtension + ".asm");
                using (var input = new StreamReader(f))
                using (var output = new StreamWriter(outputFileName))
                {
                    Console.WriteLine("Reading {0}", Path.GetFullPath(f));
                    translator.Translate(fileNameNoExtension, input, output);
                    Console.WriteLine("Wrote {0}", Path.GetFullPath(outputFileName));
                }
            }
        }
    }
}
