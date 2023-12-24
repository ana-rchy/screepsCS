using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Harvester_old : IRole_old {
	internal static IGame? Game {get; set;}
	
	public static void Run(ICreep creep) {
		creep.Memory.TryGetString("target", out var target);
		var source = Game.GetObjectById<ISource>(target);
		if (source == null) return;

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