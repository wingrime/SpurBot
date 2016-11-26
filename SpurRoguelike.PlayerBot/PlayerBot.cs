using SpurRoguelike.Core;
using SpurRoguelike.Core.Primitives;
using SpurRoguelike.Core.Views;
using System.Linq;

namespace SpurRoguelike.PlayerBot
{
    public class PlayerBot : IPlayerController
    {
        internal const int HealthMaximum = 100;
        internal const int ResetDistance = 4;
        internal ItemView equippedItem;
        internal int health;
        internal LevelView level;
        internal IView objective;
        internal PathFinder pathFinder;
        internal Location playerLocation;
        internal Turn selectedTurn;
        internal State<PlayerBot> stateMachine;
        internal PawnView targetMonster;
        internal int totalAttack;
        internal int totalDefence;

        public Turn MakeTurn(LevelView levelView, IMessageReporter messageReporter)
        {
            level = levelView;
            health = levelView.Player.Health;
            playerLocation = levelView.Player.Location;
            totalAttack = levelView.Player.TotalAttack;
            totalDefence = levelView.Player.TotalDefence;
            selectedTurn = Turn.None;
            levelView.Player.TryGetEquippedItem(out equippedItem);
            objective = default(IView);
            pathFinder = new PathFinder(level, playerLocation);
            ResetTarget(ref targetMonster);

            Tick();
            return selectedTurn;
        }

        public void Tick()
        {
            stateMachine = new StateIdle(this);
            stateMachine.Tick();
        }

        internal static double ItemValue(ItemView item)
        {
            return item.AttackBonus * 1.2 + item.DefenceBonus;
        }

        internal bool IsInAttackRange(PawnView other)
        {
            return playerLocation.IsInAttackRange(other.Location);
        }

        private void ResetTarget(ref PawnView target)
        {
            if (target.IsDestroyed
                || level.Monsters.Any(m => playerLocation.Distance(m.Location) <= ResetDistance 
                && m.Location != targetMonster.Location))
                target = default(PawnView);
        }
    }
}