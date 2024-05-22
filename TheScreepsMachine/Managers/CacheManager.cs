using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.World;

#pragma warning disable CS8618

internal static class Cache {
	private static IGame _game;
	private static IRoom _room;
    internal static IEnumerable<ISource> Sources {get; private set;} 
    internal static IEnumerable<IResource> Resources {get; private set;} 
	internal static IEnumerable<IStructure> Structures {get; private set;} 
	internal static IEnumerable<ITombstone> Tombstones {get; private set;} 
	internal static IEnumerable<IConstructionSite> ConstructionSites {get; private set;} 

	private static int _tickCounter;

	internal static void Init(IGame game) {
        _game = game;
		_room = game.Spawns["Spawn1"].Room;
    }

	internal static void Run() {
		Resources = _room.Find<IResource>();
		
		if (_tickCounter % 5 == 0) {
			Sources = _room.Find<ISource>();
			Structures = _room.Find<IStructure>();
			Tombstones = _room.Find<ITombstone>();
			ConstructionSites = _room.Find<IConstructionSite>(true);

			_tickCounter = 0;
		}
		_tickCounter++;
	}
}