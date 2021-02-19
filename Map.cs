using System;
using System.Collections.Generic;
using System.Linq;

namespace zeldagen
{
    public class Map<T, R>
    {
        private int key = 1;
        private int state = 1;

        public Map(T initial)
        {
            CreateTemplate(initial);
        }

        public List<Room<R>> Rooms { get; } = new List<Room<R>>();

        public Queue<Template<T>> Unfinished { get; } = new Queue<Template<T>>();

        public IEnumerable<Hall> Halls => Rooms.SelectMany(r => r.Exit);

        public int Key() => key++;

        public int Switch() => state++;

        public Room<R> CreateRoom(R type, int keySwitch = 0)
        {
            Room<R> r = new Room<R>(type, keySwitch);
            Rooms.Add(r);
            return r;
        }

        public Template<T> CreateTemplate(T type, int state = 0)
        {
            Template<T> t = new Template<T>(type, state);
            Unfinished.Enqueue(t);
            return t;
        }

        public Layout Replace(Layout old, Layout @new)
        {
            old.SwapLeft(@new);
            old.SwapRight(@new);
            return @new;
        }

        public void Reduce()
        {
            var halls = Halls.Where(r => r.Direction == Direction.Both && !r.Secret && !r.Key.HasValue && !r.State.HasValue).ToList(); // get all hallways

            foreach (var hall in halls)
            {
                Room<R> left = (Room<R>)hall.From;
                Room<R> right = (Room<R>)hall.To;
                if (left.Kind.Equals(right.Kind) && left.Exit.Count == 1 && right.Entrance.Count == 1)
                {
                    Console.WriteLine($"Merging room {right} into {left}");
                    //combine rooms:
                    // 1. remove this hallway
                    // 2. take the right room's left hallways
                    // 2a. have them point to the left room
                    // 2b. add them into the left room's set of entry hallways
                    // 3. repeat with right hallways

                    //   _______  ________
                    //   |     |  |      |
                    //  =   L  ====  R    =
                    //   |_____|  |______|
                    //
                    left.Exit.Remove(hall);
                    right.Entrance.Remove(hall);

                    foreach (var otherHall in right.Entrance) otherHall.To = left;
                    left.Entrance.AddRange(right.Entrance);

                    foreach (var otherHall in right.Exit) otherHall.From = left;
                    left.Exit.AddRange(right.Exit);

                    Rooms.Remove(right);
                }
            }
        }
    }
}