using System;
using System.IO;
using System.Linq;
using System.Text;

namespace JackAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = args switch
            {
                string[] a when a.Length == 0 => Directory.GetFiles(Directory.GetCurrentDirectory(), "*.jack"),
                string[] a when a.Length == 1 && a[0].EndsWith(".jack") => args,
                string[] a when a.Length == 1 && Directory.Exists(args[0]) => Directory.GetFiles(args[0], "*.jack"),
                _ => new string[0]
            };

            files = files.Where(x => Char.IsUpper(Path.GetFileName(x)[0])).ToArray();

            if (files.Length == 0)
                return;

            string parentDir = Directory.GetParent(files[0]).FullName;

            foreach (var f in files)
            {
                using (var input = new StreamReader(f))
                {
                    var outputFileName = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(f) + "T.output.xml");
                    StringBuilder output = new StringBuilder();

                    JackTokenizer tokenizer = new JackTokenizer(input);
                    output.AppendLine("<tokens>");
                    while(tokenizer.HasMoreTokens)
                    {
                        tokenizer.Advance();

                        Token current = tokenizer.CurrentToken;
                        string tokenName = Enum.GetName(current.TokenType)!;
                        tokenName = char.ToLower(tokenName[0]) + tokenName.Substring(1);

                        string outVal = current.Value switch
                        {
                            char c when c == '>' => "&gt;",
                            char c when c == '<' => "&lt;",
                            char c when c == '&' => "&amp;",
                            object o => o.ToString()
                        };

                        output.AppendLine($"<{tokenName}> {outVal} </{tokenName}>");

                        
                    }
                    output.AppendLine("</tokens>");

                    File.WriteAllText(outputFileName, output.ToString());
                }
            }

                    
                    

            //string parentDir = Directory.GetParent(files[0]).FullName;
            //var outputFileName = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(parentDir) + ".xml");


            //using (var output = new StreamWriter(outputFileName))
            //{
            //    var codeWriter = new CodeWriter("bootstrap", output);
            //    codeWriter.WriteBootstrap();

            //    var translator = new Translator(codeWriter);

            //    foreach (var f in files)
            //    {
            //        using (var input = new StreamReader(f))
            //        {
            //            Console.WriteLine("Reading {0}", Path.GetFullPath(f));
            //            translator.Translate(Path.GetFileNameWithoutExtension(f), input);
            //        }
            //    }

            //    Console.WriteLine("Wrote {0}", Path.GetFullPath(outputFileName));
            
        }
    }
}
