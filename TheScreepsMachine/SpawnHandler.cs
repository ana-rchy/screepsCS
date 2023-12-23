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
		if (GetCurrentEnergy() != GetTotalEnergy()) {
			return;
		}

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

		ScreepsMachine.CreepsMemory.Add(name, new());
		ScreepsMachine.CreepsMemory[name].Add("role", role);
		_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("role", role);
		_creepCounts[role]++;

		Spawn.SpawnCreep(body, name);

		if (role == "carrier") { // TODO: put into the first if blocks
			ScreepsMachine.CreepsMemory[name].Add("state", "collecting");
			_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("state", "collecting");
		}

		Console.WriteLine($"{role} count: {_creepCounts[role]}");
	}

	// ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

	internal void CreepDied(string role) {
		_creepCounts[role]--;
		Console.WriteLine($"{role} count: {_creepCounts[role]}");
	}
	
	void CountCreeps() {
		foreach (var creep in ScreepsMachine.CreepsMemory) {
			var role = (string) creep.Value["role"];
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

	#endregion
}