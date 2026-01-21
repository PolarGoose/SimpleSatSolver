using NUnit.Framework;
using SimpleSatSolver;

namespace Test;

[TestFixture]
public class SatSolverTests
{
    private static Clause C(params int[] rawLiterals) => new(rawLiterals.Select(x => new Literal(x)).ToArray());

    [Test]
    public void Solve_EmptyFormula_IsSatisfiable()
    {
        int varCount = 3;
        var clauses = new List<Clause>();

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1)); // 1-based
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_DuplicateLiteralsInClause_IsHandled()
    {
        int varCount = 1;
        var clauses = new List<Clause>
        {
            new Clause([new Literal(1), new Literal(1)])
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1)); // 1-based
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_ZeroVariables_EmptyFormula_IsSatisfiable()
    {
        int varCount = 0;
        var clauses = new List<Clause>();

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(1)); // only index 0 exists
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_SingleUnitClause_AssignsVariableTrue()
    {
        int varCount = 1;
        var clauses = new List<Clause>
        {
            C(1) // x1
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1));
        Assert.That(solution.Value.Values[1], Is.True);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_Contradiction_Unsatisfiable_ReturnsFalseAndEmptyModel()
    {
        int varCount = 1;
        var clauses = new List<Clause>
        {
            C(1),   // x1
            C(-1)   // ¬x1
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Null);
    }

    [Test]
    public void Solve_UnitPropagation_ImpliedAssignments()
    {
        // (x1) ∧ (¬x1 ∨ x2)  ==> x1 = true, x2 = true
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(1),       // x1
            C(-1, 2)    // ¬x1 ∨ x2
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1));
        Assert.That(solution.Value.Values[1], Is.True);
        Assert.That(solution.Value.Values[2], Is.True);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_PureLiteralOrForcedByStructure_SetsLiteralToTrue()
    {
        // (x2 ∨ x1) ∧ (x2 ∨ ¬x1) forces x2 = true
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(2, 1),   // x2 ∨ x1
            C(2, -1)   // x2 ∨ ¬x1
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1));
        Assert.That(solution.Value.Values[2], Is.True);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_RequiresBranching_FindsSatisfyingAssignment()
    {
        // (x1 ∨ x2) ∧ (¬x1 ∨ ¬x2) has solutions: (T,F) or (F,T)
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(1, 2),
            C(-1, -2)
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1));
        AssertSolutionSatisfiesClauses(solution.Value, clauses);

        // sanity: should not allow both true or both false
        Assert.That(solution.Value.Values[1] ^ solution.Value.Values[2], Is.True);
    }

    [Test]
    public void Solve_UnusedVariables_AreArbitrarilyAssignedButStillSatisfy()
    {
        // Only x1 is constrained; x2 and x3 can be anything (solver leaves them unassigned => Solution maps to false).
        int varCount = 3;
        var clauses = new List<Clause>
        {
            C(1) // x1
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values.Count, Is.EqualTo(varCount + 1));
        Assert.That(solution.Value.Values[1], Is.True);
        Assert.That(solution.Value.Values[2], Is.False);
        Assert.That(solution.Value.Values[3], Is.False);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_ContainsEmptyClause_IsUnsatisfiable()
    {
        int varCount = 3;
        var clauses = new List<Clause>
        {
            C() // empty clause => UNSAT
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Null);
    }

    [Test]
    public void Solve_DerivedContradictionViaUnitPropagation_IsUnsatisfiable()
    {
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(1),      // x1
            C(-1, 2),  // ¬x1 ∨ x2
            C(-2)      // ¬x2
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Null);
    }

    [Test]
    public void Solve_PureLiteralElimination_SetsPurePositiveLiteralTrue()
    {
        // x1 is pure positive here:
        // (x1 ∨ x2) ∧ (x1 ∨ ¬x2)
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(1, 2),
            C(1, -2)
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values[1], Is.True);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_PureLiteralElimination_SetsPureNegativeLiteralFalse()
    {
        // x1 is pure negative:
        // (¬x1 ∨ x2) ∧ (¬x1 ∨ ¬x2)
        int varCount = 2;
        var clauses = new List<Clause>
        {
            C(-1, 2),
            C(-1, -2)
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        Assert.That(solution!.Value.Values[1], Is.False);
        AssertSolutionSatisfiesClauses(solution.Value, clauses);
    }

    [Test]
    public void Solve_TautologyClause_IsSatisfiable()
    {
        int varCount = 1;
        var clauses = new List<Clause>
        {
            C(1, -1) // x1 ∨ ¬x1
        };

        var solution = SatSolver.Solve(varCount, clauses);

        Assert.That(solution, Is.Not.Null);
        AssertSolutionSatisfiesClauses(solution!.Value, clauses);
    }

    private static void AssertSolutionSatisfiesClauses(Solution solution, IReadOnlyList<Clause> clauses)
    {
        var assignment = new Assignment(solution.Values
            .Select(val => val ? AssignedValue.True : AssignedValue.False)
            .ToArray());

        foreach (var clause in clauses)
            Assert.That(clause.IsSatisfied(assignment), Is.True);
    }
}
