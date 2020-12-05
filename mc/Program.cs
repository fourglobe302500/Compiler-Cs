namespace Compiler
{
    internal static class Program
    {
        private static void Main( )
        {
            var repl = new MangleRepl();
            repl.Run();
        }
    }
}