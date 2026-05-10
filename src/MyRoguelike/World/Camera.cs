using MonoGamePoint = Microsoft.Xna.Framework.Point;
using MonoGameRectangle = Microsoft.Xna.Framework.Rectangle;

namespace MyRoguelike.World;

public class Camera
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int ViewportWidth { get; }
    public int ViewportHeight { get; }

    private int _mapWidth;
    private int _mapHeight;

    public Camera(int viewportWidth, int viewportHeight)
    {
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    public void SetMapBounds(int mapWidth, int mapHeight)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
    }

    public void CenterOn(int worldX, int worldY)
    {
        var tileSize = Constants.TileSize;
        X = worldX - ViewportWidth / (2 * tileSize);
        Y = worldY - ViewportHeight / (2 * tileSize);
        Clamp();
    }

    public void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
        Clamp();
    }

    private void Clamp()
    {
        X = Math.Clamp(X, 0, Math.Max(0, _mapWidth - TilesOnScreenX));
        Y = Math.Clamp(Y, 0, Math.Max(0, _mapHeight - TilesOnScreenY));
    }

    public int TilesOnScreenX => ViewportWidth / Constants.TileSize + 2;
    public int TilesOnScreenY => ViewportHeight / Constants.TileSize + 2;

    public MonoGamePoint WorldToScreen(int worldX, int worldY)
    {
        return new MonoGamePoint(
            (worldX - X) * Constants.TileSize,
            (worldY - Y) * Constants.TileSize
        );
    }

    public MonoGamePoint ScreenToWorld(int screenX, int screenY)
    {
        return new MonoGamePoint(
            screenX / Constants.TileSize + X,
            screenY / Constants.TileSize + Y
        );
    }

    public MonoGameRectangle GetTileScreenRect(int tileX, int tileY)
    {
        var screen = WorldToScreen(tileX, tileY);
        return new MonoGameRectangle(screen.X, screen.Y, Constants.TileSize, Constants.TileSize);
    }

    public bool IsTileVisible(int tileX, int tileY)
    {
        return tileX >= X - 1 && tileX < X + TilesOnScreenX &&
               tileY >= Y - 1 && tileY < Y + TilesOnScreenY;
    }
}
