using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Role {
    private static IGame? _game = null;
    internal static IGame? Game {
        get {
            return _game;
        }
        set {
            if (_game == null) {
                _game = value;
            }
        }
    }

    protected ICreep? _creep;
    protected string _name;

    // for preexisting creeps
    internal Role(string name) {
        _name = name;
    }

    // for new creeps
    internal Role(IStructureSpawn spawn) {
        _name = $"{GetType().Name.ToLower()}{Game.Time}";
        var energyBudget = 300 + spawn.Room.Find<IStructureExtension>().Count() * 50;

        spawn.SpawnCreep(GetBody(energyBudget), _name);
        _game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(_name)
            .SetValue("role", GetType().Name.ToLower());
        ScreepsMachine.RegisterCreep(this);
    }

    internal virtual void Run() {}
    protected virtual BodyType<BodyPartType> GetBody(int energyBudget) {
        return new();
    }
}