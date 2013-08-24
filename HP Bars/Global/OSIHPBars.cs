public class OSIHPBars : OnScreenInterfaceable {
	public static void Register() {
		OnScreenInterface.Register(new OSIHPBars(),OnScreenInterface.LAYER_INTERFACE_WORLD);
	}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		if (ModWorld.display == "off") return;
		if (Config.tileInterface != null) return;
		if (Config.npcInterface != null) return;
		
		sb.End();
		sb.Begin(SpriteSortMode.Immediate,BlendState.NonPremultiplied);
		
		foreach (Player player in Main.player) {
			if (player == null) continue;
			if (!player.active) continue;
			if (player.dead || player.ghost) continue;
			
			if (ModWorld.display != "all") {
				if (player.whoAmi == Main.myPlayer && ModWorld.display != "my") continue;
				if (player.whoAmi != Main.myPlayer && ModWorld.display != "other") continue;
			}
			
			if (player.invis) continue;
			float alpha = GetBarAlpha(player);
			if (alpha <= 0) continue;
			
			int hp = player.statLife, hpMax = player.statLifeMax;
			int xx = (int)(player.position.X-Main.screenPosition.X)-12, yy = (int)(player.position.Y-16-Main.screenPosition.Y), ww = 44, hh = 12;
			
			Color color = new Color(255,255,255);
			color.A = (byte)(alpha*255);
			
			sb.Draw(texBack,new Rectangle(xx,yy,ww,hh),color);
			sb.Draw(texRed,new Rectangle(xx+2,yy+2,ww-4,hh-4),color);
			sb.Draw(texGreen,new Rectangle(xx+2,yy+2,(int)((1d*hp/hpMax)*(ww-4)),hh-4),color);
		}
		
		sb.End();
		sb.Begin();
	}
}