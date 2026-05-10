using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameKeyboard = Microsoft.Xna.Framework.Input.Keyboard;
using MonoGameKeys = Microsoft.Xna.Framework.Input.Keys;
using MonoGameButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Microsoft.Xna.Framework.Input;
using MyRoguelike.Data;
using MyRoguelike.Scenes;

namespace MyRoguelike;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private readonly SceneManager _sceneManager = new();

    public static Game1 Instance { get; private set; } = null!;
    public SceneManager SceneManager => _sceneManager;

    public static DataManager Data { get; internal set; } = null!;
    public static Texture2D PlaceholderTile { get; private set; } = null!;
    public static SpriteFont Font { get; private set; } = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Instance = this;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Constants.ScreenWidth;
        _graphics.PreferredBackBufferHeight = Constants.ScreenHeight;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        Font = Content.Load<SpriteFont>("Fonts/Console");
        CreatePlaceholderTexture();

        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
        var dataManager = new DataManager(dataDir);
        Data = dataManager;

        if (dataManager.LoadAll())
            Debug.WriteLine("[Game1] All data loaded successfully.");
        else
            Debug.WriteLine("[Game1] WARNING: Some data files failed to load.");

        _sceneManager.Push(new TitleScene());
    }

    private void CreatePlaceholderTexture()
    {
        var texture = new Texture2D(GraphicsDevice, Constants.TileSize, Constants.TileSize);
        var data = new Microsoft.Xna.Framework.Color[Constants.TileSize * Constants.TileSize];
        for (var i = 0; i < data.Length; i++)
            data[i] = Microsoft.Xna.Framework.Color.White;
        texture.SetData(data);
        PlaceholderTile = texture;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == MonoGameButtonState.Pressed ||
            MonoGameKeyboard.GetState().IsKeyDown(MonoGameKeys.Escape))
            Exit();

        _sceneManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _sceneManager.Draw(_spriteBatch, gameTime);
        base.Draw(gameTime);
    }
}
