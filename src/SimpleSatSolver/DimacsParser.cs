using System.Text.RegularExpressions;

namespace SimpleSatSolver;

// DIMACS file looks like:
//   c commented line
//   p cnf 50  218
//   -35 -36 -10 0
//   -31 34 37 0
//   %
//   0
public static partial class DimacsParser
{
    public static (int varCount, List<Clause> clauses) Parse(string dimacsFilePath)
    {
        int varCount = 0;
        var clauses = new List<Clause>();
        var current = new List<Literal>();

        foreach (var line in File.ReadLines(dimacsFilePath).Select(l => l.Trim()))
        {
            // Skip comments and empty lines
            if (line.Length == 0 || line.StartsWith("c")) continue;

            // Some DIMACS files have extra information after the clauses like:
            //   %
            //   0
            if (line[0] == '%') break; // ignore extra information

            // First non commented line must be the problem description line
            if (line[0] == 'p')
            {
                var match = ProblemLineRegex().Match(line);
                if (!match.Success)
                    throw new FormatException($"Invalid DIMACS problem description line: '{line}'");
                varCount = int.Parse(match.Groups["vars"].ValueSpan);
                continue;
            }

            foreach (var x in line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).Select(int.Parse))
            {
                // The clause line ends with 0
                if (x == 0)
                {
                    clauses.Add(new Clause([.. current]));
                    current.Clear();
                }
                else
                {
                    current.Add(new Literal(x));
                }
            }
        }

        return (varCount, clauses);
    }

    // The problem description line looks like:
    //   p cnf <number of vars> <number of clauses>
    [GeneratedRegex(@"^p\s+cnf\s+(?<vars>\d+)\s+(?<clauses>\d+)\s*$")]
    private static partial Regex ProblemLineRegex();
}
