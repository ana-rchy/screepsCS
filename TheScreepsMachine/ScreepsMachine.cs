// the real entrypoint

using System;
using ScreepsDotNet.API.World;

internal class ScreepsMachine {
	static IGame? _game;
	SpawnHandler _spawnHandler;

	public ScreepsMachine(IGame game) {
		_game = game;
		InitCleanup();
		RefreshCreepsMemory();

		_spawnHandler = new SpawnHandler(game);
		Harvester.Game = game;
		Carrier.Game = game;
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
				case "carrier":
					Carrier.Run(creep.Value);
					break;
			}
		}
	}

	// ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

	void InitCleanup() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) return;

		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				creeps.ClearValue(creepName);
				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}

	void CleanupMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) return;

		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				creeps.TryGetObject(creepName, out var creep);
				creep.TryGetString("role", out var role);
				_spawnHandler.CreepDied(role);

				creeps.ClearValue(creepName);
				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}

	void RefreshCreepsMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) return;

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);

			Console.Write($"{creepName}:\n");

			foreach (var element in creep.Keys) {
				creep.TryGetString(element, out var value);

				Console.WriteLine($"{element}:{value}\n");
			}
		}
	}

	#endregion
}