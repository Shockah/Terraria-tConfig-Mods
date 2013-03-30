#INCLUDE "OSIHPBars.cs"

private static Texture2D texBack = null, texGreen = null, texRed = null;
private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

static ModWorld() {
	if (Main.dedServ) return;
	
	Color[] bgc = new Color[12];
	
	texBack = new Texture2D(Config.mainInstance.GraphicsDevice,1,bgc.Length);
	for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Silver,Color.Black,1f*i/bgc.Length/2f);
	texBack.SetData(bgc);
	
	texGreen = new Texture2D(Config.mainInstance.GraphicsDevice,1,bgc.Length);
	for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Lime,Color.Black,1f*i/bgc.Length/2f);
	texGreen.SetData(bgc);
	
	texRed = new Texture2D(Config.mainInstance.GraphicsDevice,1,bgc.Length);
	for (int i = 0; i < bgc.Length; i++) bgc[i] = Color.Lerp(Color.Red,Color.Black,1f*i/bgc.Length/2f);
	texRed.SetData(bgc);
}

public void RegisterOnScreenInterfaces() {
	OSIHPBars.Register();
}

public static float GetBarAlpha(Player player) {
	int xx = (int)(Math.Round(player.position.X/16)), yy = (int)(Math.Round(player.position.Y/16));
	
	double val = 0;
	for (int y = 0; y < 3; y++) for (int x = 0; x < 2; x++) {
		Color c = Lighting.GetColor(xx+x,yy+y);
		val += Lighting.lightMode < 2 ? .299d*c.R/255d+.587d*c.G/255d+.114d*c.B/255d : c.R/255d;
	}
	return (float)(val/6);
}

private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
}