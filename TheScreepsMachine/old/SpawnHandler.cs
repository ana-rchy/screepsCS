using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class SpawnHandler {
	static IGame? _game;
	static internal IStructureSpawn Spawn {get; private set;} // TODO: make it multi-room-able

	Dictionary<string, int> _creepCounts = new() {
		{"harvester", 0},
		{"carrier", 0}
	};

	public SpawnHandler(IGame game) {
		_game = game;
		Spawn = _game.Spawns["Spawn1"];

		CountCreeps();
	}

	internal void Run() {
		Console.WriteLine($"harvester: {_creepCounts["harvester"]}");
		Console.WriteLine($"carrier: {_creepCounts["carrier"]}");
		
		if (GetCurrentEnergy() != GetTotalEnergy()) return;

		var sourcesCount = Spawn.Room.Find<ISource>().Count();
		var energyBudget = GetTotalEnergy();
		string role;
		BodyType<BodyPartType> body;

		if (_creepCounts["harvester"] < sourcesCount && _creepCounts["harvester"] <= _creepCounts["carrier"]) {
			role = "harvester";
			body = Harvester.GetBody(energyBudget);
			Console.WriteLine("spawning harvester");
		} else if (_creepCounts["carrier"] < _creepCounts["harvester"]) {
			role = "carrier";
			body = Carrier.GetBody(energyBudget);
			Console.WriteLine("spawning carrier");
		} else {
			return;
		}

		var name = role + _game.Time;

		_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("role", role);
		_creepCounts[role]++;

		Spawn.SpawnCreep(body, name);

		if (role == "harvester") {
			var target = GetHarvesterTarget();
			_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("target", target.Id);
		} else if (role == "carrier") {
			_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("state", "collecting");
		}

		Console.WriteLine($"{role} count: {_creepCounts[role]}");
	}

	// ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

	internal void CreepDied(string role) {
		if (!_creepCounts.TryGetValue(role, out _)) return;
		
		_creepCounts[role]--;
		Console.WriteLine($"{role} count: {_creepCounts[role]}");
	}
	
	void CountCreeps() {
		_game.Memory.TryGetObject("creeps", out var creeps);

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			creep.TryGetString("role", out var role);
			_creepCounts[role]++;
			Console.WriteLine($"{role} count: {_creepCounts[role]}");
		}
	}

	int GetCurrentEnergy() {
		var spawnEnergy = (int) Spawn.Store.GetUsedCapacity(ResourceType.Energy);
		int extensionsEnergy = 0;
		foreach (var extension in Spawn.Room.Find<IStructureExtension>()) {
			extensionsEnergy += (int) extension.Store.GetUsedCapacity(ResourceType.Energy);
		}
		
		return spawnEnergy + extensionsEnergy;
	}

	int GetTotalEnergy() {
		var extensionsCount = Spawn.Room.Find<IStructureExtension>().Count();
		return 300 + extensionsCount * 50;
	}

	ISource GetHarvesterTarget() {
		var allSources = Spawn.Room.Find<ISource>();
		List<string> allSourceIDs = new();
		foreach (var source in allSources) {
			allSourceIDs.Add(source.Id.ToString());
		}
		
		List<string> occupiedSourceIDs = new();
		var creeps = _game.Memory.GetOrCreateObject("creeps");
		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			creep.TryGetString("target", out var targetID);
			occupiedSourceIDs.Add(targetID);
		}

		foreach (var id in occupiedSourceIDs) {
			allSourceIDs.Remove(id);
		}

		return _game.GetObjectById<ISource>(allSourceIDs[0]);
	}

	#endregion
}