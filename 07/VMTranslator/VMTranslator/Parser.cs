using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
    public class Parser
    {
        private StreamReader reader;
        private Command current;
        public Parser(StreamReader reader)
        {
            this.reader = reader;
        }

        public bool HasMoreLines => !reader.EndOfStream;
        public Command CurrentCommand => current;

        public void Advance()
        {
            current = null;
            while (current == null && HasMoreLines)
            {
                var line = reader.ReadLine()
                    .Split("//")[0]
                    .Trim();

                current = line switch
                {
                    string s when string.IsNullOrWhiteSpace(s) => null,
                    string s when s.StartsWith("push") => new Command { CommandType = CommandType.C_PUSH, Arg1 = s.Split(' ')[1], Arg2 = int.Parse(s.Split(' ')[2])},
                    //"add" => new Command { CommandType = CommandType.C_ARITHMETIC, Arg1 = "add" },
                    //"eq" => new Command { CommandType = CommandType.C_ARITHMETIC, Arg1 = "eq" },
                    //"lt" => new Command { CommandType = CommandType.C_ARITHMETIC, Arg1 = "lt" },
                    _ => new Command { CommandType = CommandType.C_ARITHMETIC, Arg1 = line },
                };
            }
        }

    }

    public class Command
    {
        public CommandType CommandType { get; set; }
        public string Arg1 { get; set; }
        public int Arg2 { get; set; }
    }

    public enum CommandType
    {
        C_ARITHMETIC,
        C_PUSH,
        C_POP,
        C_LABEL,
        C_GOTO,
        C_IF,
        C_FUNCTION,
        C_RETURN,
        C_CALL
    }
}
