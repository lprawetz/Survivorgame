using Godot;

namespace SurvivorGame.Core
{
    public partial class Game : Node
    {
        public static Game Instance { get; private set; }
        
        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        public void QuitGame()
        {
            GetTree().Quit();
        }
    }
}