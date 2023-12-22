using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class SpawnHandler {
	static IGame? _game;
	IStructureSpawn _spawn; // TODO: make it multi-room-able

	Dictionary<string, int> _creepCounts = new();

	public SpawnHandler(IGame game) {
		_game = game;
		_spawn = _game.Spawns["Spawn1"];

		CountCreeps();
	}

	internal void Run() {

		var sourcesCount = _spawn.Room.Find<ISource>().Count();
		string role;
		BodyType<BodyPartType> body;

		if (_creepCounts["harvester"] < sourcesCount) {
			role = "harvester";
			body = Harvester.Body;
		} else {
			return;
		}

		var name = role + _game.Time;
		_spawn.SpawnCreep(body, name);
		_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("role", role);
	}

	
	void CountCreeps() {
		foreach (var creep in _game.Creeps) {
			creep.Value.Memory.TryGetString("role", out var role);
			if (!_creepCounts.TryGetValue(role, out var _)) {
				_creepCounts.Add(role, 1);
			} else {
				_creepCounts[role]++;
			}
		}
	}

	int GetTotalEnergy() {
		var extensionsCount = _spawn.Room.Find<IStructureExtension>().Count();
		return 300 + extensionsCount * 50;
	}
}