using MyRoguelike.Components;

namespace MyRoguelike.Entities;

public class Entity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Microsoft.Xna.Framework.Point Position { get; set; }
    public string Glyph { get; set; } = "@";
    public Microsoft.Xna.Framework.Color Color { get; set; } = Microsoft.Xna.Framework.Color.White;

    private readonly List<IComponent> _components = [];

    public T AddComponent<T>() where T : IComponent, new()
    {
        var component = new T();
        _components.Add(component);
        return component;
    }

    public void AddComponent(IComponent component)
    {
        _components.Add(component);
    }

    public T? GetComponent<T>() where T : IComponent
    {
        return _components.OfType<T>().FirstOrDefault();
    }

    public bool HasComponent<T>() where T : IComponent
    {
        return _components.OfType<T>().Any();
    }

    public void RemoveComponent<T>() where T : IComponent
    {
        var component = GetComponent<T>();
        if (component != null)
            _components.Remove(component);
    }

    public IReadOnlyList<IComponent> Components => _components.AsReadOnly();
}
