namespace SimpleGraph
{
    public interface ITraversableNode<T>
    {
        public T Id { get; }
        public bool IsTraversable { get; }
    }
}