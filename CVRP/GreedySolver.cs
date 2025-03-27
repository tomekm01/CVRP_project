namespace CVRP;

class GreedySolver
{
    private CVRPInstance instance;

    public GreedySolver(CVRPInstance instance)
    {
        this.instance = instance;
    }

    public int[] Solve()
    {
        int numNodes = instance.Dimension;
        bool[] visited = new bool[numNodes]; // Track visited customers
        List<int> route = new List<int>();   // The full route

        int current = 0; // Start at depot
        int remainingCapacity = instance.Capacity;
        visited[current] = true;
        route.Add(current);

        int visitedCount = 0; // Track how many customer nodes have been fully visited

        while (visitedCount < numNodes - 1) // Excluding depot (node 0)
        {
            int nextNode = -1;
            double minDistance = double.MaxValue; //Initialize minDistance

            // Find the closest unvisited customer within capacity limits
            for (int i = 1; i < numNodes; i++) // Skip depot (0)
            {
                if (!visited[i] && instance.Nodes[i].Demand <= remainingCapacity) //Conditions for greedy pick
                {
                    double distance = instance.DistanceMatrix[current][i];
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextNode = i;
                    }
                }
            }

            if (nextNode == -1) // No valid next node, return to depot
            {
                route.Add(0); // Go back to depot
                remainingCapacity = instance.Capacity; // Refill capacity
                current = 0;
            }
            else
            {
                route.Add(nextNode);
                visited[nextNode] = true;
                visitedCount++; // Only increase when a new customer node is fully visited
                remainingCapacity -= instance.Nodes[nextNode].Demand;
                current = nextNode;
            }
        }

        // Ensure we return to the depot at the end
        if (current != 0) route.Add(0);

        return route.ToArray();
    }

    private double CalculateTotalDistance(int[] route)
    {
        double totalDistance = 0;
        for (int i = 0; i < route.Length - 1; i++)
        {
            totalDistance += instance.DistanceMatrix[route[i]][route[i + 1]];
        }
        return totalDistance;
    }

    public string GetTotalDistanceAsString(double totalDistance)
    {
        return totalDistance.ToString("F2");
    }
    public void PrintSolution(int[] route)
    {
        Console.WriteLine("Greedy Algorithm Solution:");
        Console.Write("Route: ");
        foreach (int node in route)
        {
            Console.Write(node + " -> ");
        }
        Console.WriteLine("End");
        Console.WriteLine($"Total Distance: {GetTotalDistanceAsString(CalculateTotalDistance(route))}");
    }
}

