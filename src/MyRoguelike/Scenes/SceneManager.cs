using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyRoguelike.Scenes;

public class SceneManager
{
    private readonly Stack<Scene> _scenes = new();

    public void Push(Scene scene)
    {
        scene.LoadContent();
        _scenes.Push(scene);
    }

    public Scene? Pop()
    {
        return _scenes.Count > 0 ? _scenes.Pop() : null;
    }

    public Scene? Peek()
    {
        return _scenes.Count > 0 ? _scenes.Peek() : null;
    }

    public int Count => _scenes.Count;

    public void Update(GameTime gameTime)
    {
        var scene = Peek();
        scene?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var scene = Peek();
        scene?.Draw(spriteBatch, gameTime);
    }

    public void Clear()
    {
        _scenes.Clear();
    }
}
