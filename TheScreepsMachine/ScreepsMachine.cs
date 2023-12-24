// the real entrypoint

using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;

internal static class ScreepsMachine {
	private static IGame? _game;

	private static readonly List<Role> Creeps = new();

	internal static void Init(IGame game) {
		_game = game;
		CleanupMemory();
		RefreshCreepsMemory();

		SpawnManager.Init(game);
        Role.Game = game;
	}

	internal static void Loop() {
		CleanupMemory();
		SpawnManager.Run();

		foreach (var creep in Creeps) {
			creep.Run();
		}
	}

	// ------------------------------------------------------------------------------------------------------------------- //
	#region | funcs

	internal static void RegisterCreep(Role creep) {
		Creeps.Add(creep);
	}

	private static void CleanupMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) return;

		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				SpawnManager.CreepDied(creepName);
				creeps.ClearValue(creepName);
				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}

	private static void RefreshCreepsMemory() {
		if (!_game.Memory.TryGetObject("creeps", out var creeps)) return;

		foreach (var creepName in creeps.Keys) {
			Console.Write($"{creepName}:\n");

			string role = "";
			creeps.TryGetObject(creepName, out var creep);

			foreach (var element in creep.Keys) {
				creep.TryGetString(element, out var value);
				if (element == "role") {
					role = value;
				}

				Console.WriteLine($"{element}:{value}\n");
			}

			switch (role) {
				case "harvester":
					Creeps.Add(new Harvester(creepName));
					break;
				case "carrier":
					Creeps.Add(new Carrier(creepName));
					break;
			}
		}
	}

	#endregion
}