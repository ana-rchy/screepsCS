// the real entrypoint

using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;

internal class ScreepsMachine {
	static IGame? _game;
	SpawnHandler _spawnHandler;

	static internal Dictionary<string, Dictionary<string, object>> CreepsMemory {get; private set;} = new();

	public ScreepsMachine(IGame game) {
		_game = game;
		CleanupMemory();
		RefreshCreepsMemory();

		_spawnHandler = new SpawnHandler(game);
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

	void RefreshCreepsMemory() {
		CreepsMemory.Clear();

		if (!_game.Memory.TryGetObject("creeps", out var creeps)) {
			return;
		}

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			CreepsMemory.Add(creepName, new());

			Console.Write($"{creepName}:\t");

			foreach (var element in creep.Keys) {
				creep.TryGetString(element, out var value);
				CreepsMemory[creepName].Add(element, value);

				Console.WriteLine($"{element}:{value}");
			}
		}
	}

	void CleanupMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) {
			return;
		}

		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				CreepsMemory.Remove(creepName);
				creeps.ClearValue(creepName);

				_spawnHandler.CreepDied((string) CreepsMemory[creepName]["role"]);

				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}

	#endregion
}