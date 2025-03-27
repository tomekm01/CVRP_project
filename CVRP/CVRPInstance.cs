using System;
using System.Linq;
using System.Text;

namespace CVRP
{
    public class Node
    {
        public int Id;
        public int X;
        public int Y;
        public int Demand;
    }

    public class CVRPInstance
    {
        public int Dimension;
        public int Capacity;
        public Node[] Nodes;
        public double[][] DistanceMatrix;

        public void CalculateDistanceMatrix()
        {
            DistanceMatrix = new double[Dimension][];

            for (int i = 0; i < Dimension; i++)
            {
                DistanceMatrix[i] = new double[Dimension];

                for (int j = 0; j < Dimension; j++)
                {
                    if (i == j)
                    {
                        DistanceMatrix[i][j] = 0;
                    }
                    else //calculating the distances between nodes using Pythagoras theorem
                    {
                        double dx = Nodes[i].X - Nodes[j].X;
                        double dy = Nodes[i].Y - Nodes[j].Y;
                        DistanceMatrix[i][j] = Math.Sqrt(dx * dx + dy * dy);
                    }
                }
            }
        }

        public void PrintDistanceMatrix()
        {
            Console.WriteLine("\nDistance Matrix:");
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    Console.Write($"{DistanceMatrix[i][j]:F2}\t");
                }
                Console.WriteLine();
            }
        }

        public void PrintNodes()
        {
            Console.WriteLine($"Dimension: {Dimension}, Capacity: {Capacity}");
            foreach (var node in Nodes)
            {
                if(node.Demand == 0)
                {
                    Console.WriteLine($"Depot - node {node.Id}");
                }
                Console.WriteLine($"Node {node.Id}: ({node.X}, {node.Y}), Demand: {node.Demand}");
            }
        }
    }

}
