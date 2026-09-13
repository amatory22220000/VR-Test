namespace MeowStudio.States
{
    public class Transition : ITransition
    {
        public Transition(IState to, IPredicate condition)
        {
            To = to;
            Condition = condition;
        }

        public IState To { get; }

        public IPredicate Condition { get; }
    }
}
