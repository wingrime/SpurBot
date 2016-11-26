using System;
using System.Collections.Generic;
using System.Linq;
using SpurRoguelike.Core;
using SpurRoguelike.Core.Entities;
using SpurRoguelike.Core.Primitives;
using System.Threading;

namespace SpurRoguelike
{

    internal class TrainerRenderer : IRenderer
    {
        private long frameCounter = 0;
        private const int frameTimeout = 4000;

        public void RenderGameEnd(bool isCompleted)
        {
        }

        public void RenderLevel(Level level)
        {
            frameCounter++;
            if (frameCounter > frameTimeout)
                throw new TimeoutException("Run timeout");

        }
    }
    internal class TrainerEventReporter : IEventReporter
    {
        public void ReportAttack(Pawn attacker, Pawn victim) { }

        public void ReportDamage(Pawn pawn, int damage, Entity instigator) { }

        public void ReportDestroyed(Pawn pawn) { }

        public void ReportGameEnd() { }

        public void ReportLevelEnd() { }

        public void ReportMessage(string message) { }

        public void ReportMessage(Entity instigator, string message) { }

        public void ReportNewEntity(Location location, Entity entity) { }

        public void ReportPickup(Pawn pawn, Pickup item) { }

        public void ReportTrap(Pawn pawn) { }

        public void ReportUpgrade(Pawn pawn, int attackBonus, int defenceBonus) { }
    }
    internal class RunData
    {
        public int Seed;
        public bool IsWin = false;
        public bool IsTimeOut = false;
        public bool IsException = false;
        public GameResult Results;
        public AutoResetEvent CompliteEvent;
    }
    internal class Trainer
    {
        public static GameResult RunTestRun(EntryPoint.GameOptions options, int Seed)
        {
            var levels = EntryPoint.GenerateLevels(Seed, options.LevelCount);

            var playerController = BotLoader.LoadPlayerController(options.PlayerController);

            var engine = new Engine(options.PlayerName, playerController, levels.First(), new TrainerRenderer(), new TrainerEventReporter());

            return engine.GameLoop();

        }

        public static void RunTrainer(EntryPoint.GameOptions options)
        {
            GameResult zeroSeedRun = null;
            try
            {
                zeroSeedRun = RunTestRun(options, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game Seed 0 - Exception {ex}");
            }


            Console.WriteLine($"Game Seed 0 - Result {zeroSeedRun?.IsWin} LevelReach {zeroSeedRun?.DieLevel} Kills {zeroSeedRun?.KillCount}");
            Console.Out.Flush();
            if (zeroSeedRun == null || !zeroSeedRun.IsWin)
            {
                return;
            }

            const int runs = 128;

            const int runGraduality = 24;


            

            var runData = new List<RunData>(runs);
            var rnd = new Random();
            for (int i = 0; i < runGraduality; i++)
            {
                List<AutoResetEvent> signals = new List<AutoResetEvent>();

                for (int x = 0; x < runs / runGraduality; x++)
                {
                    
                    var run = new RunData();

                    run.CompliteEvent = new AutoResetEvent(false);
                    signals.Add(run.CompliteEvent);
                    run.Seed = rnd.Next();
                    runData.Add(run);

                    ThreadPool.QueueUserWorkItem((o) =>
                        {

                            RunData data = (RunData)o;

                            var seed = data.Seed;

                            try
                            {
                                data.Results = RunTestRun(options, seed);
                                data.IsWin = data.Results.IsWin;
                            }
                            catch (TimeoutException)
                            {
                                data.IsWin = false;
                                data.IsTimeOut = true;
                                Console.WriteLine($"Game Seed {seed} - Timeout ");

                            }
                            catch (Exception ex)
                            {
                                data.IsWin = false;
                                data.IsException = true;
                                Console.WriteLine($"Game Seed {seed} - Exception {ex.Message}");

                            }

                            Console.WriteLine($"Game Seed {seed} - Result {data.IsWin } LevelReach {data.Results?.DieLevel} Kills {data.Results?.KillCount}");
                            Console.Out.Flush();
                            data.CompliteEvent.Set();
                        }, (object)run);

                }
                WaitHandle.WaitAll(signals.ToArray());
            }

            var avg = runData.Select(m => m.IsWin ? 1.0 : 0.0).Average();
            var suc = runData.Select(m => m.IsWin ? 1.0 : 0.0).Sum();
            var timeOuts = runData.Select(m => m.IsTimeOut ? 1.0 : 0.0).Sum();
            var excepts = runData.Select(m => m.IsException ? 1.0 : 0.0).Sum();
            var killAvg = runData.Select(m => m.Results?.KillCount ?? 0).Average();
            var dieLevelAvg = runData.Select(m => m.Results?.DieLevel ?? 0).Average();



            Console.WriteLine("------ Results -----\n");
            Console.WriteLine($"------ Runs = {runs} \n");
            Console.WriteLine($"------ Avg Complite = {avg}\n");
            Console.WriteLine($"------ Avg Kills = {killAvg}\n");
            Console.WriteLine($"------ Avg DieLevel = {dieLevelAvg} \n");

            Console.WriteLine($"------ Wins = {suc} \n");
            Console.WriteLine($"------ Timeouts = {timeOuts} \n");
            Console.WriteLine($"------ Excepts = {excepts} \n");
            Console.ReadKey();
        }
    }
}
