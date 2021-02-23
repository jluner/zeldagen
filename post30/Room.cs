namespace zeldagen.post30
{
    public class Room : RoomBase
    {

        public Room(RoomType kind, int keySwitch)
        {
            Kind = kind;
            KeySwitch = keySwitch;
        }

        public RoomType Kind { get; }

        public int KeySwitch { get; }

        public override string ToString() => KeySwitch == 0 ? $"{Kind} ({Id})" : $"{Kind} [{KeySwitch}] ({Id})";
    }
}