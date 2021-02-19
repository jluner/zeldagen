namespace zeldagen.post30
{
    public class Post30Classifier : IRoomClassifier<RoomType>
    {
        public RoomCategory Classify(Room<RoomType> room)
        {
            return room.Kind switch
            {
                RoomType.Entrance => RoomCategory.Entrance,
                RoomType.Goal => RoomCategory.Goal,
                RoomType.BonusGoal => RoomCategory.Goal,
                _ => RoomCategory.Normal
            };
        }
    }
}