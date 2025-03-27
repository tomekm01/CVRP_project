using System;

namespace CVRP;
public class TabuSolver
{
    private CVRPInstance instance;
    private int[] bestRoute;
    private int[][] allRoutes;
    private int tabuSize = 300; // Number of iterations before allowing a move again
    private int iterations = 5000;
    private List<(int, int)> tabuList = new List<(int, int)>();

    public TabuSolver(CVRPInstance instance)
    {
        this.instance = instance;
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

    public int[] GetBestRoute()
    {
        return bestRoute;
    }

    private int[] GetRandomRoute()
    {
        // Pre-allocate a route array large enough to handle depot visits
        // Worst case: each customer visit is followed by a depot visit, plus start/end at depot
        int[] route = new int[instance.Dimension * 2];
        int routeIndex = 0; // Tracks current position in route array

        // Track visited nodes
        bool[] visited = new bool[instance.Dimension];
        visited[0] = true; // Depot (node 0) is visited at the start
        int unvisitedCount = instance.Dimension - 1; // Number of unvisited customers

        // Start at depot
        route[routeIndex++] = 0;
        int currentCapacity = 0; // Track current vehicle load

        // Build the route
        while (unvisitedCount > 0)
        {
            // Find a random unvisited node
            int nextNode;
            Random rng = new Random();
            do
            {
                nextNode = rng.Next(1, instance.Dimension); // Random customer node (excluding depot)
            } while (visited[nextNode]);

            // Check if adding the next node exceeds vehicle capacity
            int demand = instance.Nodes[nextNode].Demand; // Assuming Demands is an array in instance
            if (currentCapacity + demand <= instance.Capacity) // Assuming Capacity is a property in instance
            {
                // Add the node to the route
                route[routeIndex++] = nextNode;
                currentCapacity += demand;
                visited[nextNode] = true;
                unvisitedCount--;
            }
            else
            {
                // If capacity is exceeded, return to depot
                route[routeIndex++] = 0;
                currentCapacity = 0; // Reset capacity
            }
        }

        // Ensure the route ends at the depot
        if (route[routeIndex - 1] != 0)
        {
            route[routeIndex++] = 0;
        }

        // Trim the route array to the actual size
        int[] trimmedRoute = new int[routeIndex];
        Array.Copy(route, trimmedRoute, routeIndex);

        return trimmedRoute;
    }

    private int[] GetBestNeighbor(int[] route)
    {
        int numNodes = instance.Dimension;
        int[] bestNeighbor = (int[])route.Clone();
        double bestDistance = double.MaxValue;
        (int, int) bestMove = (-1, -1); // Track move

        for (int i = 1; i < numNodes - 1; i++)
        {
            for (int j = i + 1; j < numNodes; j++)
            {
                if (!IsTabu(i, j)) // Check tabu move
                {
                    int[] newRoute = Swap(route, i, j);

                    // 🔴 NEW: Check if the new route is valid under capacity constraints
                    if (!IsValidRoute(newRoute)) continue;

                    double newDistance = CalculateTotalDistance(newRoute);

                    if (newDistance < bestDistance)
                    {
                        bestNeighbor = (int[])newRoute.Clone();
                        bestDistance = newDistance;
                        bestMove = (i, j); // Save best move
                    }
                }
            }
        }

        // Add best move to tabu list
        if (bestMove != (-1, -1))
        {
            tabuList.Add(bestMove);
            if (tabuList.Count > tabuSize)
                tabuList.RemoveAt(0); // Maintain size
        }

        return bestNeighbor;
    }



    private int[] Swap(int[] route, int i, int j)
    {
        if (route[i] == 0 || route[j] == 0) return route; // Don't swap depot
        int[] newRoute = (int[])route.Clone();
        (newRoute[i], newRoute[j]) = (newRoute[j], newRoute[i]);
        return newRoute;
    }

    private bool IsTabu(int i, int j)
    {
        return tabuList.Contains((i, j)) || tabuList.Contains((j, i)); // Swap is bidirectional
    }

    private double CalculateTotalDistance(int[] route)
    {
        double totalDistance = 0;
        for (int i = 0; i < route.Length - 1; i++)
        {
            totalDistance += instance.DistanceMatrix[route[i]][route[i + 1]];
        }
        return Math.Round(totalDistance, 2);
    }

    public void PrintSolution(int[] route)
    {
        Console.WriteLine("Tabu Search best solution:");
        Console.Write("Route: ");
        foreach (int node in bestRoute)
        {
            Console.Write(node + " -> ");
        }
        Console.WriteLine("End");
        Console.WriteLine($"Total Distance: {GetTotalDistanceAsString(CalculateTotalDistance(bestRoute))}");
    }

    public string GetTotalDistanceAsString(double totalDistance)
    {
        return totalDistance.ToString("F2");
    }

    private (int, int) FindLastMove(int[] oldRoute, int[] newRoute)
    {
        for (int i = 0; i < oldRoute.Length; i++)
        {
            if (oldRoute[i] != newRoute[i])
            {
                for (int j = i + 1; j < oldRoute.Length; j++)
                {
                    if (oldRoute[j] != newRoute[j] && oldRoute[i] == newRoute[j] && oldRoute[j] == newRoute[i])
                    {
                        return (i, j); // Swap (i, j) was performed
                    }
                }
            }
        }
        return (-1, -1); // No swap detected (shouldn't happen in a valid move)
    }

    private bool IsValidRoute(int[] route)
    {
        int vehicleCapacity = instance.Capacity;
        int currentLoad = 0;

        for (int i = 1; i < route.Length; i++) // Start from 1 (skip depot)
        {
            int node = route[i];

            if (node == 0) // If we reach depot, reset the load
            {
                currentLoad = 0;
            }
            else
            {
                currentLoad += instance.Nodes[node].Demand; // Add demand of this customer

                if (currentLoad > vehicleCapacity) // If overloaded, this route is invalid
                    return false;
            }
        }

        return true; // If we never exceeded capacity, the route is valid
    }

}
