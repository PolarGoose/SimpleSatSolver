using NUnit.Framework;
using SimpleSatSolver;

namespace Test;

[TestFixture]
public class SatSolverTest_UseTestFiles
{
    public static IEnumerable<string> SatFiles() =>
        Directory.EnumerateFiles(Path.Join(TestContext.CurrentContext.TestDirectory, "test_files", "sat"), "*", SearchOption.AllDirectories);

    public static IEnumerable<string> UnsatFiles() =>
        Directory.EnumerateFiles(Path.Join(TestContext.CurrentContext.TestDirectory, "test_files", "unsat"), "*", SearchOption.AllDirectories);

    [TestCaseSource(nameof(SatFiles))]
    public void Satisfiable_files_should_solve(string testFile)
    {
        var (varCount, clauses) = DimacsParser.Parse(testFile);
        Assert.That(SatSolver.Solve(varCount, clauses, out var model), Is.True, $"File: {testFile}");
    }
    [TestCaseSource(nameof(UnsatFiles))]
    public void Unsatisfiable_files_should_not_solve(string testFile)
    {
        var (varCount, clauses) = DimacsParser.Parse(testFile);
        Assert.That(SatSolver.Solve(varCount, clauses, out var model), Is.False, $"File: {testFile}");
    }
}
