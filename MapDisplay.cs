using System;

namespace zeldagen
{
    public interface IMapDisplay
    {
        void Print<T, R>(Map<T, R> map, IRoomClassifier<R> classifier);
    }

    public enum RoomCategory
    {
        Entrance,
        Goal,
        Normal
    }

    public interface IRoomClassifier<R>
    {
        RoomCategory Classify(Room<R> room);
    }

    public class GraphvizMapDisplay : IMapDisplay
    {
        public void Print<T, R>(Map<T, R> map, IRoomClassifier<R> classifier)
        {
            Console.WriteLine("strict digraph dungeon {");

            // add rooms
            foreach (var room in map.Rooms)
            {
                switch (classifier.Classify(room))
                {
                    case RoomCategory.Entrance:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\",shape=point]");
                        break;
                    case RoomCategory.Goal:
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
                            Console.WriteLine($"{hall.From.Id} -> {hall.To.Id} [dir=both]");
                        else
                            Console.WriteLine($"{hall.From.Id} -> {hall.To.Id} [dir=both,{label}]");
                        break;
                    case Direction.Forward:
                        if (label is null)
                            Console.WriteLine($"{hall.From.Id} -> {hall.To.Id}");
                        else
                            Console.WriteLine($"{hall.From.Id} -> {hall.To.Id} [{label}]");
                        break;
                    case Direction.Back:
                        if (label is null)
                            Console.WriteLine($"{hall.To.Id} -> {hall.From.Id}");
                        else
                            Console.WriteLine($"{hall.To.Id} -> {hall.From.Id} [{label}]");
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