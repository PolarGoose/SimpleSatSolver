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

if (SatSolver.Solve(varCount, clauses, out var solution))
{
    Console.WriteLine("SAT");
    for (int v = 1; v <= varCount; v++)
        Console.WriteLine($"x{v} = {solution[v]}");
}
else
{
    Console.WriteLine("UNSAT");
    Environment.Exit(1);
}
