using CliWrap;
using CliWrap.Buffered;
using NUnit.Framework;
using System.Text;

namespace Test;

[TestFixture]
public class ProgramTest
{
    [Test]
    public void PrintsUsageWhenNoArgumentsPassed()
    {
        var res = RunProgram([]);
        Assert.That(res.StandardOutput, Does.Contain("Usage: SimpleSatSolver <input.cnf>"));
    }

    [Test]
    public void PrintsUsageWhenMoreThan1ArgumentPassed()
    {
        var res = RunProgram(["arg1", "arg2"]);
        Assert.That(res.StandardOutput, Does.Contain("Usage: SimpleSatSolver <input.cnf>"));
    }

    [Test]
    public void FailsIfNonExistentFileIsPassed()
    {
        var res = RunProgram(["nonExistentFile"]);
        Assert.That(res.ExitCode, Is.EqualTo(2));
        Assert.That(res.StandardOutput, Does.Contain("File not found: "));
    }

    [Test]
    public void WorksCorrectlyForSatCnf()
    {
        var res = RunProgram([Path.Join(TestContext.CurrentContext.TestDirectory, "test_files", "sat", "uf50-0997.cnf")]);
        Assert.That(res.ExitCode, Is.EqualTo(0));
        Assert.That(res.StandardOutput, Does.Contain("SAT"));
        Assert.That(res.StandardOutput, Does.Contain("x1 = "));
    }

    [Test]
    public void WorksCorrectlyForUnsatCnf()
    {
        var res = RunProgram([Path.Join(TestContext.CurrentContext.TestDirectory, "test_files", "unsat", "UUF50.218.1000", "uuf50-0999.cnf")]);
        Assert.That(res.ExitCode, Is.EqualTo(1));
        Assert.That(res.StandardOutput, Does.Contain("UNSAT"));
    }

    private static BufferedCommandResult RunProgram(IEnumerable<string> args) =>
        Cli.Wrap(Path.Combine(TestContext.CurrentContext.TestDirectory, "SimpleSatSolver"))
           .WithValidation(CommandResultValidation.None)
           .WithArguments(args)
           .ExecuteBufferedAsync(Encoding.UTF8)
           .GetAwaiter()
           .GetResult();
}
