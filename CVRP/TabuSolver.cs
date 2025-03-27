using System;

namespace CVRP;
public class TabuSolver : SolverBase
{
    private int tabuSize = 300; // Number of iterations before allowing a move again
    private int iterations = 10000;
    private List<(int, int)> tabuList = new List<(int, int)>();

    public TabuSolver(CVRPInstance instance) : base(instance)
    {
        allRoutes = new int[iterations][];
    }

    public int[][] Solve()
    {
        int numNodes = instance.Dimension;
        int[] currentRoute = GetRandomRoute();
        bestRoute = (int[])currentRoute.Clone();
        double bestDistance = CalculateTotalDistance(bestRoute);

        int tabuIndex = 0;

        for (int iter = 0; iter < iterations; iter++)
        {
            int[] newRoute = GetBestNeighbor(currentRoute);
            double newDistance = CalculateTotalDistance(newRoute);

            // Store this route in the iteration history
            allRoutes[iter] = (int[])newRoute.Clone();

            // If new route is better, update best
            if (newDistance < bestDistance)
            {
                bestRoute = (int[])newRoute.Clone();
                bestDistance = newDistance;
            }

            // Extract the move that generated newRoute (e.g., the best swap made in this iteration)
            (int, int) lastMove = FindLastMove(currentRoute, newRoute);

            // Add the move to the tabu list
            tabuList.Add(lastMove);

            // Ensure the tabu list does not exceed tabuSize
            if (tabuList.Count > tabuSize)
            {
                tabuList.RemoveAt(0); // Remove oldest move (FIFO behavior)
            }

            // Move to the new solution
            currentRoute = (int[])newRoute.Clone();
        }

        return allRoutes;
    }

    private int[] GetBestNeighbor(int[] route)
    {
        // Extract the customer permutation from the route.
        int[] custSeq = ExtractCustomers(route); // should be of length instance.Dimension - 1
        int n = custSeq.Length;
        int[] bestNeighborPermutation = (int[])custSeq.Clone();
        double bestDistance = double.MaxValue;
        (int, int) bestMove = (-1, -1);

        // Consider all swaps on the customer permutation.
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (!IsTabu(i, j))
                {
                    int[] newPerm = (int[])custSeq.Clone();
                    // Swap positions i and j.
                    int temp = newPerm[i];
                    newPerm[i] = newPerm[j];
                    newPerm[j] = temp;
                    // Repair the candidate permutation into a legal route.
                    int[] candidateRoute = RepairRoute(newPerm);
                    double candidateDistance = CalculateTotalDistance(candidateRoute);

                    if (candidateDistance < bestDistance)
                    {
                        bestDistance = candidateDistance;
                        bestNeighborPermutation = newPerm;
                        bestMove = (i, j);
                    }
                }
            }
        }

        // Add the best move to the Tabu list.
        if (bestMove != (-1, -1))
        {
            tabuList.Add(bestMove);
            if (tabuList.Count > tabuSize)
                tabuList.RemoveAt(0);
        }
        // Return the repaired route based on the best neighbor permutation.
        return RepairRoute(bestNeighborPermutation);
    }
    private bool IsTabu(int i, int j)
    {
        return tabuList.Contains((i, j)) || tabuList.Contains((j, i)); // Swap is bidirectional
    }
    private (int, int) FindLastMove(int[] oldRoute, int[] newRoute)
    {
        for (int i = 0; i < oldRoute.Length; i++)
        {
            if (oldRoute[i] != newRoute[i])
            {
                for (int j = i + 1; j < oldRoute.Length; j++)
                {
                    if (oldRoute[j] != newRoute[j] && oldRoute[i] == newRoute[j])
                    {
                        return (i, j); // Swap (i, j) was performed
                    }
                }
            }
        }
        return (-1, -1); // No swap detected (shouldn't happen in a valid move)
    }

}
