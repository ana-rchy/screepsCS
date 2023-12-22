using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Harvester : IRole {
	static IGame? _game;

	static internal readonly BodyType<BodyPartType> Body = new(
		stackalloc (BodyPartType, int)[] {
			(BodyPartType.Work, 2),
			(BodyPartType.Move, 1),
			(BodyPartType.Tough, 5),
		}
	);

	static internal void Run(ICreep creep) {
		var sources = creep.Room.Find<ISource>();
		if (!sources.Any()) {
			return;
		}

		var source = sources.MinBy(source => source.LocalPosition.LinearDistanceTo(creep.LocalPosition));
		if (source == null) {
			return;
		}

		if (creep.Harvest(source) == CreepHarvestResult.NotInRange) {
			creep.MoveTo(source.RoomPosition);
		}
	}

	static internal BodyType<BodyPartType> GetBody(int energyBudget) {

	}
}