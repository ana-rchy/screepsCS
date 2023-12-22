using System;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class SpawnHandler {
	static IGame? _game;
	IStructureSpawn _spawn;

	public SpawnHandler(IGame game) {
		_game = game;
		_spawn = _game.Spawns["Spawn1"];
	}

	internal void Run() {
		if (_spawn.Store.GetUsedCapacity(ResourceType.Energy) == 300) {
			var name = $"harvester{_game.Time}";
			_spawn.SpawnCreep(Harvester.Body, name);
			
			_game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(name).SetValue("role", "harvester");
		}
	}
}