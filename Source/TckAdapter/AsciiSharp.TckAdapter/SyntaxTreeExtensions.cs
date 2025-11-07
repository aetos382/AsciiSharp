namespace AsciiSharp.TckAdapter;

public static class SyntaxTreeExtensions
{
    extension(SyntaxTree syntaxTree)
    {
        public IAsgElement ToAsg()
        {
            var visitor = new AsgConverter();
            var asg = syntaxTree.Root.Accept(visitor, Unit.Instance);

            return asg;
        }
    }
}
