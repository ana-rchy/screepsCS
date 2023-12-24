using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal interface IRole {
	abstract static void Run(ICreep crep);
	abstract static BodyType<BodyPartType> GetBody(int energyBudget);
}