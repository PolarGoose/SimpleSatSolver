# SimpleSatSolver
A very simple and readable (I hope) implementation of a SAT solver in C# using the [Davis–Putnam–Logemann–Loveland (DPLL)](https://en.wikipedia.org/wiki/DPLL_algorithm) algorithm.

# Details
[SatSolver.cs](https://github.com/PolarGoose/SimpleSatSolver/blob/main/src/SimpleSatSolver/SatSolver.cs) contains whole implementation with detailed comments.<br>
The implementation passes benchmarks from [SATLIB - Benchmark Problems](https://www.cs.ubc.ca/~hoos/SATLIB/benchm.html)

# Limitations
* The performance is very poor because the code prioritizes clarity over efficiency. It heavily relies on LINQ.
  * It takes up to 3 seconds to solve a benchmark problem with 100 variables and 430 clauses on my PC.
* The algorithm is implemented using recursion. It may hit stack overflow (I haven't encountered this in any of the benchmarks I ran).
