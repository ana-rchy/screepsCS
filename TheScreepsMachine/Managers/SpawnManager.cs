using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal static class SpawnManager {
    private static IGame _game;
	private static IStructureSpawn _spawn; // TODO: make it multi-room-able

	private static readonly Dictionary<string, int> _creepCounts = new() {
		{"harvester", 0},
		{"carrier", 0}
	};

    internal static void Init(IGame game) {
        _game = game;
        _spawn = _game.Spawns["Spawn1"];
        CountCreeps();
    }

    internal static void Run() {
		Console.WriteLine($"harvester: {_creepCounts["harvester"]}");
		Console.WriteLine($"carrier: {_creepCounts["carrier"]}");
		
		if (GetCurrentEnergy() != GetTotalEnergy()) return;

        var sourcesCount = _spawn.Room.Find<ISource>().Count();

        if (_creepCounts["carrier"] <= _creepCounts["harvester"]) {
            _ = new Carrier(_spawn);
			_creepCounts["carrier"]++;
            Console.WriteLine("spawning carrier");
        } else if (_creepCounts["harvester"] < sourcesCount) {
            _ = new Harvester(_spawn);
			_creepCounts["harvester"]++;
            Console.WriteLine("spawning harvester");
        } else {
            return;
        }
	}

    // ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

    private static void CountCreeps() {
		var creeps = _game.Memory.GetOrCreateObject("creeps");

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			creep.TryGetString("role", out var role);
			_creepCounts[role]++;
			Console.WriteLine($"{role} count: {_creepCounts[role]}");
		}
	}

	internal static void CreepDied(string creepName) {
		_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(creepName)
			.TryGetString("role", out var role);
		_creepCounts[role]--;
	}

	private static int GetCurrentEnergy() {
		var spawnEnergy = (int) _spawn.Store.GetUsedCapacity(ResourceType.Energy);
		int extensionsEnergy = 0;
		foreach (var extension in _spawn.Room.Find<IStructureExtension>()) {
			extensionsEnergy += (int) extension.Store.GetUsedCapacity(ResourceType.Energy);
		}
		
		return spawnEnergy + extensionsEnergy;
	}

	private static int GetTotalEnergy() {
		var extensionsCount = _spawn.Room.Find<IStructureExtension>().Count();
		return 300 + extensionsCount * 50;
	}

    #endregion
}