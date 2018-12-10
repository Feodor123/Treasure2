namespace Treasure
{
    public class PlayerParameters
    {
        public PlayerParameters(string name, int color, IPlayerController controller)
        {
            Name = name;
            Color = color;
            Controller = controller;
        }

        public string Name { get; set; }
        public int Color { get; set; }
        public IPlayerController Controller { get; set; }
    }
}