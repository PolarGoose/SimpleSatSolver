# SimpleSatSolver
A very simple implementation of a SAT solver in C# using the [Davis–Putnam–Logemann–Loveland (DPLL)](https://en.wikipedia.org/wiki/DPLL_algorithm) algorithm.

# Details
[SatSolver.cs](https://github.com/PolarGoose/SimpleSatSolver/blob/main/src/SimpleSatSolver/SatSolver.cs) contains whole implementation with detailed comments.<br>
The implementation passes all benchmarks from [SATLIB - Benchmark Problems](https://www.cs.ubc.ca/~hoos/SATLIB/benchm.html)

# Limitations
* The solver struggles with problems that have more than 50 variables.
* The algorithm is implemented using recursion, thus it may hit stack overflow for deep recursions (however, at least for all benchmarks that I ran, I haven't encountered this situation).
