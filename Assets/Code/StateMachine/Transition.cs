﻿public class Transition : ITransition
{
    public IState To { get; set; }
    public IPredicate Condition { get; set; }

    public Transition(IState to, IPredicate condition)
    {
        To = to;
        Condition = condition;
    }
}