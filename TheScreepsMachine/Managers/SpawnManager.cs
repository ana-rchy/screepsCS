using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal static class SpawnManager {
    private static IGame _game;
	private static IStructureSpawn _spawn; // TODO: make it multi-room-able

	private static readonly Dictionary<string, int> _maxCreepCounts = new() {
		{"harvester", 2},
		{"carrier", 2},
		{"builder", 4},
		{"upgrader", 4}
	};

	private static readonly Dictionary<string, int> _creepCounts = new() {
		{"harvester", 0},
		{"carrier", 0},
		{"builder", 0},
		{"upgrader", 0}
	};

    internal static void Init(IGame game) {
        _game = game;
        _spawn = _game.Spawns["Spawn1"];
        CountCreeps();
    }

    internal static void Run() {
		Console.WriteLine($"harvester: {_creepCounts["harvester"]}");
		Console.WriteLine($"carrier: {_creepCounts["carrier"]}");
		Console.WriteLine($"builder: {_creepCounts["builder"]}");
		Console.WriteLine($"upgrader: {_creepCounts["upgrader"]}");
		
		if (GetCurrentEnergy() != GetTotalEnergy()) return;

		var spawnTarget = _creepCounts
			.Where(x => x.Value < _maxCreepCounts[x.Key])
			.MinBy(x => x.Value);
		
		switch (spawnTarget.Key) {
			case "harvester":
				_ = new Harvester(_spawn);
				_creepCounts[spawnTarget.Key]++;
				Console.WriteLine("spawning harvester");
				break;
			case "carrier":
				_ = new Carrier(_spawn);
				_creepCounts[spawnTarget.Key]++;
				Console.WriteLine("spawning carrier");
				break;
			case "builder":
				_ = new Builder(_spawn);
				_creepCounts[spawnTarget.Key]++;
				Console.WriteLine("spawning builder");
				break;
			case "upgrader":
				_ = new Upgrader(_spawn);
				_creepCounts[spawnTarget.Key]++;
				Console.WriteLine("spawning upgrader");
				break;
		}

        //var sourcesCount = _spawn.Room.Find<ISource>().Count();

        // if (_creepCounts["carrier"] <= _creepCounts["harvester"]) {
        //     _ = new Carrier(_spawn);
		// 	_creepCounts["carrier"]++;
        //     Console.WriteLine("spawning carrier");
        // } else if (_creepCounts["harvester"] < sourcesCount) {
        //     _ = new Harvester(_spawn);
		// 	_creepCounts["harvester"]++;
        //     Console.WriteLine("spawning harvester");
		// } else if (_creepCounts["builder"] < _creepCounts["upgrader"]) {
		// 	_ = new Builder(_spawn);
		// 	_creepCounts["builder"]++;
		// 	Console.WriteLine("spawning builder");
		// } else if (_creepCounts["upgrader"] < sourcesCount * 2) {
		// 	_ = new Upgrader(_spawn);
		// 	_creepCounts["upgrader"]++;
		// 	Console.WriteLine("spawning upgrader");
		// } else {
		// 	Console.WriteLine("no spawn");
		// }
	}

    // ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

	internal static void CreepDied(string creepName) {
		var creep = _game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(creepName);

		var success = creep.TryGetString("role", out var role);
		if (!success) { // fallback
			role = creepName.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
			creep.SetValue("role", role);
		}

		_creepCounts[role]--;
	}

	internal static int GetTotalEnergy() {
		if (_creepCounts["harvester"] == 0 || _creepCounts["carrier"] == 0) {
			return 300;
		}

		var extensionsCount = _spawn.Room.Find<IStructureExtension>().Count();
		return 300 + extensionsCount * 50;
	}

    private static void CountCreeps() {
		var creeps = _game.Memory.GetOrCreateObject("creeps");

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			if (creep == null) {
				Console.WriteLine($"{creepName} not in memory. thats fucked up");
				continue;
			}

			var success = creep.TryGetString("role", out var role);
			if (!success) { // fallback
				role = creepName.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
				creep.SetValue("role", role);
			}

			_creepCounts[role]++;
			Console.WriteLine($"{role} count: {_creepCounts[role]}");
		}
	}

	private static int GetCurrentEnergy() {
		var spawnEnergy = (int) _spawn.Store.GetUsedCapacity(ResourceType.Energy);
		int extensionsEnergy = 0;
		foreach (var extension in _spawn.Room.Find<IStructureExtension>()) {
			extensionsEnergy += (int) extension.Store.GetUsedCapacity(ResourceType.Energy);
		}
		
		return spawnEnergy + extensionsEnergy;
	}

    #endregion
}