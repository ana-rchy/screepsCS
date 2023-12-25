// the real entrypoint

using System;
using System.Collections.Generic;
using ScreepsDotNet.API.World;

internal static class ScreepsMachine {
	private static IGame? _game;

	private static readonly List<Role> Creeps = new();

	internal static void Init(IGame game) {
		_game = game;
		SpawnManager.Init(game);
		Role.Game = game;

		CleanupMemory();
		RefreshCreepsMemory();
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
		var creeps = _game.Memory.GetOrCreateObject("creeps");

		foreach (var creepName in creeps.Keys) {
			if (!_game.Creeps.ContainsKey(creepName)) {
				SpawnManager.CreepDied(creepName);
				creeps.ClearValue(creepName);
				Console.WriteLine($"assassinated {creepName}");
			}
		}
	}

	private static void RefreshCreepsMemory() {
		var creeps = _game.Memory.GetOrCreateObject("creeps");

		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			if (creep == null) {
				Console.WriteLine($"{creepName} not in memory. thats fucked up");
				continue;
			}

			Console.WriteLine($"-- {creepName} --");
			string role = "";

			foreach (var element in creep.Keys) {
				creep.TryGetString(element, out var value);
				if (element == "role") {
					role = value;
				}

				Console.WriteLine($"{element}:{value}");
			}

			switch (role) {
				case "harvester":
					Creeps.Add(new Harvester(creepName));
					break;
				case "carrier":
					Creeps.Add(new Carrier(creepName));
					break;
				case "builder":
					Creeps.Add(new Builder(creepName));
					break;
				case "upgrader":
					Creeps.Add(new Upgrader(creepName));
					break;
				case "melee_defender":
					Creeps.Add(new Melee_Defender(creepName));
					break;
			}

			Console.WriteLine("------------------------");
		}
	}

	#endregion
}