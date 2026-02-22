using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuidePlayer : ModPlayer
{
	public bool DollPresent;

	public override void ResetEffects()
	{
		DollPresent = false;
	}
}
