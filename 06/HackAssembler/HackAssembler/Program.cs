using System;
using System.IO;

namespace HackAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = args switch
            {
                string[] a when a.Length == 0 => Directory.GetFiles(Directory.GetCurrentDirectory(), "*.asm"),
                string[] a when a.Length == 1 => args,
                _ => new string[0]
            };

            foreach (var f in files)
            {
                var assembler = new Assembler();                
                var symbolTable = new SymbolTable();

                using (var symbolreader = new StreamReader(f))
                    assembler.PopulateTableWithLabels(symbolreader, symbolTable);

                var outputFileName = Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f) + ".hack");
                using (var input = new StreamReader(f))
                using (var output = new StreamWriter(outputFileName))
                {
                    Console.WriteLine("Reading {0}", Path.GetFullPath(f));
                    assembler.Assemble(input, symbolTable, output);
                    Console.WriteLine("Wrote {0}", Path.GetFullPath(outputFileName));
                }
            }
        }

    }
}
