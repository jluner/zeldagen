using System;
using System.Collections.Generic;
using System.Linq;

namespace zeldagen
{
    public class Map
    {
        private int key = 1;
        private int state = 1;

        public Map()
        {
            CreateTemplate(TemplateType.DungeonStart);
        }

        public List<Room> Rooms { get; } = new List<Room>();

        public Queue<Template> Unfinished { get; } = new Queue<Template>();

        public IEnumerable<Hall> Halls => Rooms.SelectMany(r => r.Exit);

        public int Key() => key++;

        public int Switch() => state++;

        public Room CreateRoom(RoomType type, int keySwitch = 0)
        {
            Room r = new Room(type, keySwitch);
            Rooms.Add(r);
            return r;
        }

        public Template CreateTemplate(TemplateType type, int state = 0)
        {
            Template t = new Template(type, state);
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
                Room left = (Room)hall.From;
                Room right = (Room)hall.To;
                if (left.Kind == right.Kind && left.Exit.Count == 1 && right.Entrance.Count == 1)
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