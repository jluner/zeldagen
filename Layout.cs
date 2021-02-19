using System.Collections.Generic;

namespace zeldagen
{
    public class Layout
    {
        public HashSet<Hall> Entrance { get; } = new HashSet<Hall>();
        public HashSet<Hall> Exit { get; } = new HashSet<Hall>();

        public Hall Connect(Layout right, Direction dir = Direction.Both)
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
    }

    public enum TemplateType
    {
        DungeonStart,
        EntranceChain,
        LayoutChooser,
        RoomChooser,
        HookSequence,
        LockChain,
        MultiSwitch,
        LinearSequence,
        BonusGoal,
        SwitchSeq,
        SwitchLockSeq
    }

    public class Template : Layout
    {
        public Template(TemplateType type, int state)
        {
            Type = type;
            State = state;
        }

        public TemplateType Type { get; }
        public int State { get; set; }
    }
}