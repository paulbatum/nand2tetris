using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
    public class CodeWriter
    {
        private StreamWriter writer;

        public CodeWriter(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void WriteArithmetic(string command)
        {
            writer.WriteLine("// " + command);
            WritePopD();
            writer.WriteLine("@R13");
            writer.WriteLine("M=D");
            WritePopD();
            writer.WriteLine("@R13");

            if (command == "add")
            {
                writer.WriteLine("D=D+M");
            }
            else
            {
                throw new NotImplementedException("Can only add");
            }

            WritePushD();
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
            switch (commandType)
            {
                case CommandType.C_PUSH:
                    writer.WriteLine($"// push {segment} {index}");
                    if(segment == "constant")
                    {
                        writer.WriteLine($"@{index}");
                        writer.WriteLine($"D=A");
                        WritePushD();
                    }
                    else
                    {
                        throw new NotImplementedException("Can only handle constants");
                    }
                    break;
                case CommandType.C_POP:
                    writer.WriteLine($"// pop {segment} {index}");
                    throw new NotImplementedException("no popping yet");
                    //break;
                default:
                    throw new ArgumentException($"Cannot push/pop command '{commandType}'.");
            }
        }

        public void WriteInfiniteLoop()
        {
            writer.WriteLine("(LOOP)");
            writer.WriteLine("@LOOP");
            writer.WriteLine("0;JMP");
        }

        private void WritePushD()
        {
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
        }

        private void WritePopD()
        {
            writer.WriteLine("@SP");
            writer.WriteLine("M=M-1");
            writer.WriteLine("A=M");
            writer.WriteLine("D=M");
            writer.WriteLine("@SP");            
        }
    }
}
