namespace zeldagen.post24
{
    public class ItemLock : Lock
    {
        private Item _item;

        public ItemLock(Item item)
        {
            _item = item;
        }

        public override string ToString() => _item.ToString();
    }

    public class ToggleSwitchLock : ItemLock
    {
        private ToggleSwitch _switch;
        public ToggleSwitchLock(ToggleSwitch item) : base(item)
        {
            _switch = item;
        }

        public override string ToString() => (_switch.Toggled ? "!" : "") + _switch.ToString();
    }
}