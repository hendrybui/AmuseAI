using Amuse.UI.Enums;

namespace Amuse.UI.Models
{
    public record ResolutionOption(int Width, int Height, ResolutionType Type, string Aspect)
    {
        public string HQDisplayName => $"{Width * 2} x {Height * 2}";
        public override string ToString()
        {
            return $"{Width} x {Height}";
        }
    }
}
