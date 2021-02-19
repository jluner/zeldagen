namespace zeldagen
{
    public interface IMapGrammar<T, R>
    {
        Map<T, R> GenerateMap();

        IRoomClassifier<R> Classifier { get; }
    }
}