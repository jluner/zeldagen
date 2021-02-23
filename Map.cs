using System;
using System.Collections.Generic;
using System.Linq;

namespace zeldagen
{
    public class Map<T, R>
        where T : TemplateBase
        where R : RoomBase
    {
        private Func<R, R, Reduction> _reducible;

        public List<R> Rooms { get; } = new List<R>();

        public Queue<T> _unfinished = new Queue<T>();

        public Map(Func<R, R, Reduction> reducible)
        {
            _reducible = reducible;
        }

        public IEnumerable<Hall> Halls => Rooms.SelectMany(r => r.Exit);

        public IEnumerable<T> RemainingTemplates()
        {
            // This is a particularly nasty twist as we can now iterate over a mutating collection
            while (_unfinished.TryDequeue(out var t)) yield return t;
        }

        public T Track(T template)
        {
            _unfinished.Enqueue(template);
            return template;
        }

        public R Track(R room)
        {
            Rooms.Add(room);
            return room;
        }

        public void Reduce()
        {
            var halls = Halls.Where(r => r.Direction == Direction.Both && r.Lock is null).ToList(); // get all hallways

            foreach (var hall in halls)
            {
                R left = (R)hall.From;
                R right = (R)hall.To;

                switch (_reducible(left, right))
                {
                    case Reduction.MergeRightToLeft:
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
                        break;
                    case Reduction.MergeLeftToRight:
                        Console.WriteLine($"Merging room {left} into {right}");
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

                        foreach (var otherHall in left.Entrance) otherHall.To = right;
                        right.Entrance.AddRange(left.Entrance);

                        foreach (var otherHall in left.Exit) otherHall.From = right;
                        right.Exit.AddRange(left.Exit);

                        Rooms.Remove(left);
                        break;
                }
            }
        }
    }

    public enum Reduction
    {
        Keep,
        MergeRightToLeft,
        MergeLeftToRight
    }
}