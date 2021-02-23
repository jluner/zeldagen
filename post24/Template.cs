namespace zeldagen.post24
{
    public class Template : TemplateBase
    {
        public Template(TemplateType type, Item item)
        {
            Type = type;
            Item = item;
        }

        public TemplateType Type { get; }
        public Item Item { get; set; }

        public override string ToString()
        {
            return Item is null ? $"{Type} ({Id})" : $"{Type} [{Item}] ({Id})";
        }
    }
}