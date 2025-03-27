using System;
using System.IO;

namespace CVRP;
public class Program
{
    public static void Main()
    {
        PrintMenu();
    }


    private static string SelectFile()
    {
        Console.WriteLine("Select a file (1-7):");
        Console.WriteLine("1. A-n32-k5.vrp");
        Console.WriteLine("2. A-n37-k6.vrp");
        Console.WriteLine("3. A-n39-k5.vrp");
        Console.WriteLine("4. A-n45-k6.vrp");
        Console.WriteLine("5. A-n48-k7.vrp");
        Console.WriteLine("6. A-n54-k7.vrp");
        Console.WriteLine("7. A-n60-k9.vrp");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 7)
        {
            Console.WriteLine("Invalid choice. Please enter a number between 1 and 7.");
        }

        switch (choice)
        {
            case 1: return "A\\A-n32-k5.vrp";
            case 2: return "A\\A-n37-k6.vrp";
            case 3: return "A\\A-n39-k5.vrp";
            case 4: return "A\\A-n45-k6.vrp";
            case 5: return "A\\A-n48-k7.vrp";
            case 6: return "A\\A-n54-k7.vrp";
            case 7: return "A\\A-n60-k9.vrp";
            default: return "";
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine("---MENU---");
        string selectedFile = SelectFile();
        Console.WriteLine($"Selected file: {selectedFile}");
        CVRPInstance instance = GetFile(selectedFile);
        instance.PrintNodes();
        instance.CalculateDistanceMatrix();
        while (true)
        {
            Console.WriteLine("\nSelect an algorithm:");
            Console.WriteLine("1. Greedy");
            Console.WriteLine("2. Random Search");
            Console.WriteLine("3. Tabu Search");
            Console.WriteLine("4. Genetic Algorithm");
            Console.WriteLine("5. Exit");

            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 5)
            {
                Console.WriteLine("Invalid choice. Please enter a number between 1 and 5.");
            }

            if (choice == 5) break;

            switch (choice)
            {
                case 1:
                    Console.WriteLine("Greedy algorithm selected.");
                    GreedySolver grSolver = new GreedySolver(instance);
                    int[] solution = grSolver.Solve();
                    grSolver.PrintSolution(solution);
                    break;
                case 2:
                    Console.WriteLine("Random search selected.");
                    RandomSolver rSolver = new RandomSolver(instance);
                    int[][] solutions = rSolver.Solve();
                    rSolver.PrintSolution(rSolver.GetBestRoute());
                    //SaveDistancesToCSV(CalculateAllRouteDistances(instance, solutions), "C:\\Users\\pc\\source\\repos\\CVRP\\CVRP\\RandomOut.csv");
                    break;
                case 3:
                    Console.WriteLine("Tabu search selected.");
                    TabuSolver tSolver = new TabuSolver(instance);
                    int[][] tSolutions = tSolver.Solve();
                    tSolver.PrintSolution(tSolver.GetBestRoute());
                    //SaveDistancesToCSV(CalculateAllRouteDistances(instance, tSolutions), "C:\Users\tomas\source\repos\CVRP_project\CVRP\TabuOut.csv");
                    break;
                case 4:
                    Console.WriteLine("Genetic algorithm selected.");
                    GeneticSolver gSolver = new GeneticSolver(instance);
                    int[][] gSolutions = gSolver.Solve();
                    gSolver.PrintSolution(gSolver.GetBestRoute());
                    //SaveDistancesToCSV(CalculateAllRouteDistances(instance, gSolutions), "C:\Users\tomas\source\repos\CVRP_project\CVRP\GeneticOut.csv");
                    break;
            }
        }

        Console.WriteLine("Exiting menu.");
    }

    private static CVRPInstance GetFile(string path)
    {
        string filePath = path;
        CVRPInstance instance = new CVRPInstance();
        instance.Nodes = new Node[0];

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool readingCoordinates = false;
            bool readingDemands = false;
            int nodeCount = 0;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("DIMENSION"))
                {
                    instance.Dimension = int.Parse(line.Split(':')[1].Trim());
                    instance.Nodes = new Node[instance.Dimension];
                }
                else if (line.StartsWith("CAPACITY"))
                {
                    instance.Capacity = int.Parse(line.Split(':')[1].Trim());
                }
                else if (line.StartsWith("NODE_COORD_SECTION"))
                {
                    readingCoordinates = true;
                    continue;
                }
                else if (line.StartsWith("DEMAND_SECTION"))
                {
                    readingCoordinates = false;
                    readingDemands = true;
                    continue;
                }
                else if (line.StartsWith("DEPOT_SECTION") || line.StartsWith("EOF"))
                {
                    break;
                }

                if (readingCoordinates)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        int id = int.Parse(parts[0]) - 1; // Convert to 0-based index
                        int x = int.Parse(parts[1]);
                        int y = int.Parse(parts[2]);

                        instance.Nodes[id] = new Node { Id = id, X = x, Y = y };
                        nodeCount++;
                    }
                }
                else if (readingDemands)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        int id = int.Parse(parts[0]) - 1; // Convert to 0-based index
                        int demand = int.Parse(parts[1]);

                        if (instance.Nodes[id] != null)
                        {
                            instance.Nodes[id].Demand = demand;
                        }
                    }
                }
            }
        }
        return instance;
    }

    static double[] CalculateAllRouteDistances(CVRPInstance instance, int[][] allRoutes)
    {
        double[] distances = new double[allRoutes.Length];

        for (int i = 0; i < allRoutes.Length; i++)
        {
            distances[i] = CalculateTotalDistance(instance, allRoutes[i]);
        }

        return distances;
    }

    static double CalculateTotalDistance(CVRPInstance instance, int[] route)
    {
        double totalDistance = 0;

        for (int i = 0; i < route.Length - 1; i++)
        {
            totalDistance += instance.DistanceMatrix[route[i]][route[i + 1]];
        }

        return Math.Round(totalDistance, 2);
    }

    static void SaveDistancesToCSV(double[] distances, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, append: true))
        {
            foreach (double distance in distances)
            {
                writer.WriteLine(distance);
            }
        }
    }


}