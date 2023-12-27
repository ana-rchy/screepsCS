using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.World;

#pragma warning disable CS8618

internal static class Cache {
	// internal static readonly Dictionary<string, IEnumerable<IRoomObject>?> RoomObjects = new() {
	// 	{"ISource", null},
	// 	{"IResource", null},
	// 	{"IStructure", null},
	// 	{"ITombstone", null},
	// };

	private static IGame _game;
	private static IRoom _room;
    internal static IEnumerable<ISource> Sources;
    internal static IEnumerable<IResource> Resources;
	internal static IEnumerable<IStructure> Structures;
	internal static IEnumerable<ITombstone> Tombstones;
	internal static IEnumerable<IConstructionSite> ConstructionSites;

	private static int _tickCounter;

	internal static void Init(IGame game) {
        _game = game;
		_room = game.Spawns["Spawn1"].Room;
    }

	internal static void Run() {
		Resources = _room.Find<IResource>();
		
		if (_tickCounter % 5 == 0) {
			Sources = _room.Find<ISource>();
			Structures = _room.Find<IStructure>(true);
			Tombstones = _room.Find<ITombstone>();
			ConstructionSites = _room.Find<IConstructionSite>(true);

			_tickCounter = 0;
		}
		_tickCounter++;
	}

	internal static IEnumerable<T> Find<T>(IEnumerable<IRoomObject> objects) where T : IRoomObject {
		return objects.Where(x => x is T).Cast<T>();
	}

	// internal static IEnumerable<T> Find<T>() where T : IRoomObject {
	// 	return RoomObjects.Where(x => x is T).Cast<T>();
	// }
}