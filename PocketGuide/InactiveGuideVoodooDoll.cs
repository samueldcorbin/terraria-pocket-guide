using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PocketGuide;

public class InactiveGuideVoodooDoll : ModItem
{
	public override string Texture => $"Terraria/Images/Item_{ItemID.GuideVoodooDoll}";

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.GuideVoodooDoll);
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		drawColor *= 0.5f;
		spriteBatch.Draw(
			Terraria.GameContent.TextureAssets.Item[ItemID.GuideVoodooDoll].Value,
			position,
			frame,
			drawColor,
			0f,
			origin,
			scale,
			SpriteEffects.None,
			0f);
		return false;
	}
}
