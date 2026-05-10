using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyRoguelike.Scenes;

public abstract class Scene
{
    public virtual void LoadContent() { }
    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime) { }
}
