using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Carrier : Collector {
    internal Carrier(string name) : base(name) {}
    internal Carrier(IStructureSpawn spawn) : base(spawn) {}

    internal override bool Run() {
		if (!base.Run()) return false;

        var success = _creep.Memory.TryGetString("state", out var state);
		if (!success) {
			_creep.Memory.SetValue("state", "collecting");
			state = "collecting";
		}

		switch (state) {
			case "collecting":
				success = _creep.Memory.TryGetString("collectionTarget", out _);
				if (_creep.Store.GetFreeCapacity(ResourceType.Energy) == 0 ||
					(!success && _creep.Store.GetUsedCapacity(ResourceType.Energy) > 50)) {
					_creep.Memory.SetValue("state", "transferring");
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

			case "transferring":
				if (_creep.Store.GetUsedCapacity(ResourceType.Energy) == 0) {
					_creep.Memory.SetValue("state", "collecting");
					return false;
				}

                IEnumerable<IWithStore> containers = new List<IWithStore>();
                containers = containers
                    .Concat(Cache.Find<IStructureSpawn>())
                    .Concat(Cache.Find<IStructureExtension>());
                var transferTarget = (IStructure?) containers
					.Where(x => x.Store.GetFreeCapacity(ResourceType.Energy) != 0)
                    .MinBy(x => x.Store.GetFreeCapacity(ResourceType.Energy));
				
				if (transferTarget == null) {
					_creep.Memory.SetValue("state", "collecting");
					return false;
				}

				var result = _creep.Transfer(transferTarget, ResourceType.Energy);
				if (result == CreepTransferResult.NotInRange) {
					_creep.MoveTo(transferTarget.LocalPosition);
				} else if (result != CreepTransferResult.Ok) {
					Console.WriteLine($"{_name}: {result}");
				}

				break;
		}

		return true;
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
}