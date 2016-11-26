using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class StateFight : State<PlayerBot>
    {
        private const double AvgDamage = 9.5;
        private const int MaxDamage = 10;
        private const int MinDamage = 9;
        private const double SafetyDecide = 9;
        private const double SafetyLastLevelDecide = 4;

        public StateFight(PlayerBot self)
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
            var attackRoute = Self.pathFinder.FindPath(Self.targetMonster.Location);
            if (attackRoute.Count == 0)
            {
                GoToState(() => new StateEscape(Self));
                return;
            }

            var monsters = MonstersInRange(attackRoute.Last());
            if (monsters.Count() > 1)
            {
                GoToState(() => new StateEscape(Self));
                return;
            }

            if (FightDecideFunction(Self.targetMonster))
            {
                if (Self.IsInAttackRange(Self.targetMonster))
                {
                    var attackOffset = Self.targetMonster.Location - Self.playerLocation;
                    Self.selectedTurn = Turn.Attack(attackOffset);
                    return;
                }

                if (Offset.StepOffsets.Any(o => Self.level.GetItemAt(Self.playerLocation + o).HasValue))
                {
                    var item = Self.level.Items
                                .Where(i => Offset.StepOffsets
                                .Any(o => i.Location == Self.playerLocation + o))
                                .OrderByDescending(i => PlayerBot.ItemValue(i))
                                .First();
                    if (PlayerBot.ItemValue(item) > PlayerBot.ItemValue(Self.equippedItem))
                    {
                        var itemStep = item.Location - Self.playerLocation;

                        Self.selectedTurn = Turn.Step(itemStep);
                        return;
                    }
                }
                var attackStep = (attackRoute.First() - Self.playerLocation).SnapToStep();
                Self.selectedTurn = Turn.Step(attackStep);
                return;
            }

            GoToState(() => new StateEscape(Self));
        }

        internal bool FightDecideFunction(PawnView monster)
        {
            var damageToMonster = (((double)Self.totalAttack / monster.TotalDefence) * MinDamage);

            if (damageToMonster >= monster.Health)
            {
                return true;
            }
            var damageToSelf = (((double)monster.TotalAttack / Self.totalDefence) * MaxDamage);

            if (damageToSelf > Self.health)
            {
                return false;
            }

            var safety = SafetyDecide;
            if (Self.level.Monsters.Count() == 1)
                safety = SafetyLastLevelDecide;

            var avgDamageToSelf = (((double)monster.TotalAttack / Self.totalDefence) * AvgDamage);

            if (avgDamageToSelf * safety < Self.health)
                return true;
            else
                return false;
        }

        private IEnumerable<PawnView> MonstersInRange(Location around)
        {
            return Self.level.Monsters.Where(m => Offset.AttackOffsets.Any(o => m.Location + o == around));
        }
    }
}