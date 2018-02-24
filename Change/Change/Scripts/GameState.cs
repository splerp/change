using Splerp.Change.Controllers;
using System;

namespace Splerp.Change
{
    public sealed class GameState : IEquatable<GameState>
    {
        public string Key { get; set; }
        public GameController.StartEnter OnEnterState { get; set; }
        public GameController.StageUpdate OnStateUpdate { get; set; }
        public GraphicsController.DrawState Draw { get; set; }

        public GameState(string key)
        {
            Key = key;
        }

        public bool Equals(GameState other)
        {
            return Key == other.Key;
        }

        public static GameState Title;
        public static GameState MapControls;
        public static GameState Playing;
        public static GameState BetweenStages;
        public static GameState GameOver;
    }
}
