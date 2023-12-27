using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal class Collector : Role {
	internal Collector(string name) : base(name) {}
    internal Collector(IStructureSpawn spawn) : base(spawn) {
        Game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(_name)
			.SetValue("state", "collecting");
    }

    protected string GetCollectionTarget() {
		List<IRoomObject> energySources = new();
		foreach (var container in Cache.Find<IStructureContainer>().Where(x => x.Store.GetUsedCapacity(ResourceType.Energy) != 0)) {
			energySources.Add(container);
		}
		foreach (var dropped in Cache.Find<IResource>().Where(x => x.ResourceType == ResourceType.Energy)) {
			energySources.Add(dropped);
		}
		foreach (var tombstone in Cache.Find<ITombstone>()) {
			energySources.Add(tombstone);
		}

		var validSources = energySources.Where(x => GetEnergy(x) >= 10);
		if (!validSources.Any()) {
			Console.WriteLine($"{_name}: no valid collection targets found");
			return "null";
		}

		var withdrawTarget = validSources.MaxBy(x => GetEnergy(x));
		
		return ((IWithId?) withdrawTarget) != null ? ((IWithId?) withdrawTarget).Id : "null";
	}

    protected static int GetEnergy(IRoomObject energySource) {
		if (energySource is IResource source) {
			return source.Amount;
		} else if (energySource is IRuin ruin) {
			var energy = ruin.Store.GetUsedCapacity(ResourceType.Energy);
			return energy != null ? (int) energy : 0;
		} else {
			return -1;
		}
	}

	protected static void CommonWithdraw(ICreep creep, IRoomObject withdrawTarget) {
		if (withdrawTarget is IStructureContainer container) {
			if (creep.Withdraw(container, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(container.LocalPosition);
			}
		} else if (withdrawTarget is IResource target) {
			if (creep.Pickup(target) == CreepPickupResult.NotInRange) {
				creep.MoveTo(target.LocalPosition);
			}
		} else if (withdrawTarget is ITombstone tombstone) {
			if (creep.Withdraw(tombstone, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(tombstone.LocalPosition);
			}
        }
	}
}