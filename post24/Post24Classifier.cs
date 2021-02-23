namespace zeldagen.post24
{
    public class Post24Classifier : IRoomClassifier<Room>
    {
        public RoomCategory Classify(Room room)
        {
            return room.Kind switch{
                RoomType.Start => RoomCategory.Entrance,
                RoomType.Goal => RoomCategory.Goal,
                RoomType.Enemy => RoomCategory.Battle,
                RoomType.MiniBoss => RoomCategory.BossBattle,
                RoomType.EndBoss => RoomCategory.BossBattle,
                _ => RoomCategory.Normal
            };
        }
    }
}