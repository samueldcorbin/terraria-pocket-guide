using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class InactivePocketGuideItem : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.DontHurtComboBook}";

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.DontHurtComboBookInactive);
	}

	public override bool CanRightClick() => Item.stack == 1;

	public override bool ConsumeItem(Player player) => false;

	public override void RightClick(Player player)
	{
		Item.ChangeItemType(ModContent.ItemType<PocketGuideItem>());
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		var texture = TextureAssets.Item[ItemID.DontHurtComboBook].Value;
		var center = new Vector2(frame.Width / 2f, frame.Height / 2f);
		spriteBatch.Draw(texture, position + (center - origin) * scale, frame, drawColor, MathHelper.Pi, center, scale, SpriteEffects.None, 0f);
		return false;
	}
}
