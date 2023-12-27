using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal static class Cache {
	internal static IEnumerable<IRoomObject> RoomObjects = new List<IRoomObject>();

	private static IGame _game;
	private static IRoom _room;

	internal static void Init(IGame game) {
        _game = game;
		_room = game.Rooms["E38S44"];
    }

	internal static void Run() {
		RoomObjects = _room.Find<IRoomObject>();
	}


	internal static IEnumerable<T> Find<T>() {
		return (IEnumerable<T>) RoomObjects.Where(x => x is T);
	}
}