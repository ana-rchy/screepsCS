using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal static class Cache {
	internal static Dictionary<string, IEnumerable<IRoomObject>?> RoomObjects = new() {
		{"ISource", null},
		{"IResource", null},
		{"IStructure", null},
		{"ITombstone", null},
	};

	private static IGame _game;
	private static IRoom _room;

	internal static void Init(IGame game) {
        _game = game;
		_room = game.Spawns["Spawn1"].Room;
    }

	internal static void Run() {
		RoomObjects = _room.Find<IRoomObject>();
		foreach (var i in _room.Find<IRoomObject>()) {
			Console.WriteLine(i);
		}
	}


	internal static IEnumerable<T> Find<T>() where T : IRoomObject {
		return RoomObjects.Where(x => x is T).Cast<T>();
	}
}