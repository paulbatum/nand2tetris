using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HackAssembler
{
    public class Parser
    {
        private StreamReader reader;
        private Instruction current;
        private Regex cInstructionRegex;

        public Parser(StreamReader reader)
        {
            this.reader = reader;
            cInstructionRegex = new Regex(@"(?:([ADM]+)=)?([01ADM\!\-\+\&\|]+)(?:;([JGTEQLNMP]+))?"); // https://regexr.com/
        }

        public bool HasMoreLines => !reader.EndOfStream;
        public Instruction CurrentInstruction => current;

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
                    string s when s.StartsWith("(") => new Instruction { Type = InstructionType.L_INSTRUCTION, Symbol = s[1..^1] },
                    string s when s.StartsWith("@") => new Instruction { Type = InstructionType.A_INSTRUCTION, Symbol = s[1..] },
                    string s => cInstructionRegex.Match(s) switch
                    {
                        Match m when m.Success => new Instruction
                        {
                            Type = InstructionType.C_INSTRUCTION,
                            Dest = m.Groups[1].Value,
                            Comp = m.Groups[2].Value,
                            Jump = m.Groups[3].Value
                        },
                        _ => throw new Exception($"Failed to parse C-instruction: {s}")
                    },
                    _ => throw new Exception($"Failed to parse line: {line}")
                };
            }

        }


    }

    public class Instruction
    {
        public InstructionType Type { get; set; }
        public string Symbol { get; set; }
        public string Dest { get; set; }
        public string Comp { get; set; }
        public string Jump { get; set; }
    }

    public enum InstructionType
    {
        A_INSTRUCTION,
        C_INSTRUCTION,
        L_INSTRUCTION
    }
}
