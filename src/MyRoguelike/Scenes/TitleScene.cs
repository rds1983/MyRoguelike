using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public class TitleScene : Scene
{
    private KeyboardState _prevKeyboard;

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
        {
            var overworld = new OverworldScene();
            Game1.Instance.SceneManager.Push(overworld);
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
        var title = Constants.GameTitle;
        var subtitle = "A Hack-n-Slash Roguelike";
        var prompt = "Press Enter to Start";

        var titleSize = font.MeasureString(title);
        var subSize = font.MeasureString(subtitle);
        var promptSize = font.MeasureString(prompt);
        var centerX = Constants.ScreenWidth / 2f;

        spriteBatch.DrawString(font, title,
            new Vector2(centerX - titleSize.X / 2f, Constants.ScreenHeight / 2f - 60), Microsoft.Xna.Framework.Color.Gold);
        spriteBatch.DrawString(font, subtitle,
            new Vector2(centerX - subSize.X / 2f, Constants.ScreenHeight / 2f - 20), Microsoft.Xna.Framework.Color.Gray);
        spriteBatch.DrawString(font, prompt,
            new Vector2(centerX - promptSize.X / 2f, Constants.ScreenHeight / 2f + 30), Microsoft.Xna.Framework.Color.White);

        spriteBatch.End();
    }
}
