namespace zeldagen
{
    public abstract class TemplateBase : Layout
    {
        private static int _counter;

        protected TemplateBase() : base(_counter++)
        {
        }
    }
}