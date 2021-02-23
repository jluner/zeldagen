namespace zeldagen.post24
{
    public class Item
    {
        private string _kind;

        public Item(string kind)
        {
            _kind = kind;
        }

        public override string ToString() => _kind;

        public virtual ItemLock ToLock() => new ItemLock(this);

        public static Item NonDungeonItem() => new Item("!");
        public static Item DungeonItem() => new Item("?");
        public static Item BossKey() => new Item("i5");
        public static Item SmallKey() => new Item("i1");
        public static Item HeartPiece() => new Item("hp");
        public static Item Rupee() => new Item("r");
        public static Item Bombs() => new Item("b");
        public static Item Arrows() => new Item("a");
    }

    public class Switch : Item
    {
        public Switch(int id) : base($"s{id}")
        {
            Id = id;
        }
        public int Id { get; }
    }

    public class ToggleSwitch : Switch
    {
        private bool _toggled;

        public ToggleSwitch(int id, bool toggled)
        : base(id)
        {
            _toggled = toggled;
        }

        public bool Toggled => _toggled;
        public ToggleSwitch Toggle() => new ToggleSwitch(Id, !_toggled);
        public override ItemLock ToLock() => new ToggleSwitchLock(this);
    }
}