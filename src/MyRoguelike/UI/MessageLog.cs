using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace MyRoguelike.UI;

public class LogEntry
{
    public string Message { get; }
    public Color Color { get; }

    public LogEntry(string message, Color color)
    {
        Message = message;
        Color = color;
    }
}

public class MessageLog
{
    private readonly List<LogEntry> _entries = [];
    private const int MaxEntries = 100;

    public IReadOnlyList<LogEntry> Entries => _entries.AsReadOnly();
    public int Count => _entries.Count;

    public void Add(string message, Color color)
    {
        _entries.Add(new LogEntry(message, color));
        if (_entries.Count > MaxEntries)
            _entries.RemoveAt(0);
    }

    public void Add(string message)
    {
        Add(message, Color.White);
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public void Draw(SpriteBatch spriteBatch, SpriteFont font, int x, int y,
        int maxLines, int lineHeight = 20)
    {
        var start = Math.Max(0, _entries.Count - maxLines);
        for (var i = start; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            spriteBatch.DrawString(font, entry.Message,
                new Vector2(x, y + (i - start) * lineHeight), entry.Color);
        }
    }
}
