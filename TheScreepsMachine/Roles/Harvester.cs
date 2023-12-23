using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Harvester : IRole {
	public static void Run(ICreep creep) {
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

	public static BodyType<BodyPartType> GetBody(int energyBudget) {
		(BodyPartType, int)[] body = new[] {
			(BodyPartType.Move, 1),
			(BodyPartType.Work, 2),
			(BodyPartType.Tough, 5)
		};
		energyBudget -= 300;

		while (energyBudget - 100 >= 0) {
			body[0].Item2++;
			energyBudget -= 100;
		}

		while (energyBudget - 10 >= 0) {
			body[1].Item2++;
		}

		return new(body);
	}
}