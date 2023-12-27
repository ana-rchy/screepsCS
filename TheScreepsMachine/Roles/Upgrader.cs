using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Upgrader : Collector {
    internal Upgrader(string name) : base(name) {}
    internal Upgrader(IStructureSpawn spawn) : base(spawn) {}

    internal override bool Run() {
		if (!base.Run()) return false;
		
        var success = Game.Creeps.TryGetValue(_name, out _creep);
		if (!success) return false;

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
					return false;
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

		return true;
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
}