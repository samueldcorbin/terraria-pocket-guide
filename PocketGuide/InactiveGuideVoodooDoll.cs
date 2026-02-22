using System;
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

	public override void HoldItem(Player player)
	{
		player.killGuide = true;
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		// If touching lava, become a vanilla Guide Voodoo Doll so
		// vanilla CheckLavaDeath handles WoF spawning.
		if (Collision.LavaCollision(Item.position, Item.width, Item.height))
			Item.ChangeItemType(ItemID.GuideVoodooDoll);
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		var texture = Terraria.GameContent.TextureAssets.Item[ItemID.GuideVoodooDoll].Value;
		var center = new Vector2(frame.Width / 2f, frame.Height / 2f);
		spriteBatch.Draw(
			texture,
			position + (center - origin) * scale,
			frame,
			drawColor,
			MathHelper.Pi,
			center,
			scale,
			SpriteEffects.None,
			0f);
		return false;
	}
}
