using SimpleSatSolver;

if (args.Length != 1)
{
    Console.WriteLine("Usage: SimpleSatSolver <input.cnf>");
    return;
}

string cnfFilePath = args[0];
if (!File.Exists(cnfFilePath))
{
    Console.WriteLine($"File not found: {cnfFilePath}");
    Environment.Exit(2);
}

var (varCount, clauses) = DimacsParser.Parse(cnfFilePath);

var solution = SatSolver.Solve(varCount, clauses);
if (solution.HasValue)
{
    Console.WriteLine("SAT");
    foreach (var (value, index) in solution.Value.Values.Select((v, i) => (v, i)))
        Console.WriteLine($"x{index} = {value}");
}
else
{
    Console.WriteLine("UNSAT");
    Environment.Exit(1);
}
