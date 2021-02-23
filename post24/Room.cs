namespace zeldagen.post24
{
    public class Room : RoomBase
    {
        public Room(RoomType kind, Item item)
        {
            Kind = kind;
            Item = item;
        }

        public RoomType Kind { get; }

        public Item Item { get; }

        public override string ToString() => Kind switch
        {
            RoomType.Start => "s",
            RoomType.EndBoss => "eb",
            RoomType.Goal => "g",
            RoomType.MiniBoss => "em",
            RoomType.Enemy => "e",
            RoomType.Empty => "n",
            RoomType.Puzzle => "p",
            _ => Item.ToString()
        };
    }
}