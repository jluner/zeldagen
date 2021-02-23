namespace zeldagen.post30
{
    public class Post30Classifier : IRoomClassifier<Room>
    {
        public RoomCategory Classify(Room room)
        {
            return room.Kind switch
            {
                RoomType.Entrance => RoomCategory.Entrance,
                RoomType.Goal => RoomCategory.Goal,
                RoomType.BonusGoal => RoomCategory.Goal,
                RoomType.Monster => RoomCategory.Battle,
                _ => RoomCategory.Normal
            };
        }
    }
}