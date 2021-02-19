namespace zeldagen
{

    public class Room<T> : Layout
    {
        private static int _counter;
        
        public Room(T kind, int keySwitch) : base(_counter++)
        {
            Kind = kind;
            KeySwitch = keySwitch;
        }

        public T Kind { get; }

        public int KeySwitch { get; }

        public override string ToString() => KeySwitch == 0 ? $"{Kind} ({Id})" : $"{Kind} [{KeySwitch}] ({Id})";
    }
}