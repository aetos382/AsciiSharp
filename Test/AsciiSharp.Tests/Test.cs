using AsciiSharp.Syntax;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AsciiSharp.Tests;

[TestClass]
public sealed class Test
{
    [TestMethod]
    public void SampleTest()
    {
        const string code =
            """
            = foo

            == bar

            === baz
            """;

        _ = SyntaxTree.ParseText(code);
    }
}
