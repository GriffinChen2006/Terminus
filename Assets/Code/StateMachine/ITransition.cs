public interface ITransition
{
    public IState To { get; set; }
    public IPredicate Condition { get; set; }
}