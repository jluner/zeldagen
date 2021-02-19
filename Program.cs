using System;

namespace zeldagen
{
    class Program
    {
        static void Main(string[] args)
        {
            IMapGrammar grammar = new post30.Post30Grammar();

            Console.WriteLine("Generating map...");
            var map = grammar.GenerateMap();

            // Collapse rooms of the same kind
            map.Reduce();

            Console.WriteLine("Completed map of {0} rooms", map.Rooms.Count);

            //graphviz output
            new GraphvizMapDisplay().Print(map);
        }
    }
}
