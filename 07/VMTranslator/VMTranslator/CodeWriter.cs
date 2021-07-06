using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
    public class CodeWriter
    {
        private StreamWriter writer;
        private int labelCounter = 0;

        public CodeWriter(StreamWriter writer)
        {
            this.writer = writer;
        }

        public void WritePushPop(CommandType commandType, string segment, int index)
        {
            switch (commandType)
            {
                case CommandType.C_PUSH:
                    writer.WriteLine($"// push {segment} {index}");
                    if (segment == "constant")
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

        public void WriteArithmetic(string command)
        {
            writer.WriteLine("// " + command);
            switch (command)
            {
                case "neg":
                    WritePopD();
                    writer.WriteLine("D=-D");
                    WritePushD();
                    break;
                case "not":
                    WritePopD();
                    writer.WriteLine("D=!D");
                    WritePushD();
                    break;
                default:
                    WriteDoubleOperandArithmetic(command);
                    break;
            }            
        }

        private void WriteDoubleOperandArithmetic(string command)
        {            
            WritePopD();
            writer.WriteLine("@R13");
            writer.WriteLine("M=D");
            WritePopD();
            writer.WriteLine("@R13");

            switch(command)
            {
                case "add":
                    writer.WriteLine("D=D+M");
                    break;
                case "sub":
                    writer.WriteLine("D=D-M");
                    break;
                case "eq":
                    writer.WriteLine("D=D-M");
                    WriteDCompareZero("JEQ");
                    break;
                case "lt":
                    writer.WriteLine("D=D-M");
                    WriteDCompareZero("JLT");
                    break;
                case "gt":
                    writer.WriteLine("D=D-M");
                    WriteDCompareZero("JGT");
                    break;
                case "and":
                    writer.WriteLine("D=D&M");
                    break;
                case "or":
                    writer.WriteLine("D=D|M");
                    break;                
                default:
                    throw new NotImplementedException($"Unrecognized command '{command}");
            }

            WritePushD();
        }

        private void WriteDCompareZero(string comp)
        {
            string equalLabel = $"EQ.{ labelCounter++}";
            string joinLabel = $"JOIN.{ labelCounter++}";

            writer.WriteLine($"@{equalLabel}");
            writer.WriteLine($"D;{comp}");
            writer.WriteLine("D=0");
            writer.WriteLine($"@{joinLabel}");
            writer.WriteLine($"0;JMP");
            writer.WriteLine($"({equalLabel})");
            writer.WriteLine("D=-1");
            writer.WriteLine($"({joinLabel})");
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
