using System;
using System.Collections.Generic;
using System.IO;

public class Node
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Demand { get; set; }

    public int DepotIndex { get; set; }
    public override string ToString()
    {
        return $"Node {Id}: ({X}, {Y}), Demand: {Demand}";
    }
}

public class CVRPInstance
{
    public int Dimension { get; set; }
    public int Capacity { get; set; }
    public List<Node> Nodes { get; set; } = new List<Node>();

    public void PrintNodes()
    {
        foreach (var node in Nodes)
        {
            Console.WriteLine(node);
        }
    }
}

public class Program
{
    public static void Main()
    {
        CVRPInstance instance = GetDataFromFile();
        Console.WriteLine($"Dimension: {instance.Dimension}, Capacity: {instance.Capacity}");
        instance.PrintNodes();
    }

    public static CVRPInstance GetDataFromFile()
    {
        string filePath = "C:\\Users\\tomas\\OneDrive\\Pulpit\\A\\A-n32-k5.vrp";
        CVRPInstance instance = new CVRPInstance();
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool readingCoordinates = false;
            bool readingDemands = false;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("DIMENSION"))
                {
                    instance.Dimension = int.Parse(line.Split(':')[1].Trim());
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
                        int id = int.Parse(parts[0]);
                        int x = int.Parse(parts[1]);
                        int y = int.Parse(parts[2]);

                        instance.Nodes.Add(new Node { Id = id, X = x, Y = y });
                    }
                }
                else if (readingDemands)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        int id = int.Parse(parts[0]);
                        int demand = int.Parse(parts[1]);

                        var node = instance.Nodes.Find(n => n.Id == id);
                        if (node != null)
                        {
                            node.Demand = demand;
                        }
                    }
                }
            }
        }
        return instance;
    }
}
