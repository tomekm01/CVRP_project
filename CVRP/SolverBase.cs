using System;

namespace CVRP;

public class SolverBase
{
    protected CVRPInstance instance;
    protected int[] bestRoute;
    protected int[][] allRoutes;

    public SolverBase(CVRPInstance instance)
    {
        this.instance = instance;
    }
    // Generate a random route by creating a random permutation of customers and repairing it.
    protected int[] GetRandomRoute()
    {
        int[] custPerm = GetRandomCustomerPermutation();
        return RepairRoute(custPerm);
    }


    // Repair a customer sequence by inserting depot visits (0) so that:
    // - The route starts and ends with depot,
    // - Depot visits are inserted whenever the cumulative demand would exceed capacity.
    protected int[] RepairRoute(int[] customerSequence)
    {
        int capacity = instance.Capacity;
        int currentLoad = 0;
        int n = customerSequence.Length;
        // Worst-case: each customer is preceded by a depot, plus start depot
        int maxSize = 2 * n + 1;
        int[] temp = new int[maxSize];
        int idx = 0;

        // Start with depot
        temp[idx++] = 0;

        for (int i = 0; i < n; i++)
        {
            int customer = customerSequence[i];
            int demand = instance.Nodes[customer].Demand;
            // If adding this customer exceeds capacity, insert depot and reset load
            if (currentLoad + demand > capacity)
            {
                temp[idx++] = 0;
                currentLoad = 0;
            }
            temp[idx++] = customer;
            currentLoad += demand;
        }
        // End with depot if needed
        if (temp[idx - 1] != 0)
        {
            temp[idx++] = 0;
        }

        int[] repaired = new int[idx];
        Array.Copy(temp, repaired, idx);
        return repaired;
    }

    // Generate a random permutation of customers (customers are numbered 1 to instance.Dimension-1)
    private int[] GetRandomCustomerPermutation()
    {
        int n = instance.Dimension - 1; // number of customers
        Random rnd = new Random();
        int[] customers = new int[n];
        for (int i = 0; i < n; i++)
        {
            customers[i] = i + 1;
        }
        // Fisher-Yates shuffle
        for (int i = n - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            int temp = customers[i];
            customers[i] = customers[j];
            customers[j] = temp;
        }
        return customers;
    }
    public int[] GetBestRoute()
    {
        return bestRoute;
    }

    // Calculate the total distance of a route
    protected double CalculateTotalDistance(int[] route)
    {
        double totalDistance = 0;
        for (int i = 0; i < route.Length - 1; i++)
        {
            totalDistance += instance.DistanceMatrix[route[i]][route[i + 1]];
        }
        return totalDistance;
    }

    // Print the best solution
    public void PrintSolution(int[] bestRoute)
    {
        Console.WriteLine("Best solution:");
        Console.Write("Route: ");
        foreach (int node in bestRoute)
        {
            Console.Write(node + " -> ");
        }
        Console.WriteLine("End");
        Console.WriteLine($"Total Distance: {CalculateTotalDistance(bestRoute):F2}");
    }

    // Extract all customer nodes from a route (ignoring depot visits)
    protected int[] ExtractCustomers(int[] route)
    {
        int count = 0;
        for (int i = 0; i < route.Length; i++)
        {
            if (route[i] != 0)
                count++;
        }
        int[] result = new int[count];
        int idx = 0;
        for (int i = 0; i < route.Length; i++)
        {
            if (route[i] != 0)
            {
                result[idx++] = route[i];
            }
        }
        return result;
    }
}
