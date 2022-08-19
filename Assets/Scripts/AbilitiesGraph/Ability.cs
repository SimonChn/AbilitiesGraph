using SimpleGraph;
using UniRx;

public class Ability<T> : ITraversableNode<T> where T : notnull
{
    private const string defaultTitle = "?";
    private const string defaultDescription = "??";
    private const long defaultPrice = 0;

    public T Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public long Price { get; private set; }

    public bool IsTraversable => IsLearned.Value;

    public BoolReactiveProperty IsLearned { get; private set; }

    public Ability(T id, string title = defaultTitle, string description = defaultDescription, long price = defaultPrice, bool isLearned = false)
    {
        Id = id;
        Title = title;
        Description = description;
        Price = price;

        IsLearned = new BoolReactiveProperty(isLearned);
    }
}