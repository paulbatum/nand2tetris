using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HackAssembler
{
    public class Assembler
    {
        public Assembler()
        {

        }        

        public void PopulateTableWithLabels(StreamReader input, SymbolTable symbolTable)
        {
            var parser = new Parser(input);
            short lineNumber = -1;
            while (parser.HasMoreLines)
            {
                parser.Advance();
                var current = parser.CurrentInstruction;
                if (current == null) continue;
                switch(current.Type)
                {
                    case InstructionType.A_INSTRUCTION:
                    case InstructionType.C_INSTRUCTION:
                        lineNumber++;
                        break;
                    case InstructionType.L_INSTRUCTION:
                        symbolTable.AddEntry(current.Symbol, (short)(lineNumber + 1));
                        break;                    
                }                                
            }
        }

        public void Assemble(StreamReader input, SymbolTable symbolTable, StreamWriter output)
        {
            var parser = new Parser(input);            
            while (parser.HasMoreLines)
            {
                parser.Advance();
                var current = parser.CurrentInstruction;
                if (current == null) continue;
                switch (current.Type)
                {
                    case InstructionType.A_INSTRUCTION:                        
                        if(!int.TryParse(current.Symbol, out int val))
                        {
                            if (symbolTable.Contains(current.Symbol))
                            {
                                val = symbolTable.GetAddress(current.Symbol);
                            }
                            else
                            {
                                val = symbolTable.AddVariable(current.Symbol);
                            }
                        }                            
                        output.WriteLine(Convert.ToString((short)val, 2).PadLeft(16,'0'));
                        break;
                    case InstructionType.C_INSTRUCTION:                        
                        var binaryInstruction = $"111{Code.Comp(current.Comp)}{Code.Dest(current.Dest)}{Code.Jump(current.Jump)}";
                        if (binaryInstruction.Length != 16)
                            throw new Exception($"Instruction '{binaryInstruction}' has length {binaryInstruction.Length} but should have length 16");
                        output.WriteLine(binaryInstruction);
                        break;
                    case InstructionType.L_INSTRUCTION:
                        // no op
                        break;
                }
            }
        }
    }
}
