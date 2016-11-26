using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    internal class StateEscape : State<PlayerBot>
    {
        public StateEscape(PlayerBot self) : base(self)
        {
        }

        public override void GoToState<TState>(Func<TState> factory)
        {
            Self.stateMachine = factory();
            Self.stateMachine.Tick();
        }

        public override void Tick()
        {
            var objectivePath = default(Path);
            var item = default(ItemView);
            var health = default(HealthPackView);

            bool healthRequired = Self.health < PlayerBot.HealthMaximum;

            bool itemRequied = Self.health == PlayerBot.HealthMaximum;

            if (!itemRequied && !healthRequired)
            {
                EscapeLevel();
                return;
            }

            if (itemRequied)
                item = BestLevelItem();

            if (healthRequired)
                objectivePath = PathToHealth(ref health);

            if (itemRequied && !item.HasValue || healthRequired && !health.HasValue)
            {
                EscapeLevel();
                return;
            }
            if (itemRequied)
                if (IsItemBetterThanEquiped(item))
                {
                    Self.objective = item;
                    objectivePath = Self.pathFinder.FindPath(item.Location);
                }
                else
                {
                    EscapeLevel();
                    return;
                }
            else
                Self.objective = health;

            GoToState(() => new StateGrabItem(Self, objectivePath));
        }

        private ItemView BestLevelItem()
        {
            return Self.level.Items
                .DefaultIfEmpty()
                .OrderByDescending(x => PlayerBot.ItemValue(x))
                .FirstOrDefault();
        }

        private void EscapeLevel()
        {
            var exitLocation = Self.level.Field.GetCellsOfType(CellType.Exit).First();

            var exitRoute = Self.pathFinder.FindPath(exitLocation);

            var attackRangeMonsters = Self.level.Monsters.Where(Self.IsInAttackRange);

            if (exitRoute.Count == 0)
            {
                FightForExit(attackRangeMonsters);
                return;
            }

            var direction = exitRoute.First() - Self.playerLocation;

            var exitStep = direction.SnapToStep();

            Self.selectedTurn = Turn.Step(exitStep);
        }

        private void FightForExit(IEnumerable<PawnView> localMonsters)
        {
            if (localMonsters.Any())
            {
                var offset = localMonsters.First().Location - Self.playerLocation;
                Self.selectedTurn = Turn.Attack(offset);
            }
            else if (Self.level.Monsters.Any())
            {
                Offset step = StepToMonsters();
                Self.selectedTurn = Turn.Step(step);
            }
        }

        private bool IsItemBetterThanEquiped(ItemView item)
        {
            return !Self.equippedItem.HasValue || PlayerBot.ItemValue(item) > PlayerBot.ItemValue(Self.equippedItem);
        }

        private Path PathToHealth(ref HealthPackView safeHP)
        {
            var rankedRoutes = RankHealthPacks();

            if (rankedRoutes.Count > 0)
            {
                var rankedPath = rankedRoutes
                    .OrderBy(r => r.Item3)
                    .ThenBy(r => r.Item1.Count)
                    .First();

                safeHP = rankedPath.Item2;
                return rankedPath.Item1;
            }
            else
                return default(Path);
        }

        private List<Tuple<Path, HealthPackView, int>> RankHealthPacks()
        {
            var rankedRoutes = new List<Tuple<Path, HealthPackView, int>>();
            var healthPacks = Self.level.HealthPacks.OrderBy(x => Self.playerLocation.Distance(x.Location));

            foreach (HealthPackView hp in healthPacks)
            {
                var route = Self.pathFinder.FindPath(hp.Location);

                if (route.Any())
                {
                    var safetyRank = route.SafetyRank(Self.level.Monsters);
                    rankedRoutes.Add(Tuple.Create(route, hp, safetyRank));
                    if (safetyRank == 0)
                        break;
                }
            }

            return rankedRoutes;
        }

        private Offset StepToMonsters()
        {
            var stepAttack = Self.level.Monsters
                .OrderBy(m => Self.playerLocation.Distance(m.Location))
                .ThenBy(m => m.Health)
                .First().Location - Self.playerLocation;
            return stepAttack.SnapToStep();
        }
    }
}