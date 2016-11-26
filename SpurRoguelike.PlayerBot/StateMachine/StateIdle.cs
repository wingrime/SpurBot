using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class StateIdle : State<PlayerBot>
    {
        public StateIdle(PlayerBot self)
            : base(self)
        {
        }

        public override void GoToState<TState>(Func<TState> factory)
        {
            Self.stateMachine = factory();
            Self.stateMachine.Tick();
        }

        public override void Tick()
        {
            var health = default(HealthPackView);
            if (GetHealthPack(ref health) && Self.health < PlayerBot.HealthMaximum)
            {
                var healthStep = health.Location - Self.playerLocation;
                Self.selectedTurn = Turn.Step(healthStep);

                return;
            }
            if (Self.targetMonster.HasValue)
            {
                GoToState(() => new StateFight(Self));
                return;
            }

            if (SelectTargetMonster(ref Self.targetMonster))
            {
                GoToState(() => new StateFight(Self));
                return;
            }

            GoToState(() => new StateEscape(Self));
        }

        private bool GetHealthPack(ref HealthPackView health)
        {
            var target = Offset.StepOffsets
                .Select(o => Self.level.GetHealthPackAt(Self.playerLocation + o))
                .Where(o => o.HasValue);
            if (target.Any())
            {
                health = target.First();
                return true;
            }
            else
                return false;
        }

        private bool SelectTargetMonster(ref PawnView target)
        {
            if (target.HasValue)
                return true;

            if (Self.level.Monsters.Any(Self.IsInAttackRange))
            {
                target = Self.level.Monsters
                    .Where(Self.IsInAttackRange)
                    .OrderBy(m => m.Health)
                    .ThenBy(m => m.Attack)
                    .ThenBy(m => m.Defence)
                    .First();
                return true;
            }
            else
            {
                target = Self.level.Monsters
                    .OrderBy(m => Self.playerLocation.Distance(m.Location))
                    .ThenBy(m => m.Health)
                    .ThenBy(m => m.Attack)
                    .ThenBy(m => m.Defence)
                    .FirstOrDefault();
                return target.HasValue;
            }
        }
    }
}