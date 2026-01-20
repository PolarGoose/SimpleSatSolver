namespace SimpleSatSolver;

public static class SatSolver
{
    public static bool Solve(int varCount, List<int[]> clauses, out bool[] solution)
    {
        if (Dpll(clauses, new bool?[varCount + 1], out bool?[] finalAssign))
        {
            solution = [.. finalAssign.Select(b => b ?? false)];
            return true;
        }

        solution = [];
        return false;
    }

    // Implements `Davis–Putnam–Logemann–Loveland` algorithm with unit propagation and pure literal elimination.
    private static bool Dpll(List<int[]> clauses, bool?[] assignment, out bool?[] solution)
    {
        // At this point not all variables in `assignment` are assigned yet.
        // Given partial assignment we can try to deduce values for some unassigned variables using:
        //   * Unit clauses (a clause with a single literal) => must set the only literal to true
        //   * Pure literals (a variable that appears with only one polarity in all clauses) => can set it to satisfy all its clauses
        // This step reduces the search space.
        if (!Propagate(clauses, assignment))
        {
            // While doing propagation, we found a conflict, which means our current assigment doesn't work.
            // We should backtrack or if it is the root call, conclude unsat.
            solution = [];
            return false;
        }

        // The previous propagation step may have satisfied all clauses.
        if (AllClausesSatisfied(clauses, assignment))
        {
            solution = assignment;
            return true;
        }

        // We need to select which next variable to branch on.
        // We can either pick a random unassigned variable or use some heuristics.
        // We choose to use a simple heuristic: pick the variable that appears most frequently in unsatisfied clauses.
        var branchVar = GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(clauses, assignment);

        // We try both possible values for the chosen variable (true and false).
        foreach (var value in new[] { true, false })
        {
            // Clone the assignment for branching, because it will be modified in the recursive call.
            var newAssignment = (bool?[])assignment.Clone();
            newAssignment[branchVar] = value;
            if (Dpll(clauses, newAssignment, out solution))
                return true;
        }

        // Neither branch worked. Thus the current assignment doesn't work.
        solution = [];
        return false;
    }

    // Given a specific partial assignment, we try to deduce more variable assignments using unit propagation and pure literal elimination.
    private static bool Propagate(List<int[]> clauses, bool?[] assignment)
    {
        // There might be multiple rounds of propagation needed
        while (true)
        {
            var changed = false;

            //
            // Unit propagation.
            //

            // Find all unit clauses (clauses with a single unassigned literal) and set that literal to satisfy the clause.
            foreach (var clause in clauses.Where(c => !ClauseSatisfied(c, assignment)))
            {
                // Collect up to 2 unassigned literals.
                var unassigned = clause.Where(v => LiteralValue(v, assignment) is null)
                                       .Take(2)
                                       .ToArray();

                // If there are no unassigned literals but clause is unsatisfied => conflict
                if (unassigned.Length == 0)
                    return false;

                // If exactly one unassigned => must set it to satisfy the clause
                if (unassigned.Length == 1)
                {
                    int v = Math.Abs(unassigned[0]);
                    bool mustBeTrue = unassigned[0] > 0; // literal itself must become true
                    assignment[v] = mustBeTrue;
                    changed = true;
                }
            }

            //
            // Pure literal elimination.
            // Check if there are any literals that appear with only one polarity in unsatisfied clauses.
            // If we find such literals, we can set them to satisfy all their clauses.
            //

            // Gather polarity information for unassigned variables in unsatisfied clauses.
            // For every variable we find if it ever appears as positive or negative.
            var polarityInfo = clauses.Where(c => !ClauseSatisfied(c, assignment))
                                      .SelectMany(c => c)
                                      .Where(lit => assignment[Math.Abs(lit)] is null)
                                      .GroupBy(lit => Math.Abs(lit))
                                      .Select(g => new
                                      {
                                          Var = g.Key,
                                          HasPos = g.Any(l => l > 0),
                                          HasNeg = g.Any(l => l < 0)
                                      });

            // If a variable appears only as positive or only as negative, we can safely assign it to the corresponding value.
            foreach (var p in polarityInfo)
            {
                if (p.HasPos && !p.HasNeg)
                {
                    assignment[p.Var] = true;
                    changed = true;
                }
                else if (!p.HasPos && p.HasNeg)
                {
                    assignment[p.Var] = false;
                    changed = true;
                }
            }

            //
            // End of one round
            //

            // If no changes were made in this round, we are done
            if (!changed)
                return true;
        }
    }

    // Returns 0 if all variables are assigned.
    private static int GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(List<int[]> clauses, bool?[] assignment)
    {
        return clauses.Where(c => !ClauseSatisfied(c, assignment))
                      .SelectMany(c => c)
                      .Select(lit => Math.Abs(lit))
                      .Where(v => assignment[v] is null)
                      .GroupBy(v => v)
                      .Select(g => new { Var = g.Key, Count = g.Count() })
                      .MaxBy(x => x.Count)!
                      .Var;
    }

    private static bool AllClausesSatisfied(List<int[]> clauses, bool?[] assignment) =>
        clauses.All(clause => ClauseSatisfied(clause, assignment));

    private static bool ClauseSatisfied(int[] clause, bool?[] assignment) =>
        clause.Any(lit => LiteralValue(lit, assignment) == true);

    private static bool? LiteralValue(int lit, bool?[] assignment)
    {
        var v = Math.Abs(lit);
        var av = assignment[v];
        if (av == null) return null;
        return lit > 0 ? av.Value : !av.Value;
    }
}
