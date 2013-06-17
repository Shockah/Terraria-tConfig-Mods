public class OSICheat : OnScreenInterfaceable {
	public static void Register() {
		OnScreenInterface.Register(new OSICheat(),OnScreenInterface.LAYER_INTERFACE_SCREEN);
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		Player player = Main.player[Main.myPlayer];
		
		if (resetChat) Main.chatMode = false;
		resetChat = false;
		
		if (Config.tileInterface != null && Config.tileInterface.code is GuiCheat) {
			Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
			((GuiCheat)Config.tileInterface.code).PreDrawInterface(sb);
		}
		
		if (Main.playerInventory) {
			Color c;
			Vector2 v;
			int xx = 0;
			
			c = Config.tileInterface != null && Config.tileInterface.code is GuiItem ? Color.White : Color.Gray;
			v = new Vector2(8+xx,Main.screenHeight-8-texItem.Height);
			sb.Draw(texItem,v,GetTexRectangle(texItem),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			xx += texItem.Width+2;
			
			c = Config.tileInterface != null && Config.tileInterface.code is GuiPrefix ? Color.White : Color.Gray;
			v = new Vector2(8+xx,Main.screenHeight-8-texPrefix.Height);
			sb.Draw(texPrefix,v,GetTexRectangle(texPrefix),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			xx += texPrefix.Width+2;
			
			c = Config.tileInterface != null && Config.tileInterface.code is GuiNPC ? Color.White : Color.Gray;
			v = new Vector2(8+xx,Main.screenHeight-8-texNPC.Height);
			sb.Draw(texNPC,v,GetTexRectangle(texNPC),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			xx += texNPC.Width+2;
			
			c = Config.tileInterface != null && Config.tileInterface.code is GuiBuff ? Color.White : Color.Gray;
			v = new Vector2(8+xx,Main.screenHeight-8-texBuff.Height);
			sb.Draw(texBuff,v,GetTexRectangle(texBuff),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			xx += texBuff.Width+2;
			
			c = Config.tileInterface != null && Config.tileInterface.code is GuiMisc ? Color.White : Color.Gray;
			v = new Vector2(8+xx,Main.screenHeight-8-texMisc.Height);
			sb.Draw(texMisc,v,GetTexRectangle(texMisc),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
			xx += texMisc.Width+2;
		}
	}
}