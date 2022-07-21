using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
    class Translator
    {
        private CodeWriter _writer;

        public Translator(CodeWriter writer)
        {
            _writer = writer;
        }

        public void Translate(string fileName, StreamReader input)
        {
            _writer.FileName = fileName;
            var parser = new Parser(input);            

            while (parser.HasMoreLines)
            {
                parser.Advance();
                var current = parser.CurrentCommand;
                if (current == null) continue;
                switch (current.CommandType)
                {
                    case CommandType.C_ARITHMETIC:
                        _writer.WriteArithmetic(current.Arg1);
                        break;
                    case CommandType.C_PUSH:
                    case CommandType.C_POP:
                        _writer.WritePushPop(current.CommandType, current.Arg1, current.Arg2);
                        break;
                    case CommandType.C_LABEL:
                        _writer.WriteLabel(current.Arg1);
                        break;
                    case CommandType.C_IF:
                        _writer.WriteIf(current.Arg1);
                        break;
                    case CommandType.C_GOTO:
                        _writer.WriteGoto(current.Arg1);
                        break;
                    case CommandType.C_FUNCTION:
                        _writer.WriteFunction(current.Arg1, current.Arg2);
                        break;
                    case CommandType.C_RETURN:
                        _writer.WriteReturn();
                        break;
                    case CommandType.C_CALL:
                        _writer.WriteCall(current.Arg1, current.Arg2);
                        break;
                    default:                    
                        throw new Exception($"No handling for '{current.CommandType}.");
                }
            }
        }
    }
}
