using System;
using System.Drawing;


namespace CVRP;

class RandomSolver : SolverBase
{
    private double bestDistance;
    private int triesAmount = 10000; 
    public RandomSolver(CVRPInstance instance) : base(instance)
    {
        allRoutes = new int[triesAmount][];
        bestDistance = double.MaxValue;
    }
    public int[][] Solve()
    {
        for (int i = 0; i < triesAmount; i++)
        {
            int[] route = GetRandomRoute();
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
}

