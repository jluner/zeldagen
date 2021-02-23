namespace zeldagen.post30
{
    public class Template : TemplateBase
    {
        public Template(TemplateType type, int state)
        {
            Type = type;
            State = state;
        }

        public TemplateType Type { get; }
        public int State { get; set; }
    }
}