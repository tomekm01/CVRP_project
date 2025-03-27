using System;

namespace CVRP;

public class GeneticSolver : SolverBase
{
    private int populationSize = 200;
    private int generations = 50;
    private double mutationRate = 0.05;
    private double crossoverRate = 0.92;
    private int tournamentSize = 7;
    private Random random;
    public GeneticSolver(CVRPInstance instance) : base(instance)
    {
        random = new Random();
    }

    public int[][] Solve()
    {
        // Initialize population as an array of routes
        int[][] population = InitializePopulation();
        double bestDistance = double.MaxValue;

        // Create a list to store the history of all solutions
        List<int[]> solutionHistory = new List<int[]>();

        for (int gen = 0; gen < generations; gen++)
        {
            // Evaluate population
            int[][] newPopulation = new int[populationSize][];
            for (int i = 0; i < populationSize; i++)
            {
                int[] route = population[i];
                double routeDistance = CalculateTotalDistance(route);

                if (routeDistance < bestDistance)
                {
                    bestRoute = (int[])route.Clone();
                    bestDistance = routeDistance;
                }

                // Selection and crossover/mutation
                if (random.NextDouble() < crossoverRate)
                {
                    // Perform crossover
                    // Select first parent
                    int[] parent1 = SelectParent(population);

                    int[] parent2;
                    do
                    {
                        parent2 = SelectParent(population);
                    } while (parent1 == parent2); // Ensure they are not the same
                    int[] offspring = OrderedCrossover(parent1, parent2);
                    if (random.NextDouble() < mutationRate)
                    //Perform mutation
                    {
                        newPopulation[i] = Mutate(offspring);
                    }
                    else
                    {
                        newPopulation[i] = offspring;
                    }
                }
                else
                {
                    // Keep the current individual
                    newPopulation[i] = route;
                }
            }

            // Add the current population to the history
            solutionHistory.AddRange(newPopulation); // Store all individuals of this generation

            // Replace old population with new one
            population = newPopulation;
        }
        allRoutes = solutionHistory.ToArray();
        // Return the history of all solutions (population history)
        return allRoutes;
    }

    // Create initial population with random routes
    private int[][] InitializePopulation()
    {
        int[][] population = new int[populationSize][];
        for (int i = 0; i < populationSize; i++)
        {
            population[i] = GetRandomRoute();
        }
        return population;
    }

    // Select a parent for crossover using tournament selection
    private int[] SelectParent(int[][] population)
    {
        int[][] tournament = new int[tournamentSize][];
        for (int i = 0; i < tournamentSize; i++)
        {
            tournament[i] = population[random.Next(population.Length)];
        }

        // Sort tournament by fitness (ascending order of distance)
        Array.Sort(tournament, (a, b) => CalculateTotalDistance(a).CompareTo(CalculateTotalDistance(b)));

        return tournament[0]; // Return the best (shortest distance) route from the tournament
    }

    // Perform the Capacity - aware Ordered Crossover (OX) operator
    private int[] OrderedCrossover(int[] parent1, int[] parent2)
    {
        int numNodes = instance.Dimension;
        int[] offspring = new int[numNodes - 1]; // Exclude depot
        bool[] visited = new bool[numNodes]; // Track visited nodes (including depot)

        // Step 1: Extract customers from both parents (no depots)
        int[] noDepotParent1 = ExtractCustomers(parent1);
        int[] noDepotParent2 = ExtractCustomers(parent2);

        // Step 2: Randomly select a subsequence from parent1's customers
        int start = random.Next(0, noDepotParent1.Length);
        int end = random.Next(start + 1, noDepotParent1.Length);

        // Mark uninitialized positions
        Array.Fill(offspring, -1);

        // Copy the subsequence from parent1 into offspring
        for (int i = start; i < end; i++)
        {
            offspring[i] = noDepotParent1[i];
            visited[offspring[i]] = true;
        }

        // Step 3: Fill remaining positions from parent2 (only customers not already in offspring)
        int fillIndex = 0;
        for (int i = 0; i < noDepotParent2.Length; i++)
        {
            if (!visited[noDepotParent2[i]])
            {
                // Find the next empty position in offspring
                while (offspring[fillIndex] != -1)
                {
                    fillIndex++;
                }
                offspring[fillIndex] = noDepotParent2[i];
                visited[noDepotParent2[i]] = true;
            }
        }

        // Step 4: Rebuild the full route by adding depots back and ensuring legality
        int[] legalOffspring = RepairRoute(offspring);

        return legalOffspring;
    }



    // Perform mutation by swapping two nodes
    private int[] Mutate(int[] route)
    {
        // Step 1: Extract customers (remove depots)
        int[] mutatedRoute = new int[route.Length];
        int[] noDepotRoute = ExtractCustomers(route);

        // Step 2: Swap two random places in the noDepotRoute
        int idx1 = random.Next(0, noDepotRoute.Length);
        int idx2 = random.Next(0, noDepotRoute.Length);

        // Ensure idx1 and idx2 are different
        while (idx1 == idx2)
        {
            idx2 = random.Next(0, noDepotRoute.Length);
        }

        // Swap the customers at idx1 and idx2
        int temp = noDepotRoute[idx1];
        noDepotRoute[idx1] = noDepotRoute[idx2];
        noDepotRoute[idx2] = temp;

        // Step 3: Rebuild the mutated route with depots and enforce capacity constraints
        mutatedRoute = RepairRoute(noDepotRoute);
        return mutatedRoute;
    }

}
