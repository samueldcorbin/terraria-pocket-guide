using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class PocketGuideItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.DontHurtComboBook}";

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.DontHurtComboBook);
	}

	public override bool CanRightClick() => Item.stack == 1;

	public override bool ConsumeItem(Player player) => false;

	public override void RightClick(Player player)
	{
		Item.ChangeItemType(ModContent.ItemType<InactivePocketGuideItem>());
	}

	public override void UpdateInventory(Player player)
	{
		player.GetModPlayer<PocketGuidePlayer>().DollPresent = true;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.GuideVoodooDoll)
			.AddIngredient(ItemID.PurificationPowder, 30)
			.Register();
	}
}
