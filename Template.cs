namespace zeldagen
{
    public class Template<T> : Layout
    {
        private static int _counter;

        public Template(T type, int state) : base(_counter++)
        {
            Type = type;
            State = state;
        }

        public T Type { get; }
        public int State { get; set; }
    }
}