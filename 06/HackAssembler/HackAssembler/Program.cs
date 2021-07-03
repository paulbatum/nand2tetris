using System;
using System.IO;

namespace HackAssembler
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader reader = args switch
            {
                string[] a when a.Length == 1 => new StreamReader(a[0]),
                _ => null
            };

            var parser = new Parser(reader);

            while (parser.HasMoreLines)
            {
                parser.Advance();
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
