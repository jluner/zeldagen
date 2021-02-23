namespace zeldagen.post30
{
    public class Switch : Lock
    {
        private int _switch;

        public Switch(int @switch)
        {
            _switch = @switch;
        }

        public override string ToString() => $"s{_switch}";
    }
}