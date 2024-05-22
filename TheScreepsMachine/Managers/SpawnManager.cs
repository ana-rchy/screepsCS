using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

#pragma warning disable CS8618

internal static class SpawnManager {
	internal static IStructureSpawn Spawn {get; private set;}
    private static IGame _game;

	private static readonly Dictionary<string, int> _maxCreepCounts = new() {
		{"harvester", 2},
		{"carrier", 2},
		{"builder", 4},
		{"upgrader", 4},
		{"melee_defender", 2}
	};

	private static readonly Dictionary<string, int> _creepCounts = new() {
		{"harvester", 0},
		{"carrier", 0},
		{"builder", 0},
		{"upgrader", 0},
		{"melee_defender", 0}
	};

    internal static void Init(IGame game) {
        _game = game;
        Spawn = _game.Spawns["Spawn1"];
        CountCreeps();
    }

    internal static void Run() {
		if (GetCurrentEnergy() < GetTotalEnergy()) return;

		// determine what role to spawn
		var spawnTargets = _creepCounts
			.Where(x => x.Value < _maxCreepCounts[x.Key]);
		if (!spawnTargets.Any()) return;
		var spawnTarget = spawnTargets.MinBy(x => x.Value);
			
		
		switch (spawnTarget.Key) {
			case "harvester":
				_ = new Harvester(Spawn);
				Console.WriteLine("spawning harvester");
				Console.WriteLine($"harvester: {_creepCounts["harvester"]}");
				break;
			case "carrier":
				_ = new Carrier(Spawn);
				Console.WriteLine("spawning carrier");
				Console.WriteLine($"carrier: {_creepCounts["carrier"]}");
				break;
			case "builder":
				_ = new Builder(Spawn);
				Console.WriteLine("spawning builder");
				Console.WriteLine($"builder: {_creepCounts["builder"]}");
				break;
			case "upgrader":
				_ = new Upgrader(Spawn);
				Console.WriteLine("spawning upgrader");
				Console.WriteLine($"upgrader: {_creepCounts["upgrader"]}");
				break;
			case "melee_defender":
				_ = new Melee_Defender(Spawn);
				Console.WriteLine("spawning melee defender");
				Console.WriteLine($"melee defender: {_creepCounts["melee_defender"]}");
				break;
			default:
				return;
		}

		_creepCounts[spawnTarget.Key]++;
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

	internal static int GetCurrentEnergy() {
		var spawnEnergy = Spawn.Store.GetUsedCapacity(ResourceType.Energy) ?? 0;
		int extensionsEnergy = 0;
		foreach (var extension in Cache.Structures.OfType<IStructureExtension>()) {
			extensionsEnergy += extension.Store.GetUsedCapacity(ResourceType.Energy) ?? 0;
		}
		
		return spawnEnergy + extensionsEnergy;
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

	private static int GetTotalEnergy() {
		if (_creepCounts["harvester"] == 0 || _creepCounts["carrier"] == 0) {
			return 300;
		}

		var extensionsCount = Cache.Structures.OfType<IStructureExtension>().Count();
		return 300 + extensionsCount * 50;
	}

    #endregion
}