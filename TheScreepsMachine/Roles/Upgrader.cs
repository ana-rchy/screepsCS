using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Upgrader : Role {
    internal Upgrader(string name) : base(name) {}
    internal Upgrader(IStructureSpawn spawn) : base(spawn) {
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
                success = _creep.Memory.TryGetString("collectionTarget", out _);
				if (_creep.Store.GetFreeCapacity(ResourceType.Energy) == 0 ||
					(!success && _creep.Store.GetUsedCapacity(ResourceType.Energy) > 50)) {
					_creep.Memory.SetValue("state", "upgrading");
					return;
				}

				success = _creep.Memory.TryGetString("collectionTarget", out var collectionTarget);
				if (!success || Game.GetObjectById<IRoomObject>(collectionTarget) == null) {
					collectionTarget = GetCollectionTarget();
					if (collectionTarget == null) {
						Console.WriteLine("no carrier collection targets found");
						break;
					}

					_creep.Memory.SetValue("collectionTarget", collectionTarget);
				}

				CommonWithdraw(_creep, Game.GetObjectById<IRoomObject>(collectionTarget));

				break;

            case "upgrading":
				if (_creep.Store.GetUsedCapacity() == 0) {
					_creep.Memory.SetValue("state", "collecting");
				}

                var controller = _creep.Room.Controller;
				var result = _creep.UpgradeController(controller);
                if (result == CreepUpgradeControllerResult.NotInRange) {
                    _creep.MoveTo(controller.LocalPosition);
                } else if (result != CreepUpgradeControllerResult.Ok) {
					Console.WriteLine($"{_name}: {result}");
				}

                break;
        }
    }


    protected override BodyType<BodyPartType> GetBody(int energyBudget) {
		(BodyPartType, int)[] body = new[] {
            (BodyPartType.Work, 2),
			(BodyPartType.Carry, 1),
			(BodyPartType.Move, 1)
		};
		energyBudget -= 300;

		while (true) {
            if (energyBudget - 50 >= 0) {
                body[1].Item2++;
                energyBudget -= 50;
            } else {
                break;
            }
            if (energyBudget - 50 >= 0) {
                body[2].Item2++;
                energyBudget -= 50;
            }
            if (energyBudget - 100 >= 0) {
                body[0].Item2++;
                energyBudget -= 100;
            }
        }

		return new(body);
	}

	private string GetCollectionTarget() {
		List<IRoomObject> energySources = new();
		foreach (var dropped in _creep.Room.Find<IResource>().Where(d => d.ResourceType == ResourceType.Energy)) {
			energySources.Add(dropped);
		}
		foreach (var tombstone in _creep.Room.Find<ITombstone>()) {
			energySources.Add(tombstone);
		}
		foreach (var ruin in _creep.Room.Find<IRuin>()) {
			energySources.Add(ruin);
		}
		var validSources = energySources.Where(x => GetEnergy(x) >= 10);
		if (!validSources.Any()) {
			Console.WriteLine($"{_name}: no valid collection targets found");
			return "null";
		}

		var withdrawTarget = validSources.MaxBy(x => GetEnergy(x));
		
		return ((IWithId?) withdrawTarget) != null ? ((IWithId?) withdrawTarget).Id : "null";
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
		} else if (withdrawTarget is ITombstone tombstone) {
			if (creep.Withdraw(tombstone, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(tombstone.LocalPosition);
			}
        } else if (withdrawTarget is IRuin ruin) {
			if (creep.Withdraw(ruin, ResourceType.Energy) == CreepWithdrawResult.NotInRange) {
				creep.MoveTo(ruin.LocalPosition);
			}
		}
	}
}