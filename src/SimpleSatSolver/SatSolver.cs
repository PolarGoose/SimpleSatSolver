namespace SimpleSatSolver;

public static class SatSolver
{
    public static bool Solve(int varCount, List<int[]> clauses, out bool[] solution)
    {
        if (Dpll(varCount, clauses, new bool?[varCount + 1], out bool?[] finalAssign))
        {
            solution = [.. finalAssign.Select(b => b ?? false)];
            return true;
        }

        solution = [];
        return false;
    }

    // Implements `Davis–Putnam–Logemann–Loveland` algorithm with unit propagation and pure literal elimination.
    private static bool Dpll(int varCount, List<int[]> clauses, bool?[] assignment, out bool?[] solution)
    {
        // At this point not all variables in `assignment` are assigned yet.
        // Given partial assignment we can try to deduce values for some unassigned variables using:
        //   * Unit clauses (a clause with a single literal) => must set the only literal to true
        //   * Pure literals (a variable that appears with only one polarity in all clauses) => can set it to satisfy all its clauses
        // This step reduces the search space.
        if (!Propagate(varCount, clauses, assignment))
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
        int branchVar = GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(varCount, clauses, assignment);
        if (branchVar == 0)
        {
            // All variables are already set, but not all clauses are satisfied.
            // Thus, the current assignment doesn't work.
            solution = [];
            return false;
        }

        // We try both possible values for the chosen variable (true and false).
        foreach (var value in new[] { true, false })
        {
            // Clone the assignment for branching, because it will be modified in the recursive call.
            var newAssignment = (bool?[])assignment.Clone();
            newAssignment[branchVar] = value;
            if (Dpll(varCount, clauses, newAssignment, out solution))
                return true;
        }

        // Neither branch worked. Thus the current assignment doesn't work.
        solution = [];
        return false;
    }

    // Given a specific partial assignment, we try to deduce more variable assignments using unit propagation and pure literal elimination.
    private static bool Propagate(int varCount, List<int[]> clauses, bool?[] assignment)
    {
        // There might be multiple rounds of propagation needed
        while (true)
        {
            bool changed = false;

            // Unit propagation.
            // We find all unit clauses (clauses with a single unassigned literal) and set that literal to satisfy the clause.
            foreach (var clause in clauses)
            {
                if (ClauseSatisfied(clause, assignment)) continue;

                // Count unassigned literals and track the last unassigned literal
                int unassignedCount = 0;
                int lastUnassignedLit = 0;
                foreach (int lit in clause)
                {
                    if (LiteralValue(lit, assignment) == null)
                    {
                        unassignedCount++;
                        lastUnassignedLit = lit;
                        if (unassignedCount > 1) break;
                    }
                }

                // If no unassigned literals and the clause not satisfied => conflict
                if (unassignedCount == 0) return false;

                // If exactly one unassigned => must set it to satisfy the clause
                if (unassignedCount == 1)
                {
                    int v = Math.Abs(lastUnassignedLit);
                    bool mustBeTrue = lastUnassignedLit > 0; // literal itself must become true
                    assignment[v] = mustBeTrue;
                    changed = true;
                }
            }

            // Pure literal elimination.
            // Check if there are any literals that appear with only one polarity in unsatisfied clauses.
            // If we find such literals, we can set them to satisfy all their clauses.
            var polarity = new int[varCount + 1]; // 0 none, +1 seen positive, -1 seen negative, 2 both
            foreach (var clause in clauses)
            {
                if (ClauseSatisfied(clause, assignment)) continue;

                foreach (int lit in clause)
                {
                    int v = Math.Abs(lit);
                    if (assignment[v] != null) continue;

                    int sign = lit > 0 ? +1 : -1;
                    if (polarity[v] == 0) polarity[v] = sign;
                    else if (polarity[v] != sign) polarity[v] = 2; // both polarities
                }
            }
            // Set all pure literals to their satisfying value
            for (int v = 1; v <= varCount; v++)
            {
                if (assignment[v] != null) continue;
                if (polarity[v] == 1)
                {
                    assignment[v] = true;
                    changed = true;
                }
                else if (polarity[v] == -1)
                {
                    assignment[v] = false;
                    changed = true;
                }
            }

            // If no changes were made in this round, we are done
            if (!changed)
                return true;
        }
    }

    private static int GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(int varCount, List<int[]> clauses, bool?[] assignment)
    {
        var count = new int[varCount + 1];

        foreach (var clause in clauses)
        {
            if (ClauseSatisfied(clause, assignment)) continue;

            foreach (var lit in clause)
            {
                var v = Math.Abs(lit);
                if (assignment[v] == null) count[v]++;
            }
        }

        return Enumerable.Range(1, varCount)
                         .Where(v => assignment[v] == null)
                         .MaxBy(v => count[v]);
    }

    private static bool AllClausesSatisfied(List<int[]> clauses, bool?[] assignment) {
        foreach (var clause in clauses)
            if (!ClauseSatisfied(clause, assignment))
                return false;
        return true;
    }

    private static bool ClauseSatisfied(int[] clause, bool?[] assignment) {
        foreach (int lit in clause) {
            bool? val = LiteralValue(lit, assignment);
            if (val == true) return true;
        }
        return false;
    }

    private static bool? LiteralValue(int lit, bool?[] assignment)
    {
        int v = Math.Abs(lit);
        bool? av = assignment[v];
        if (av == null) return null;
        bool positive = lit > 0;
        return positive ? av.Value : !av.Value;
    }
}
