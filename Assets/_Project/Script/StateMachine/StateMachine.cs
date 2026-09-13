using System;
using System.Collections.Generic;

namespace MeowStudio.States
{
    public class StateMachine
    {
        public StateNode current;
        Dictionary<Type, StateNode> nodes = new();
        HashSet<ITransition> anyTransitions = new();

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.To);
            current.State?.Update();
        }
        public void FixedUpdate()
        {
            current.State?.FixedUpdate();
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }
        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }
        public void AddStateWithoutTransitions(IState state)
        {
            GetOrAddNode(state);
        }

        //Start state
        public void SetStartState(IState state)
        {
            current = nodes[state.GetType()];
            current.State?.OnEnter();
        }
        //Force set state
        public void ChangeState(IState state)
        {
            if (state == current.State) return;

            var previousState = current.State;
            var nextState = nodes[state.GetType()].State;

            previousState?.OnExit();
            nextState?.OnEnter();

            current = nodes[nextState.GetType()];
        }

        private ITransition GetTransition()
        {
            foreach (var transition in anyTransitions)
            {
                if (transition.Condition.Evaluate())
                    return transition;
            }
            foreach (var transition in current.Transitions)
            {
                if (transition.Condition.Evaluate())
                    return transition;
            }
            return null;
        }
        private StateNode GetOrAddNode(IState state)
        {
            var node = nodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }
            return node;
        }
    }
}
