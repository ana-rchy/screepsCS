using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Carrier : Role {
    internal Carrier(string name) : base(name) {}
    internal Carrier(IStructureSpawn spawn) : base(spawn) {
		Game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(_name)
			.SetValue("state", "collecting");
    }

    internal override void Run() {
        var success = Game.Creeps.TryGetValue(_name, out _creep);
		if (!success) return;

        success = _creep.Memory.TryGetString("state", out var state);
		if (!success) {
			_creep.Memory.SetValue("state", "collecting");
			state = "collecting";
		}

		switch (state) {
			case "collecting":
				if (base._creep.Store.GetFreeCapacity() == 0) {
					_creep.Memory.SetValue("state", "transferring");
				}

				List<IRoomObject> energySources = new();
				foreach (var ruin in _creep.Room.Find<IRuin>()) {
					energySources.Add(ruin);
				}
				foreach (var dropped in _creep.Room.Find<IResource>().Where(d => d.ResourceType == ResourceType.Energy)) {
					energySources.Add(dropped);
				}
				if (!energySources.Any()) return;

				var withdrawTarget = energySources.MaxBy(x => GetEnergy(x));
				CommonWithdraw(_creep, withdrawTarget);

				break;

			case "transferring":
				if (_creep.Store.GetUsedCapacity() == 0) {
					_creep.Memory.SetValue("state", "collecting");
				}

                IEnumerable<IStructure> containers = new List<IStructure>();
                containers = containers
                    .Concat(_creep.Room.Find<IStructureSpawn>())
                    .Concat(_creep.Room.Find<IStructureExtension>());
                var transferTarget = containers.First();

				if (_creep.Transfer(transferTarget, ResourceType.Energy) == CreepTransferResult.NotInRange) {
					_creep.MoveTo(transferTarget.LocalPosition);
				}

				break;
		}
    }


    protected override BodyType<BodyPartType> GetBody(int energyBudget) {
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

    static int GetEnergy(IRoomObject energySource) {
		if (energySource is IResource source) {
			return source.Amount;
		} else if (energySource is IRuin ruin) {
			var energy = ruin.Store.GetUsedCapacity(ResourceType.Energy);
			return energy != null ? (int) energy : 0;
		} else {
			return -1;
		}
	}

	static void CommonWithdraw(ICreep creep, IRoomObject withdrawTarget) {
		if (withdrawTarget is IResource target) {
			if (creep.Pickup(target) == CreepPickupResult.NotInRange) {
				creep.MoveTo(target.LocalPosition);
			}
        } else if (withdrawTarget is IRuin ruin) {
			if (creep.Withdraw(ruin, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(ruin.LocalPosition);
			}
		}
	}
}