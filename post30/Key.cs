namespace zeldagen.post30
{
    public class Key : Lock
    {
        private int _key;

        public Key(int key)
        {
            _key = key;
        }

        public override string ToString() => $"k{_key}";
    }
}