using HDLTools.TestScripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HDLTools.Test.Chapter3
{
    public class BitTests
    {

        private readonly ITestOutputHelper testOutput;
        public BitTests(ITestOutputHelper output)
        {
            this.testOutput = output;
        }

        [Fact]
        public void PassesTestScript()
        {
            string compare = File.ReadAllText(@"cmp\Bit.cmp");

            var fs = new MockFileSystem();
            fs.AddFile(@"cmp\Bit.cmp", new MockFileData(compare));

            var library = new ChipLibrary();
            library.LoadAll("hdl");

            string script = File.ReadAllText(@"tst\Bit.tst");
            var commands = TestScriptParser.ParseString(script);
            var executor = new TestScriptExecutor(fs, library, commands);

            while (executor.HasMoreLines)
                executor.Step();

            if(executor.ComparisonFailures.Count > 0)
            {
                testOutput.WriteLine("Compare:");
                testOutput.WriteLine(compare);
                testOutput.WriteLine("");
                testOutput.WriteLine("Out:");
                testOutput.WriteLine(fs.File.ReadAllText("Bit.out"));
            }

            Assert.Empty(executor.ComparisonFailures);
        }
    }
}
