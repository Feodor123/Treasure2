namespace Treasure
{
    public class PlayerParameters
    {
        public PlayerParameters(IPlayerController controller)
        {
            Controller = controller;
        }

        public IPlayerController Controller { get; set; }
    }
}