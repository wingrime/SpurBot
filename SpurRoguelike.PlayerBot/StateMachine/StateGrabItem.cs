using SpurRoguelike.Core.Primitives;
using System;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class StateGrabItem : State<PlayerBot>
    {
        private readonly Path objectiveRoute;

        public StateGrabItem(PlayerBot self, Path objectiveRoute)
            : base(self)
        {
            this.objectiveRoute = objectiveRoute;
        }

        public override void GoToState<TState>(Func<TState> factory)
        {
            Self.stateMachine = factory();
            Self.stateMachine.Tick();
        }

        public override void Tick()
        {
            if (objectiveRoute.Count == 0)
            {
                if (Self.level.Monsters.Any(Self.IsInAttackRange))
                {
                    var attackOffset = Self.level.Monsters
                        .Where(Self.IsInAttackRange)
                        .OrderBy(m => m.Health)
                        .ThenBy(m => m.Attack)
                        .ThenBy(m => m.Defence)
                        .First()
                        .Location - Self.playerLocation;
                    Self.selectedTurn = Turn.Attack(attackOffset);
                }
            }
            else
            {
                var objectiveStep = (objectiveRoute.First() - Self.playerLocation).SnapToStep();
                Self.selectedTurn = Turn.Step(objectiveStep);
            }
        }
    }
}