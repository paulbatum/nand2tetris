﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalyzer
{
    public class VMWriter
    {
        private StreamWriter output;

        public VMWriter(StreamWriter output)
        {
            this.output = output;
        }

        public void WriteFunction(string name, int nVars)
        {
            output.WriteLine($"function {name} {nVars}");
        }

        public void WriteCall(string name, int nVars)
        {
            output.WriteLine($"call {name} {nVars}");
        }

        public void WriteReturn()
        {
            output.WriteLine("return");
        }

        public void WritePush(Segment segment, int index)
        {
            output.WriteLine($"push {segment.ToString().ToLower()} {index}");
        }

        public void WritePop(Segment segment, int index)
        {
            output.WriteLine($"pop {segment.ToString().ToLower()} {index}");
        }
    }

    public enum Segment
    {
        Constant,
        Argument,
        Local,
        Static,
        This,
        That,
        Pointer,
        Temp
    }
}
