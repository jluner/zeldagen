
namespace zeldagen
{

    public class Hall
    {
        public Layout From { get; set; }
        public Layout To { get; set; }

        public Direction Direction { get; set; }

        public bool Secret { get; set; }

        public int? Key { get; set; }

        public int? State { get; set; }
    }

    public enum Direction
    {
        Both,
        Forward,
        Back
    }
}