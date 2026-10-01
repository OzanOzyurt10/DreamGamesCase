namespace DreamCase.Core.Items
{
    public sealed class Cube : Item
    {
        public CubeColor Color { get; }

        public Cube(CubeColor color)
        {
            Color = color;
        }
    }
}
