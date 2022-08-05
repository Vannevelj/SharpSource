using System;

namespace RoslynTester.DiagnosticResults
{
    /// <summary>
    ///     Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string filePath, int line, int column)
        {
            if (line < 0 || column < 0)
            {
                throw new ArgumentOutOfRangeException("Line and column should be 0 or positive.");
            }

            FilePath = filePath;
            Line = line;
            Column = column;
        }

        public int? Column { get; }

        public int? Line { get; }

        public string FilePath { get; }
    }
}