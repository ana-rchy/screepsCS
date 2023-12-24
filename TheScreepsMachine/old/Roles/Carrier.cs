using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Carrier_old : IRole_old {
	internal static IGame? Game {get; set;}

	public static void Run(ICreep creep) {
		creep.Memory.TryGetString("state", out var state);

		switch (state) {
			case "collecting":
				if (creep.Store.GetFreeCapacity() == 0) {
					creep.Memory.SetValue("state", "transferring");
				}

				List<IRoomObject> energySources = new();
				foreach (var ruin in creep.Room.Find<IRuin>()) {
					energySources.Add(ruin);
				}
				foreach (var dropped in creep.Room.Find<IResource>().Where(d => d.ResourceType == ResourceType.Energy)) {
					energySources.Add(dropped);
				}
				if (!energySources.Any()) return;

				var target = energySources.MaxBy(x => GetEnergy(x));
				CommonWithdraw(creep, target);

				break;

			case "transferring":
				if (creep.Store.GetUsedCapacity() == 0) {
					creep.Memory.SetValue("state", "collecting");
				}

				if (creep.Transfer(SpawnHandler.Spawn, ResourceType.Energy) == CreepTransferResult.NotInRange) {
					creep.MoveTo(SpawnHandler.Spawn.LocalPosition);
				}

				break;
		}
	}

	public static BodyType<BodyPartType> GetBody(int energyBudget) {
		(BodyPartType, int)[] body = new[] {
			(BodyPartType.Carry, 3),
			(BodyPartType.Move, 3)
		};
		energyBudget -= 300;

		while (energyBudget - 50 >= 0) {
			body[0].Item2++;
			energyBudget -= 50;

			if (energyBudget - 50 >= 0) {
				body[1].Item2++;
				energyBudget -= 50;
			}
		}

		return new(body);
	}

	// ------------------------------------------------------------------------------------------------------------------- //
	#region | other funcs

	static int GetEnergy(IRoomObject energySource) {
		if (energySource is IResource) {
			return ((IResource) energySource).Amount;
		} else if (energySource is IRuin) {
			var energy = ((IRuin) energySource).Store.GetUsedCapacity(ResourceType.Energy);
			return energy != null ? (int) energy : 0;
		} else {
			return -1;
		}
	}

	static void CommonWithdraw(ICreep creep, IRoomObject withdrawTarget) {
		if (withdrawTarget is IResource) {
			var target = (IResource) withdrawTarget;
			if (creep.Pickup(target) == CreepPickupResult.NotInRange) {
				creep.MoveTo(target.LocalPosition);
			}
		} else if (withdrawTarget is IRuin) {
			var target = (IRuin) withdrawTarget;
			if (creep.Withdraw(target, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(target.LocalPosition);
			}
		}
	}

	#endregion
}