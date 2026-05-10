namespace MyRoguelike.Data.Models;

public class TileDef
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsWalkable { get; set; }
    public bool IsTransparent { get; set; }
    public string Glyph { get; set; } = ".";
    public Microsoft.Xna.Framework.Color? Color { get; set; }
}
