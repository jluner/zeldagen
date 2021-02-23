using System;

namespace zeldagen
{
    public interface IMapDisplay
    {
        void Print<T, R>(Map<T, R> map, IRoomClassifier<R> classifier) where T : TemplateBase where R : RoomBase;
    }

    public enum RoomCategory
    {
        Entrance,
        Goal,
        Battle,
        BossBattle,
        Normal
    }

    public interface IRoomClassifier<R> where R : RoomBase
    {
        RoomCategory Classify(R room);
    }

    public class GraphvizMapDisplay : IMapDisplay
    {
        public void Print<T, R>(Map<T, R> map, IRoomClassifier<R> classifier) where T : TemplateBase where R : RoomBase
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
                    case RoomCategory.Battle:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\",shape=octagon]");
                        break;
                    case RoomCategory.BossBattle:
                        Console.WriteLine($"{room.Id} [label=\"{room.ToString()}\",shape=doubleoctagon]");
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

            string Label(Hall hall) => hall.Lock is null ? null : $"label = \"{hall.Lock}\"";
        }
    }
}