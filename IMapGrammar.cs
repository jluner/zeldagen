namespace zeldagen
{
    public interface IMapGrammar<T, R> where T: TemplateBase where R: RoomBase
    {
        Map<T, R> GenerateMap();

        IRoomClassifier<R> Classifier { get; }
    }
}