using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal sealed class Harvester : Role {
    internal Harvester(string name) : base(name) {}
    internal Harvester(IStructureSpawn spawn) : base(spawn) {
		Game.Memory.GetOrCreateObject("creeps").GetOrCreateObject(_name)
			.SetValue("target", GetHarvesterTarget(spawn).Id);
    }

    internal override void Run() {
        var success = Game.Creeps.TryGetValue(_name, out _creep);
		if (!success) return;

        success = _creep.Memory.TryGetString("target", out var target);
		if (!success) {
			Console.WriteLine("no target assigned");
			return;
		}

		var source = Game.GetObjectById<ISource>(target);
		if (source == null) return;

		if (_creep.Harvest(source) == CreepHarvestResult.NotInRange) {
			_creep.MoveTo(source.RoomPosition);
		}
    }



    protected override BodyType<BodyPartType> GetBody(int energyBudget) {
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

    private static ISource GetHarvesterTarget(IStructureSpawn spawn) {
		var allSources = spawn.Room.Find<ISource>();
		List<string> allSourceIDs = new();
		foreach (var source in allSources) {
			allSourceIDs.Add(source.Id.ToString());
		}
		
		List<string> occupiedSourceIDs = new();
		var creeps = Game.Memory.GetOrCreateObject("creeps");
		foreach (var creepName in creeps.Keys) {
			creeps.TryGetObject(creepName, out var creep);
			creep.TryGetString("target", out var targetID);
			occupiedSourceIDs.Add(targetID);
		}

		foreach (var id in occupiedSourceIDs) {
			allSourceIDs.Remove(id);
		}

		if (!allSourceIDs.Any()) {
			Console.WriteLine("uh oh!!! fucky wucky!!! no source to harvest!!!");
			return Game.GetObjectById<ISource>(allSources.First().Id);
		}

		return Game.GetObjectById<ISource>(allSourceIDs[0]);
	}
}