using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PocketGuide;

public class PocketGuidePlayer : ModPlayer
{
	public bool DollActive;
	public bool DollPresent;

	public override void ResetEffects()
	{
		DollPresent = false;
	}

	public override void SaveData(TagCompound tag)
	{
		if (DollActive)
			tag["dollActive"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		DollActive = tag.ContainsKey("dollActive");
	}
}
