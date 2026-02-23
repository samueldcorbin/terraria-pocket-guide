using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace PocketGuide;

public class PocketGuideSystem : ModSystem
{
	public static bool ForceGuideUI { get; private set; }
	private static bool _wasForceGuideUI;
	private static int _lastSourceType;
	private static int _savedFocusRecipe;

	public override void Load()
	{
		IL_Main.DrawInventory += PatchCloseGuard;
		IL_Main.DrawInventory += PatchSkipMaterialSlot;

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideHover",
				BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(Item[]), typeof(int), typeof(int) }, null),
			PatchOverrideHover);

		MonoModHooks.Modify(
			typeof(ItemSlot).GetMethod("OverrideLeftClick",
				BindingFlags.NonPublic | BindingFlags.Static),
			PatchOverrideLeftClick);

		On_Player.dropItemCheck += HookDropItemCheck;
	}

	public override void PostUpdateInput()
	{
		if (Main.gameMenu || Main.netMode == NetmodeID.Server)
			return;

		var player = Main.LocalPlayer;
		bool active = player.GetModPlayer<PocketGuidePlayer>().DollPresent;
		bool guideAlive = NPC.AnyNPCs(NPCID.Guide);

		bool pocketGuideActive = active && guideAlive && Main.playerInventory
			&& player.chest == -1 && Main.npcShop == 0 && !Main.InReforgeMenu;

		if (pocketGuideActive)
		{
			int sourceType = !Main.mouseItem.IsAir ? Main.mouseItem.type
				: (player.mouseInterface && !Main.HoverItem.IsAir) ? Main.HoverItem.type
				: 0;

			// Non-material items can't have guide recipes; treat like air.
			bool sourceMaterial = sourceType != 0 &&
				(!Main.mouseItem.IsAir ? Main.mouseItem.material : Main.HoverItem.material);

			if (sourceType != _lastSourceType)
			{
				_lastSourceType = sourceType;

				if (sourceType != 0 && sourceMaterial)
				{
					if (!ForceGuideUI)
						_savedFocusRecipe = Main.focusRecipe;
					Main.guideItem.SetDefaults(sourceType);
					Recipe.FindRecipes();
					if (Main.numAvailableRecipes == 0)
					{
						Main.guideItem = new Item();
						RestoreNormalCrafting();
					}
					else
					{
						Main.focusRecipe = 0;
						SnapRecipeScroll();
					}
				}
				else if (ForceGuideUI)
				{
					Main.guideItem = new Item();
					RestoreNormalCrafting();
				}
			}

			ForceGuideUI = !Main.guideItem.IsAir;
			Main.InGuideCraftMenu = ForceGuideUI;
		}
		else
		{
			ForceGuideUI = false;
			if (_wasForceGuideUI)
			{
				Main.InGuideCraftMenu = false;
				Main.guideItem = new Item();
				RestoreNormalCrafting();
			}
			_lastSourceType = 0;
		}

		_wasForceGuideUI = ForceGuideUI;
	}

	private static void RestoreNormalCrafting()
	{
		Recipe.FindRecipes();
		Main.focusRecipe = Math.Clamp(_savedFocusRecipe, 0,
			Math.Max(0, Main.numAvailableRecipes - 1));
		SnapRecipeScroll();
	}

	private static void SnapRecipeScroll()
	{
		if (Main.numAvailableRecipes == 0)
			return;
		float offset = Main.availableRecipeY[Main.focusRecipe];
		for (int i = 0; i < Main.availableRecipeY.Length; i++)
			Main.availableRecipeY[i] -= offset;
	}

	// Prevent dropItemCheck from "returning" virtual guideItem to inventory.
	// Save/restore so guideItem survives into the draw phase for DrawGuideCraftText.
	private static void HookDropItemCheck(On_Player.orig_dropItemCheck orig, Player self)
	{
		if (ForceGuideUI)
		{
			var saved = Main.guideItem;
			Main.guideItem = new Item();
			orig(self);
			Main.guideItem = saved;
		}
		else
			orig(self);
	}

	// Bypass talkNPC == -1 in the InGuideCraftMenu close guard.
	// Vanilla: if (chest != -1 || npcShop != 0 || talkNPC == -1 || InReforgeMenu)
	// Patch: replace talkNPC value with 0 when ForceGuideUI so the == -1 check fails.
	private static void PatchCloseGuard(ILContext il)
	{
		var c = new ILCursor(il);

		// Skip past first InGuideCraftMenu load (reforge close guard at ~line 54130)
		c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		// Find second InGuideCraftMenu load (the else-if branch at ~line 54259)
		c.GotoNext(i => i.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		// Find talkNPC property getter in the close guard condition
		c.GotoNext(MoveType.After, i =>
			i.OpCode == OpCodes.Callvirt
			&& i.Operand is MethodReference mr
			&& mr.Name == "get_talkNPC");
		// Replace talkNPC value on stack
		c.EmitDelegate<Func<int, int>>(talkNPC => ForceGuideUI ? 0 : talkNPC);
	}

	// Make the material slot non-interactive when ForceGuideUI by failing the hit-test.
	// The slot icon (ItemSlot.Draw) still renders; only OverrideHover/LeftClick/
	// RightClick/MouseHover inside the if-block are skipped.
	private static void PatchSkipMaterialSlot(ILContext il)
	{
		var c = new ILCursor(il);

		// Find the DrawGuideCraftText call
		c.GotoNext(MoveType.After, i =>
			i.OpCode == OpCodes.Call
			&& i.Operand is MethodReference mr
			&& mr.Name == "DrawGuideCraftText");

		// Find the next mouseX load — start of: if (mouseX >= inventoryX && ...)
		c.GotoNext(MoveType.After, i =>
			i.MatchLdsfld<Main>(nameof(Main.mouseX)));

		// Replace mouseX with int.MinValue when ForceGuideUI so the condition fails
		c.EmitDelegate<Func<int, int>>(mx => ForceGuideUI ? int.MinValue : mx);
	}

	// Suppress cursorOverride = 9 on inventory materials when ForceGuideUI.
	// Two occurrences of: else if (context == 0 && Main.InGuideCraftMenu)
	private static void PatchOverrideHover(ILContext il)
	{
		var c = new ILCursor(il);
		for (int i = 0; i < 2; i++)
		{
			c.GotoNext(MoveType.After,
				instr => instr.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
			c.EmitDelegate<Func<bool, bool>>(
				inGuide => inGuide && !ForceGuideUI);
		}
	}

	// Suppress shift-click swap into guideItem when ForceGuideUI.
	// Target: else if (Main.InGuideCraftMenu) { Utils.Swap(...) }
	private static void PatchOverrideLeftClick(ILContext il)
	{
		var c = new ILCursor(il);
		c.GotoNext(MoveType.After,
			instr => instr.MatchLdsfld<Main>(nameof(Main.InGuideCraftMenu)));
		c.EmitDelegate<Func<bool, bool>>(
			inGuide => inGuide && !ForceGuideUI);
	}
}
