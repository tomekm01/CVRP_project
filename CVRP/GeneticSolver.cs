using System;

namespace CVRP
{
    public class GeneticSolver
    {
        private CVRPInstance instance;
        private Random random;
        private int populationSize = 100;
        private int generations = 100;
        private double mutationRate = 0.1; // Probability of mutation
        private double crossoverRate = 0.7; // Probability of crossover
        private int tournamentSize = 5;
        private int[] bestRoute;

        public GeneticSolver(CVRPInstance instance)
        {
            this.instance = instance;
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
                        int[] parent1 = route;
                        int[] parent2 = SelectParent(population);
                        int[] offspring = OrderedCrossover(parent1, parent2);
                        newPopulation[i] = Mutate(offspring);
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

            // Return the history of all solutions (population history)
            return solutionHistory.ToArray();
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

        // Perform the Ordered Crossover (OX) operator
        private int[] OrderedCrossover(int[] parent1, int[] parent2)
        {
            // Extract customer sequences from the parents (ignore depot visits)
            int[] seq1 = ExtractCustomers(parent1);
            int[] seq2 = ExtractCustomers(parent2);
            int len = seq1.Length; // should equal instance.Dimension - 1
            int[] offspringSeq = new int[len];

            // Choose two crossover points (for the customer sequences)
            int start = random.Next(0, len);
            int end = random.Next(start + 1, len);

            // Copy the subsequence from parent1 to offspring
            for (int i = start; i < end; i++)
            {
                offspringSeq[i] = seq1[i];
            }

            // Fill in remaining positions from parent2 in order
            int pos = end;
            for (int i = 0; i < len; i++)
            {
                int candidate = seq2[(end + i) % len];
                // Check if candidate is already in the subsequence
                bool found = false;
                for (int j = start; j < end; j++)
                {
                    if (offspringSeq[j] == candidate)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    if (pos >= offspringSeq.Length)
                        break;
                    offspringSeq[pos] = candidate;
                    pos++;
                    if (pos >= len) pos = 0;
                    // Ensure we don't overwrite the subsequence
                    if (pos >= start && pos < end)
                        pos = end;
                }
            }

            // Repair the route by inserting depot visits according to capacity constraints
            int[] repairedOffspring = RepairRoute(offspringSeq);
            return repairedOffspring;
        }



        // Perform mutation by swapping two nodes, ensuring the capacity is valid
        private int[] Mutate(int[] route)
        {
            if (random.NextDouble() > mutationRate) return route;

            // Extract the customer permutation (remove depot visits)
            int[] customers = ExtractCustomers(route);

            // Randomly swap two customers in the permutation
            int pos1 = random.Next(0, customers.Length);
            int pos2 = random.Next(0, customers.Length);
            while (pos1 == pos2)
                pos2 = random.Next(0, customers.Length);
            int temp = customers[pos1];
            customers[pos1] = customers[pos2];
            customers[pos2] = temp;

            // Repair the route: reinsert depot visits according to capacity
            int[] repairedRoute = RepairRoute(customers);

            // (Optional) Check capacity validity – it should be valid after repair
            if (!IsValidCapacity(repairedRoute))
                return route; // revert if repair failed

            return repairedRoute;
        }

        // Check if a route satisfies the capacity constraint
        private bool IsValidCapacity(int[] route)
        {
            double currentLoad = 0;
            int prevNode = 0; // Start from depot

            foreach (int node in route)
            {
                if (node == 0) continue; // Skip depot
                currentLoad += instance.Nodes[node].Demand;

                if (currentLoad > instance.Capacity)
                {
                    return false; // Exceeds vehicle capacity
                }
            }
            return true;
        }

        // Calculate the total distance of a route
        private double CalculateTotalDistance(int[] route)
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
            Console.WriteLine("Genetic Algorithm best solution:");
            Console.Write("Route: ");
            foreach (int node in bestRoute)
            {
                Console.Write(node + " -> ");
            }
            Console.WriteLine("End");
            Console.WriteLine($"Total Distance: {CalculateTotalDistance(bestRoute):F2}");
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
                Random rng = new Random();
                int nextNode;
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

        private int[] ExtractCustomers(int[] route)
        {
            // Count non-depot nodes
            int count = 0;
            for (int i = 0; i < route.Length; i++)
            {
                if (route[i] != 0) count++;
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

        private int[] RepairRoute(int[] customerSequence)
        {
            int capacity = instance.Capacity;
            int currentLoad = 0;

            // Worst-case: each customer is preceded by a depot
            int maxSize = customerSequence.Length * 2 + 1;
            int[] temp = new int[maxSize];
            int idx = 0;

            // Start with the depot
            temp[idx++] = 0;

            for (int i = 0; i < customerSequence.Length; i++)
            {
                int customer = customerSequence[i];
                int demand = instance.Nodes[customer].Demand;
                // If adding this customer exceeds capacity, insert a depot visit and reset load
                if (currentLoad + demand > capacity)
                {
                    temp[idx++] = 0;
                    currentLoad = 0;
                }
                // Add the customer
                temp[idx++] = customer;
                currentLoad += demand;
            }

            // End with the depot if not already
            if (temp[idx - 1] != 0)
            {
                temp[idx++] = 0;
            }

            // Trim the array to the actual size
            int[] repaired = new int[idx];
            Array.Copy(temp, repaired, idx);
            return repaired;
        }

        public int[] GetBestRoute()
        {
            return bestRoute;
        }
    }
}
