using System;

namespace zeldagen
{
    public interface IMapDisplay
    {
        void Print(Map map);
    }

    public class GraphvizMapDisplay : IMapDisplay
    {
        public void Print(Map map)
        {
            Console.WriteLine("strict digraph dungeon {");

            // add rooms
            foreach (var room in map.Rooms)
            {
                switch (room.Kind)
                {
                    case RoomType.Entrance:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\",shape=point]");
                        break;
                    case RoomType.Goal:
                    case RoomType.BonusGoal:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\",peripheries=2]");
                        break;
                    default:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\"]");
                        break;
                }
            }

            foreach (var hall in map.Halls)
            {
                string label = Label(hall);
                switch (hall.Direction)
                {
                    case Direction.Both:
                        if (label is null)
                            Console.WriteLine($"{((Room)hall.From).Id} -> {((Room)hall.To).Id} [dir=both]");
                        else
                            Console.WriteLine($"{((Room)hall.From).Id} -> {((Room)hall.To).Id} [dir=both,{label}]");
                        break;
                    case Direction.Forward:
                        if (label is null)
                            Console.WriteLine($"{((Room)hall.From).Id} -> {((Room)hall.To).Id}");
                        else
                            Console.WriteLine($"{((Room)hall.From).Id} -> {((Room)hall.To).Id} [{label}]");
                        break;
                    case Direction.Back:
                        if (label is null)
                            Console.WriteLine($"{((Room)hall.To).Id} -> {((Room)hall.From).Id}");
                        else
                            Console.WriteLine($"{((Room)hall.To).Id} -> {((Room)hall.From).Id} [{label}]");
                        break;
                }
            }

            Console.WriteLine("}");

            string Label(Hall hall)
            {
                if (hall.Secret) return "label = \"?\"";
                if (hall.Key.HasValue) return $"label = \"k{hall.Key}\"";
                if (hall.State.HasValue) return $"label = \"s{hall.State}\"";
                return null;
            }
        }
    }
}