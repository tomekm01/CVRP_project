using System;
using System.Drawing;


namespace CVRP;

class RandomSolver
{
    private CVRPInstance instance;
    private int[][] allRoutes;
    private int[] bestRoute;
    private double bestDistance;
    private int triesAmount = 10000; 
    private Random random = new Random();
    public RandomSolver(CVRPInstance instance)
    {
        this.instance = instance;
        allRoutes = new int[triesAmount][];
        bestRoute = null;
        bestDistance = double.MaxValue;
    }
    public int[][] Solve()
    {
        for (int i = 0; i < triesAmount; i++)
        {
            int[] route = GenerateRandomRoute();
            allRoutes[i] = route;

            double distance = CalculateTotalDistance(route);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRoute = (int[])route.Clone();
            }
        }

        return allRoutes;
    }
    public int[] GetBestRoute()
    {
        return bestRoute;
    }
    public void PrintSolution(int[] route)
    {
        Console.WriteLine("Random search best solution:");
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
    private int[] GenerateRandomRoute()
    {
        int numNodes = instance.Dimension;
        int[] route = new int[numNodes + 1];
        bool[] visited = new bool[numNodes];

        route[0] = 0; // Start at depot
        visited[0] = true;

        for (int i = 1; i < numNodes; i++)
        {
            int nextNode;
            do
            {
                nextNode = random.Next(1, numNodes); // Pick a random node (excluding depot)
            } while (visited[nextNode]);

            route[i] = nextNode;
            visited[nextNode] = true;
        }

        route[numNodes] = 0; // Return to depot
        return route;
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

}

