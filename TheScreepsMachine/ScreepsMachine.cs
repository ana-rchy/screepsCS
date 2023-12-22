// the real entrypoint

using System;
using ScreepsDotNet.API.World;

internal class ScreepsMachine {
	static IGame? _game;
	SpawnHandler _spawnHandler;

	public ScreepsMachine(IGame game) {
		_game = game;
		_spawnHandler = new SpawnHandler(game);
	}

	internal void Loop() {
		_spawnHandler.Run();
		CleanupMemory();

		foreach (var creep in _game.Creeps) {
			creep.Value.Memory.TryGetString("role", out var role);
			switch (role) {
				case "harvester":
					Harvester.Run(creep.Value);
					break;
			}
		}
	}

	void CleanupMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) {
			return;
		}
		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				creeps.ClearValue(creepName);
				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}
}