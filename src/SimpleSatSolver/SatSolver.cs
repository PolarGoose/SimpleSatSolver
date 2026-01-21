namespace SimpleSatSolver;

public static class SatSolver
{
    public static Solution? Solve(int variableCount, IReadOnlyList<Clause> clauses)
    {
        return Dpll(clauses, new Assignment(variableCount));
    }

    // Implements `Davis–Putnam–Logemann–Loveland` algorithm with unit propagation and pure literal elimination.
    private static Solution? Dpll(IReadOnlyList<Clause> clauses, Assignment assignment)
    {
        // At this point, not all variables in `assignment` are assigned yet.
        // Given partial assignment, we can try to deduce values for some unassigned variables using:
        //   * Unit clauses (a clause with a single literal) => must set the only literal to true
        //   * Pure literals (a variable that appears with only one polarity in all clauses) => can set it to satisfy all its clauses
        // This step reduces the search space.
        if (!Propagate(clauses, assignment))
        {
            // While doing propagation, we found a conflict, which means our current assignment doesn't work.
            // We should backtrack, or if it is the root call, conclude unsat.
            return null;
        }

        // The previous propagation step may have satisfied all clauses.
        if (AllClausesSatisfied(clauses, assignment))
        {
            return new Solution(assignment);
        }

        // We need to select which next variable to branch on.
        // We can either pick a random unassigned variable or use some heuristics.
        // We choose to use a simple heuristic: pick the variable that appears most frequently in unsatisfied clauses.
        // In practice, this heuristic speeds up solving by about 5 times.
        var branchVariableIndex = GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(clauses, assignment);

        // We try both possible values for the chosen variable (true and false).
        foreach (var value in new[] { AssignedValue.True, AssignedValue.False })
        {
            // Clone the assignment for branching, because it will be modified in the recursive call.
            var newAssignment = assignment.Clone();
            newAssignment.Values[branchVariableIndex] = value;

            var solution = Dpll(clauses, newAssignment);
            if (solution.HasValue)
                return solution;
        }

        // Neither branch worked. Thus, the current assignment doesn't work.
        return null;
    }

    // Given a specific partial assignment, we try to deduce more variable assignments using unit propagation and pure literal elimination.
    private static bool Propagate(IReadOnlyList<Clause> clauses, Assignment assignment)
    {
        // There might be multiple rounds of propagation needed
        while (true)
        {
            var changed = false;

            //
            // Unit propagation.
            //

            // Find all unit clauses (clauses with a single unassigned literal) and set that literal to satisfy the clause.
            foreach (var clause in clauses.Where(clause => !clause.IsSatisfied(assignment)))
            {
                // Collect up to 2 unassigned literals.
                var unassigned = clause.Literals.Where(literal => assignment.Values[literal.VariableIndex] == AssignedValue.Unassigned)
                                                .Take(2)
                                                .ToArray();

                // If there are no unassigned literals but the clause is unsatisfied => conflict
                if (unassigned.Length == 0)
                    return false;

                // If exactly one unassigned => must set it to satisfy the clause
                if (unassigned.Length == 1)
                {
                    assignment.Satisfy(unassigned[0]);
                    changed = true;
                }
            }

            //
            // Pure literal elimination.
            // Check if any literals appear with only one polarity in unsatisfied clauses.
            // If we find such literals, we can set them to satisfy all their clauses.
            //

            // Gather polarity information for unassigned variables in unsatisfied clauses.
            // For every variable, we find if it ever appears as positive or negative.
            var polarityInfo = clauses.Where(clause => !clause.IsSatisfied(assignment))
                                      .SelectMany(clause => clause.Literals)
                                      .Where(literal => assignment.Values[literal.VariableIndex] == AssignedValue.Unassigned)
                                      .GroupBy(literal => literal.VariableIndex)
                                      .Select(g => new
                                      {
                                          VariableIndex = g.Key,
                                          HasPos = g.Any(literal => literal.Polarity == LiteralPolarity.Positive),
                                          HasNeg = g.Any(literal => literal.Polarity == LiteralPolarity.Negative)
                                      });

            // If a variable appears only as positive or only as negative, we can safely assign it to the corresponding value.
            foreach (var p in polarityInfo)
            {
                if (p.HasPos && !p.HasNeg)
                {
                    assignment.Values[p.VariableIndex] = AssignedValue.True;
                    changed = true;
                }
                else if (!p.HasPos && p.HasNeg)
                {
                    assignment.Values[p.VariableIndex] = AssignedValue.False;
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

    private static int GetUnnasignedVariableThatAppearsInMostUnsatisfiedClauses(IReadOnlyList<Clause> clauses, Assignment assignment)
    {
        return clauses.Where(clause => !clause.IsSatisfied(assignment))
                      .SelectMany(clause => clause.Literals)
                      .Where(literal => assignment.Values[literal.VariableIndex] == AssignedValue.Unassigned)
                      .GroupBy(literal => literal.VariableIndex)
                      .Select(g => new { Var = g.Key, Count = g.Count() })
                      .MaxBy(x => x.Count)!
                      .Var;
    }

    private static bool AllClausesSatisfied(IReadOnlyList<Clause> clauses, Assignment assignment) =>
        clauses.All(clause => clause.IsSatisfied(assignment));
}
