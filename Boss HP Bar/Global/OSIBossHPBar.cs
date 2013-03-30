public class OSIBossHPBar : OnScreenInterfaceable {
	private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
	
	public static void Register() {
		OnScreenInterface.Register(new OSIBossHPBar(),OnScreenInterface.LAYER_INTERFACE_SCREEN);
	}
	
	public static void PostUpdate() {}
	
	public void DrawOnScreen(SpriteBatch sb, double layer) {
		int xx = 200, yy = Main.screenHeight-32, ww = Main.screenWidth-2*xx, hh = 32;
		
		List<ModWorld.NPCInst> list = ModWorld.GetActualNPCs();
		foreach (ModWorld.NPCInst npci in list) {
			if (!ModWorld.IsBoss(npci)) continue;
			
			Player p = Main.player[Main.myPlayer];
			if (Vector2.Distance(p.position+new Vector2(p.width/2f,p.height/2f),npci.GetCenterPos()) > 2*Math.Sqrt(Math.Pow(Main.screenWidth,2)+Math.Pow(Main.screenHeight,2))) continue;
			
			sb.Draw(ModWorld.texBack,new Rectangle(xx,yy+4,ww,hh-8),Color.White);
			sb.Draw(ModWorld.texWhite,new Rectangle(xx+2,yy+6,ww-4,hh-12),Color.Black);
			
			int phase = 0;
			float percent = 1f*npci.GetLife()/npci.GetLifeMax();
			Object[] ret = new Object[]{null,null};
			if (Codable.RunGlobalMethod("ModWorld","ExternalGetBossPhase",npci.parts,npci.GetName(),npci.GetCenterPos(),npci.GetLife(),npci.GetLifeMax(),ret)) if (ret[0] != null && ret[1] != null) {
				phase = (int)ret[0];
				percent = (float)ret[1];
			}
			if (phase > 0) sb.Draw(ModWorld.texPhase[phase-1],new Rectangle(xx+2,yy+6,(int)(1f*(ww-4)),hh-12),Color.White);
			sb.Draw(ModWorld.texPhase[phase],new Rectangle(xx+2,yy+6,(int)((percent)*(ww-4)),hh-12),Color.White);
			
			DrawStringShadowed(sb,Main.fontMouseText,npci.GetName(),new Vector2(xx+4,yy+4),Color.White,Color.Black);
			string s = ""+npci.GetLife()+"/"+npci.GetLifeMax();
			DrawStringShadowed(sb,Main.fontMouseText,s,new Vector2(xx+ww-4-Main.fontMouseText.MeasureString(s).X,yy+4),Color.White,Color.Black);
			
			yy -= hh;
		}
	}
	
	private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
		foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
		sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
	}
}