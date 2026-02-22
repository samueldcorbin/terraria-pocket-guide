using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideGlobalItem : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.type == ItemID.GuideVoodooDoll;
	}

	public override bool CanRightClick(Item item)
	{
		return item.stack == 1;
	}

	public override bool ConsumeItem(Item item, Player player)
	{
		return false;
	}

	public override void RightClick(Item item, Player player)
	{
		item.ChangeItemType(ModContent.ItemType<InactiveGuideVoodooDoll>());
	}

	public override void UpdateInventory(Item item, Player player)
	{
		player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
		player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}
}
