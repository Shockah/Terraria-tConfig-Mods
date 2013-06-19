public class OSICrafting : OnScreenInterfaceable {
	public static void Register() {
		OnScreenInterface.Register(new OSICrafting(),OnScreenInterface.LAYER_INTERFACE_SCREEN);
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		Player player = Main.player[Main.myPlayer];
		
		if (resetChat) Main.chatMode = false;
		resetChat = false;
		
		if (ModGeneric.instaShow && Main.playerInventory && !Main.craftingHide && (!Codable.RunGlobalMethod("ModWorld","PreDrawAvailableRecipes",sb) || (bool)Codable.customMethodReturn)) {
			if (Main.reforge || player.chest != -1 || Main.npcShop != 0 || player.talkNPC != -1 || Main.craftGuide || Main.ForceGuideMenu) return;
			if (Config.tileInterface == null && Config.npcInterface == null) {
				if (shouldInit) {
					GuiCraft.Init();
					shouldInit = false;
				}
				GuiCraft.Create();
			}
		}
		
		if (Config.tileInterface != null && Config.tileInterface.code is GuiCraft) {
			Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
			((GuiCraft)Config.tileInterface.code).PreDrawInterface(sb);
		}
		
		if (Main.playerInventory && !ModGeneric.instaShow && (Config.tileInterface == null || Config.tileInterface is GuiCraft)) {
			Color c = Config.tileInterface != null && Config.tileInterface.code is ModWorld.GuiCraft ? Color.White : Color.Gray;
			sb.Draw(ModWorld.texShow,new Vector2(414,210),GetTexRectangle(texShow),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		}
	}
}