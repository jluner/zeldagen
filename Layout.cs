using System.Collections.Generic;

namespace zeldagen
{
    public class Layout
    {
        public int Id { get; }
        public HashSet<Hall> Entrance { get; } = new HashSet<Hall>();
        public HashSet<Hall> Exit { get; } = new HashSet<Hall>();

        public Layout(int id)
        {
            Id = id;
        }

        public Hall ConnectTo(Layout right, Direction dir = Direction.Both)
        {
            var hall = new Hall { From = this, To = right, Direction = dir };
            this.Exit.Add(hall);
            right.Entrance.Add(hall);
            return hall;
        }

        public void SwapLeft(Layout @new)
        {
            foreach (var hall in this.Entrance) hall.To = @new;
            @new.Entrance.AddRange(this.Entrance);
            this.Entrance.Clear();
        }

        public void SwapRight(Layout @new)
        {
            foreach (var hall in this.Exit) hall.From = @new;
            @new.Exit.AddRange(this.Exit);
            this.Exit.Clear();
        }

        public Layout ReplaceWith(Layout @new)
        {
            SwapLeft(@new);
            SwapRight(@new);
            return @new;
        }
    }
}