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

        public override string ToString() => Item is null ? $"{Kind} ({Id})" : $"[{Item}] ({Id})";
    }
}