using MyRoguelike.Data.Models;

namespace MyRoguelike.World;

public class Tile
{
    public string TileDefId { get; set; } = "grass";
    public int X { get; set; }
    public int Y { get; set; }

    public TileDef? Def => Game1.Data.GetTile(TileDefId);

    public bool IsWalkable => Def?.IsWalkable ?? true;
    public bool IsTransparent => Def?.IsTransparent ?? true;
    public string Glyph => Def?.Glyph ?? ".";

    public Microsoft.Xna.Framework.Color? GetColor() => Def?.Color;
}
