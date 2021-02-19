namespace zeldagen
{
    public enum RoomType
    {
        Entrance,
        Goal,
        BonusGoal,
        Empty,
        Monster,
        Challenge,
        Trap,
        Puzzle,
        Key,
        Switch
    }

    public class Room : Layout
    {
        private static int _counter;
        public Room(RoomType kind, int keySwitch)
        {
            Kind = kind;
            KeySwitch = keySwitch;
            Id = _counter++;
        }
        public RoomType Kind { get; }

        public int KeySwitch { get; }

        public int Id { get; }

        public override string ToString() => KeySwitch == 0 ? $"{Kind} ({Id})" : $"{Kind} [{KeySwitch}] ({Id})";
    }
}