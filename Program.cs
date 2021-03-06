﻿using System;

namespace zeldagen
{
    class Program
    {
        static void Main(string[] args)
        {
            var grammar = new post24.Post24Grammar();

            Console.WriteLine("Generating map...");
            var map = grammar.GenerateMap();

            // Collapse rooms of the same kind
            map.Reduce();

            Console.WriteLine("Completed map of {0} rooms", map.Rooms.Count);

            //graphviz output
            new GraphvizMapDisplay().Print(map, grammar.Classifier);
        }
    }
}
