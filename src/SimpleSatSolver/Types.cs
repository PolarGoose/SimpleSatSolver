namespace SimpleSatSolver;

public enum LiteralPolarity
{
    Positive,
    Negative
}

// Represents a literal in a clause, for example:
//   * x1
//   * ¬x2
public struct Literal
{
    public int VariableIndex { get; }
    public LiteralPolarity Polarity { get; }

    public Literal(int rawLiteral)
    {
        VariableIndex = Math.Abs(rawLiteral);
        Polarity = rawLiteral > 0 ? LiteralPolarity.Positive : LiteralPolarity.Negative;
    }

    public bool IsSatisfied(Assignment assignment)
    {
        var value = assignment.Values[VariableIndex];
        return value == AssignedValue.True && Polarity == LiteralPolarity.Positive ||
               value == AssignedValue.False && Polarity == LiteralPolarity.Negative;
    }
}

// Represents a clause for example:
//   x1 or ¬x2 or x3
public struct Clause(Literal[] literals)
{
    public IReadOnlyList<Literal> Literals => literals;
    public bool IsSatisfied(Assignment assignment) => literals.Any(lit => lit.IsSatisfied(assignment));
}

public enum AssignedValue
{
    Unassigned = 0, True, False
}

// Represents an assignment of boolean values to variables.
public struct Assignment
{
    public AssignedValue[] Values { get; }

    public Assignment(int variableCount)
    {
        Values = new AssignedValue[variableCount + 1];
    }

    public Assignment(AssignedValue[] values)
    {
        Values = values;
    }

    public void Satisfy(Literal literal) => Values[literal.VariableIndex] = literal.Polarity == LiteralPolarity.Positive ? AssignedValue.True : AssignedValue.False;

    public Assignment Clone() => new((AssignedValue[])Values.Clone());
}

// Represents a solution to the SAT problem.
public struct Solution
{
    public readonly IReadOnlyList<bool> Values { get; }

    public Solution(Assignment assignment)
    {
        // The unassigned values are treated as False
        Values = assignment.Values.Select(v => v == AssignedValue.True).ToList();
    }
}
