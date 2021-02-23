
namespace zeldagen
{

    public class Hall
    {
        public Layout From { get; set; }
        public Layout To { get; set; }

        public Direction Direction { get; set; }

        public Lock Lock { get; set; }
    }

    public enum Direction
    {
        Both,
        Forward,
        Back
    }
}