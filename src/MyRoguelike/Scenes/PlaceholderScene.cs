using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public class PlaceholderScene : Scene
{
    private readonly string _message;
    private KeyboardState _prevKeyboard;

    public PlaceholderScene(string message)
    {
        _message = message;
    }

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            Game1.Instance.SceneManager.Pop();
        _prevKeyboard = kb;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var device = spriteBatch.GraphicsDevice;
        if (device == null) return;

        device.Clear(Microsoft.Xna.Framework.Color.Black);

        spriteBatch.Begin();

        var font = Game1.Font;
        var msgSize = font.MeasureString(_message);
        var promptSize = font.MeasureString("Press Enter to return");
        var centerX = Constants.ScreenWidth / 2f;

        spriteBatch.DrawString(font, _message,
            new Vector2(centerX - msgSize.X / 2f, Constants.ScreenHeight / 2f - 40), Microsoft.Xna.Framework.Color.White);
        spriteBatch.DrawString(font, "Press Enter to return",
            new Vector2(centerX - promptSize.X / 2f, Constants.ScreenHeight / 2f + 10), Microsoft.Xna.Framework.Color.Gray);

        spriteBatch.End();
    }
}
