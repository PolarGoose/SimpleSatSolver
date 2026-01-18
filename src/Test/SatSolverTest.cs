using NUnit.Framework;
using SimpleSatSolver;

namespace Test;

[TestFixture]
public class SatSolverTests
{
    [Test]
    public void Solve_EmptyFormula_IsSatisfiable()
    {
        int varCount = 3;
        var clauses = new List<int[]>();

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Length, Is.EqualTo(varCount + 1)); // 1-based
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_ZeroVariables_EmptyFormula_IsSatisfiable()
    {
        int varCount = 0;
        var clauses = new List<int[]>();

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(1)); // only index 0 exists
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_SingleUnitClause_AssignsVariableTrue()
    {
        int varCount = 1;
        var clauses = new List<int[]>
            {
                new[] { 1 } // x1
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));
        Assert.That(model[1], Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_Contradiction_Unsatisfiable_ReturnsFalseAndEmptyModel()
    {
        int varCount = 1;
        var clauses = new List<int[]>
            {
                new[] { 1 },   // x1
                new[] { -1 }   // ¬x1
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.False);
        Assert.That(model, Is.Not.Null);
        Assert.That(model, Is.Empty);
    }

    [Test]
    public void Solve_UnitPropagation_ImpliedAssignments()
    {
        // (x1) ∧ (¬x1 ∨ x2)  ==> x1 = true, x2 = true
        int varCount = 2;
        var clauses = new List<int[]>
            {
                new[] { 1 },        // x1
                new[] { -1, 2 }     // ¬x1 ∨ x2
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));
        Assert.That(model[1], Is.True);
        Assert.That(model[2], Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_PureLiteralOrForcedByStructure_SetsLiteralToTrue()
    {
        // (x2 ∨ x1) ∧ (x2 ∨ ¬x1) forces x2 = true
        // If x2=false then first forces x1=true and second forces x1=false => contradiction.
        int varCount = 2;
        var clauses = new List<int[]>
            {
                new[] { 2, 1 },   // x2 ∨ x1
                new[] { 2, -1 }   // x2 ∨ ¬x1
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));
        Assert.That(model[2], Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_RequiresBranching_FindsSatisfyingAssignment()
    {
        // (x1 ∨ x2) ∧ (¬x1 ∨ ¬x2) has solutions: (T,F) or (F,T)
        int varCount = 2;
        var clauses = new List<int[]>
            {
                new[] { 1, 2 },
                new[] { -1, -2 }
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));
        AssertModelSatisfies(varCount, clauses, model);

        // sanity: should not allow both true or both false
        Assert.That(model[1] ^ model[2], Is.True);
    }

    [Test]
    public void Solve_UnusedVariables_AreArbitrarilyAssignedButStillSatisfy()
    {
        // Only x1 is constrained; x2 and x3 can be anything (solver defaults them to false).
        int varCount = 3;
        var clauses = new List<int[]>
            {
                new[] { 1 } // x1
            };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));
        Assert.That(model[1], Is.True);
        Assert.That(model[2], Is.False);
        Assert.That(model[3], Is.False);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_ContainsEmptyClause_IsUnsatisfiable()
    {
        int varCount = 3;
        var clauses = new List<int[]>
        {
            Array.Empty<int>() // empty clause => UNSAT
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.False);
        Assert.That(model, Is.Empty);
    }

    [Test]
    public void Solve_DerivedContradictionViaUnitPropagation_IsUnsatisfiable()
    {
        int varCount = 2;
        var clauses = new List<int[]>
        {
            new[] { 1 },       // x1
            new[] { -1, 2 },   // ¬x1 ∨ x2
            new[] { -2 }       // ¬x2
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.False);
        Assert.That(model, Is.Empty);
    }

    [Test]
    public void Solve_PureLiteralElimination_SetsPurePositiveLiteralTrue()
    {
        // x1 is pure positive here:
        // (x1 ∨ x2) ∧ (x1 ∨ ¬x2)
        int varCount = 2;
        var clauses = new List<int[]>
        {
            new[] { 1, 2 },
            new[] { 1, -2 }
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model[1], Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_PureLiteralElimination_SetsPureNegativeLiteralFalse()
    {
        // x1 is pure negative:
        // (¬x1 ∨ x2) ∧ (¬x1 ∨ ¬x2)
        int varCount = 2;
        var clauses = new List<int[]>
        {
            new[] { -1, 2 },
            new[] { -1, -2 }
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        Assert.That(model[1], Is.False);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_TautologyClause_IsSatisfiable()
    {
        int varCount = 1;
        var clauses = new List<int[]>
        {
            new[] { 1, -1 } // x1 ∨ ¬x1
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    [Test]
    public void Solve_DuplicateLiteralsInClause_IsHandled()
    {
        int varCount = 1;
        var clauses = new List<int[]>
        {
            new[] { 1, 1 }
        };

        bool sat = SatSolver.Solve(varCount, clauses, out var model);

        Assert.That(sat, Is.True);
        AssertModelSatisfies(varCount, clauses, model);
    }

    private static void AssertModelSatisfies(int varCount, List<int[]> clauses, bool[] model)
    {
        Assert.That(model, Is.Not.Null);
        Assert.That(model.Length, Is.EqualTo(varCount + 1));

        foreach (var clause in clauses)
        {
            Assert.That(ClauseSatisfied(clause, model), Is.True,
                $"Model did not satisfy clause: [{string.Join(", ", clause)}]");
        }
    }

    private static bool ClauseSatisfied(int[] clause, bool[] model)
    {
        foreach (int lit in clause)
        {
            int v = Math.Abs(lit);
            bool val = model[v];
            bool litVal = lit > 0 ? val : !val;
            if (litVal) return true;
        }
        return false;
    }
}
