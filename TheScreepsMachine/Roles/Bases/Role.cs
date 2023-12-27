using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Role {
    private static IGame _game;
    internal static IGame Game {
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
        var energyBudget = SpawnManager.GetCurrentEnergy();

        var result = spawn.SpawnCreep(GetBody(energyBudget), _name);
        if (result == SpawnCreepResult.Ok) {
            _game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(_name)
                .SetValue("role", GetType().Name.ToLower());
            ScreepsMachine.RegisterCreep(this);
        } else {
            Console.WriteLine(result);
        }
    }

    internal virtual bool Run() {
        // creep may not be in game memory yet
        if (!Game.Creeps.TryGetValue(_name, out _creep)) return false;

        return true;
    }

    protected virtual BodyType<BodyPartType> GetBody(int energyBudget) {
        return new();
    }
}