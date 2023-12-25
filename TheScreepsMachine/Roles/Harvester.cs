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
		if (!success) return; // creep may not be in game memory yet

        success = _creep.Memory.TryGetString("target", out var target);
		if (!success) {
			Console.WriteLine("no harvester target assigned");
			return;
		}

		var source = Game.GetObjectById<ISource>(target);
		if (source == null) {
			Console.WriteLine("invalid harvester target");
			return;
		}

		var result = _creep.Harvest(source);
		if (result == CreepHarvestResult.NotInRange) {
			_creep.MoveTo(source.RoomPosition);
		} else if (result != CreepHarvestResult.Ok) {
			Console.WriteLine($"{_name}: {result}");
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
			body[1].Item2++;
			energyBudget -= 100;
		}

		while (energyBudget - 10 >= 0) {
			body[2].Item2++;
			energyBudget -= 10;
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
			if (creep == null) {
				Console.WriteLine($"{creepName} not in memory. thats fucked up");
				continue;
			}

			var success = creep.TryGetString("target", out var targetID);
			if (success) {
				occupiedSourceIDs.Add(targetID);
			} else {
				Console.WriteLine($"{creepName} had no target property");
			}
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