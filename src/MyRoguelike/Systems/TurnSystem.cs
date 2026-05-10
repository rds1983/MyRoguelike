using MyRoguelike.Entities;

namespace MyRoguelike.Systems;

public class TurnSystem
{
    private readonly List<TurnActor> _actors = [];
    private int _currentIndex;

    public int TurnNumber { get; private set; } = 1;
    public int ActorCount => _actors.Count;

    public Entity? CurrentActor =>
        _actors.Count > 0 && _currentIndex < _actors.Count ? _actors[_currentIndex].Entity : null;

    public void AddEntity(Entity entity, int speed)
    {
        _actors.Add(new TurnActor(entity, speed));
        _actors.Sort((a, b) => b.Speed.CompareTo(a.Speed));
    }

    public void RemoveEntity(Entity entity)
    {
        var idx = _actors.FindIndex(a => a.Entity == entity);
        if (idx < 0) return;

        _actors.RemoveAt(idx);
        if (_currentIndex >= _actors.Count && _actors.Count > 0)
            _currentIndex = 0;
        else if (_currentIndex > idx)
            _currentIndex--;
    }

    public void NextTurn()
    {
        _currentIndex++;
        if (_currentIndex >= _actors.Count && _actors.Count > 0)
        {
            _currentIndex = 0;
            TurnNumber++;
        }
    }

    public void Clear()
    {
        _actors.Clear();
        _currentIndex = 0;
        TurnNumber = 1;
    }

    private record TurnActor(Entity Entity, int Speed);
}
