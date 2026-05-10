using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public class GameOverScene : Scene
{
    private KeyboardState _prevKeyboard;

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
        {
            Game1.Instance.SceneManager.Clear();
            Game1.Instance.SceneManager.Push(new TitleScene());
        }
        _prevKeyboard = kb;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var device = spriteBatch.GraphicsDevice;
        if (device == null) return;

        device.Clear(Microsoft.Xna.Framework.Color.Black);

        spriteBatch.Begin();

        var font = Game1.Font;
        var death = "You Died";
        var prompt = "Press Enter to return to the title screen";

        var deathSize = font.MeasureString(death);
        var promptSize = font.MeasureString(prompt);
        var centerX = Constants.ScreenWidth / 2f;

        spriteBatch.DrawString(font, death,
            new Vector2(centerX - deathSize.X / 2f, Constants.ScreenHeight / 2f - 40), Microsoft.Xna.Framework.Color.Red);
        spriteBatch.DrawString(font, prompt,
            new Vector2(centerX - promptSize.X / 2f, Constants.ScreenHeight / 2f + 10), Microsoft.Xna.Framework.Color.Gray);

        spriteBatch.End();
    }
}
