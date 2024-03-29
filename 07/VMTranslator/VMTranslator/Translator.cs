﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VMTranslator
{
    class Translator
    {
        public void Translate(string fileName, StreamReader input, StreamWriter output)
        {
            var parser = new Parser(input);
            var codeWriter = new CodeWriter(fileName, output);

            while (parser.HasMoreLines)
            {
                parser.Advance();
                var current = parser.CurrentCommand;
                if (current == null) continue;
                switch (current.CommandType)
                {
                    case CommandType.C_ARITHMETIC:
                        codeWriter.WriteArithmetic(current.Arg1);
                        break;
                    case CommandType.C_PUSH:
                    case CommandType.C_POP:
                        codeWriter.WritePushPop(current.CommandType, current.Arg1, current.Arg2);
                        break;
                    default:
                        throw new Exception($"No handling for '{current.CommandType}.");
                }
            }

            codeWriter.WriteInfiniteLoop();
        }
    }
}
