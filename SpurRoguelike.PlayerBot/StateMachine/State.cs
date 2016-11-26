using System;

namespace SpurRoguelike.PlayerBot
{
    internal abstract class State<T>
    {
        protected T Self;

        protected State(T self)
        {
            Self = self;
        }

        public abstract void GoToState<TState>(Func<TState> factory) where TState : State<T>;

        public abstract void Tick();
    }
}