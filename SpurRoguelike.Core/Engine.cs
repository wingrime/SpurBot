using SpurRoguelike.Core.Entities;

namespace SpurRoguelike.Core
{

    public class GameResult
    {
        public int DieLevel = 0;
        public int KillCount = 0;
        public bool IsWin = false;
    }
    public class Engine
    {
        public Engine(string playerName, IPlayerController playerController, Level entryLevel, IRenderer renderer, IEventReporter eventReporter)
        {
            this.playerName = playerName;
            this.playerController = playerController;
            this.entryLevel = entryLevel;
            this.renderer = renderer;
            this.eventReporter = eventReporter;
        }

        public GameResult GameLoop()
        {
            var player = new Player(playerName, 10, 10, 100, 100, playerController, eventReporter);
            entryLevel.Spawn(entryLevel.Field.PlayerStart, player);

            while (!player.IsDestroyed)
            {
                renderer.RenderLevel(player.Level);
                player.Level.Tick();
            }
            renderer.RenderGameEnd(player.Level.IsCompleted);
            var result = new GameResult();
            result.IsWin = player.Level.IsCompleted;
            result.DieLevel = player.Level.Number;
            result.KillCount = player.Kills;
            return result;
        }

        private readonly string playerName;
        private readonly IPlayerController playerController;
        private readonly Level entryLevel;
        private readonly IRenderer renderer;
        private readonly IEventReporter eventReporter;
    }
}