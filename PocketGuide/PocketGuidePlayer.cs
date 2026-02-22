using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PocketGuide;

public class PocketGuidePlayer : ModPlayer
{
	public bool DollActive = true;
	public bool DollPresent;

	public override void ResetEffects()
	{
		DollPresent = false;
	}

	public override void SaveData(TagCompound tag)
	{
		if (!DollActive)
			tag["dollInactive"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		DollActive = !tag.ContainsKey("dollInactive");
	}
}
