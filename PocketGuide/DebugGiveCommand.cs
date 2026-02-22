using Terraria;
using Terraria.ModLoader;

namespace PocketGuide;

#if DEBUG
public class DebugGiveCommand : ModCommand
{
	public override string Command => "give";
	public override string Usage => "/give <itemID> [stack]";
	public override string Description => "Spawn an item by ID.";
	public override CommandType Type => CommandType.Chat;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		if (args.Length < 1 || !int.TryParse(args[0], out int id))
		{
			caller.Reply("Usage: /give <itemID> [stack]");
			return;
		}

		int stack = 1;
		if (args.Length >= 2)
			int.TryParse(args[1], out stack);

		caller.Player.QuickSpawnItem(null, id, stack);
		caller.Reply($"Spawned item {id} x{stack}");
	}
}
#endif
