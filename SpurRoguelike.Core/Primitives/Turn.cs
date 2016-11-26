using System;
using SpurRoguelike.Core.Entities;

namespace SpurRoguelike.Core.Primitives
{
    public class Turn
    {
        private Turn(Action<Player> action)
        {
            this.action = action;
        }

        public static Turn Step(StepDirection direction)
        {
            return new Turn(player =>
            {
                player.Move(player.Location + Offset.FromDirection(direction), player.Level);
            });
        }

        public static Turn Step(Offset offset)
        {
            return new Turn(player =>
            {
                var stepOffset = offset.SnapToStep();

                if (stepOffset.XOffset != 0 || stepOffset.YOffset != 0)
                    player.Move(player.Location + stepOffset, player.Level);
            });
        }

        public static Turn Attack(AttackDirection direction)
        {
            return new Turn(player =>
            {
                var targetLocation = player.Location + Offset.FromDirection(direction);

                var target = player.Level.GetEntity<Monster>(targetLocation);

                if (target == null)
                    return;

                player.PerformAttack(target);
            });
        }
        public static Turn Attack(Offset offset)
        {
            return new Turn(player =>
            {
                var attackOffset = offset.Normalize();

                if (attackOffset.XOffset == 0 && attackOffset.YOffset == 0)
                    return;

                var targetLocation = player.Location + attackOffset;

                var target = player.Level.GetEntity<Monster>(targetLocation);

                if (target == null)
                    return;

                player.PerformAttack(target);
            });
        }

        public void Apply(Player player)
        {
            action(player);
        }

        public static readonly Turn None = new Turn(player => { });

        private readonly Action<Player> action;
    }
}
