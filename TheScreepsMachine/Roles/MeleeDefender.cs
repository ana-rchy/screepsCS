using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Melee_Defender : Role {
    internal Melee_Defender(string name) : base(name) {}
    internal Melee_Defender(IStructureSpawn spawn) : base(spawn) {}

    internal override bool Run() {
        if (!base.Run()) return false;

        var targets = _creep.Room.Find<ICreep>()
            .Where(x => {
                if (x.My) return false;

                var partList = x.BodyType.AsBodyPartList;
                return partList.Contains(BodyPartType.Attack) || partList.Contains(BodyPartType.RangedAttack) || partList.Contains(BodyPartType.Heal);
            });
        if (!targets.Any()) return false;
        
        var target = targets.MinBy(x => _creep.LocalPosition.LinearDistanceTo(x.LocalPosition));
        
        var result = _creep.Attack(target);
        if (result == CreepAttackResult.NotInRange) {
            _creep.MoveTo(target.LocalPosition);
        } else if (result != CreepAttackResult.Ok) {
            Console.WriteLine($"{_name}: {result}");
        }

        return true;
    }


    protected override BodyType<BodyPartType> GetBody(int energyBudget) {
        (BodyPartType, int)[] body = new[] {
			(BodyPartType.Attack, 2),
            (BodyPartType.Move, 2),
            (BodyPartType.Tough, 4)
		};
		energyBudget -= 300;

        while (true) {
            if (energyBudget - 80 >= 0) {
                body[0].Item2++;
                energyBudget -= 80;
            }
            if (energyBudget - 50 >= 0) {
                body[1].Item2++;
                energyBudget -= 50;
            }
            if (energyBudget - 10 >= 0) {
                body[2].Item2++;
                energyBudget -= 10;
            } else {
                break;
            }
        }

        return new(body);
    }
}