using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideGlobalItem : GlobalItem
{
	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return entity.type == ItemID.GuideVoodooDoll || entity.ModItem is InactiveGuideVoodooDoll;
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
		if (item.type == ItemID.GuideVoodooDoll)
			item.ChangeItemType(ModContent.ItemType<InactiveGuideVoodooDoll>());
		else
			item.ChangeItemType(ItemID.GuideVoodooDoll);
	}

	public override void UpdateInventory(Item item, Player player)
	{
		if (item.type == ItemID.GuideVoodooDoll)
			player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void UpdateAccessory(Item item, Player player, bool hideVisual)
	{
		if (item.type == ItemID.GuideVoodooDoll)
			player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		string fullTooltip = item.type == ItemID.GuideVoodooDoll
			? Language.GetTextValue("ItemTooltip.DontHurtCrittersBook")
			: Language.GetTextValue("ItemTooltip.DontHurtCrittersBookInactive");

		// Extract the last line (the toggle instruction)
		int lastNewline = fullTooltip.LastIndexOf('\n');
		string toggleText = lastNewline >= 0 ? fullTooltip[(lastNewline + 1)..] : fullTooltip;
		toggleText = Lang.SupportGlyphs(toggleText);

		var line = new TooltipLine(Mod, "PocketGuideToggle", toggleText);
		int idx = tooltips.FindIndex(t => t.Name == "JourneyResearch");
		if (idx >= 0)
			tooltips.Insert(idx, line);
		else
			tooltips.Add(line);
	}

	public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
	{
		bool equippedIsDoll = equippedItem.type == ItemID.GuideVoodooDoll || equippedItem.ModItem is InactiveGuideVoodooDoll;
		bool incomingIsDoll = incomingItem.type == ItemID.GuideVoodooDoll || incomingItem.ModItem is InactiveGuideVoodooDoll;
		if (equippedIsDoll && incomingIsDoll)
			return false;
		return true;
	}
}
