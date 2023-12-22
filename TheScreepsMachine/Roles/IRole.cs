using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

internal interface IRole {
	static IGame? _game;

	internal static readonly BodyType<BodyPartType> BodyType;
}