private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
private static Texture2D texBar, texBarBorder;

public static void Initialize() {
	texBar = Main.goreTexture[Config.goreID["EnergyBar"]];
	texBarBorder = Main.goreTexture[Config.goreID["EnergyBarBorder"]];
}

public static void PreDrawInterface(SpriteBatch sb) {
	if (ModPlayer.energyMax <= 0) return;
	int x = Main.screenWidth-300, y = 82;
	
	string text = "Energy: "+ModPlayer.energy+"/"+ModPlayer.energyMax;
	DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+260/2-Main.fontMouseText.MeasureString(text).X/2,y),Color.White,Color.Black,default(Vector2),1f,SpriteEffects.None);
	
	y += 24;
	sb.Draw(texBarBorder,new Vector2(x,y),new Rectangle(0,0,texBarBorder.Width,texBarBorder.Height),Color.White);
	
	int times = (int)Math.Ceiling(texBar.Height/2f);
	float energyPercent = ModPlayer.energyMax == 0 ? 0f : 1f*ModPlayer.energy/ModPlayer.energyMax;
	int w = (int)(Math.Floor(texBar.Width/2f*energyPercent)*2);
	for (int i = 0; i < times; i++) {
		int ww = w-(i/2)*2;
		if (ww > 0) sb.Draw(texBar,new Vector2(x+4,y+4+i*2),new Rectangle(0,i*2,ww,2),Color.White);
	}
}

public static Color Alpha(Color color, float alpha) {
	Color ret = new Color();
	ret.R = color.R;
	ret.G = color.G;
	ret.B = color.B;
	ret.A = (byte)(color.A*alpha);
	return ret;
}
public static Color Merge(Color c1, Color c2, float value, bool noAlpha) {
	value = Math.Min(Math.Max(value,0),1);
	float R = c1.R/255f-((c1.R/255f-c2.R/255f)*value);
	float G = c1.G/255f-((c1.G/255f-c2.G/255f)*value);
	float B = c1.B/255f-((c1.B/255f-c2.B/255f)*value);
	float A = noAlpha ? c1.A/255f : c1.A/255f-((c1.A/255f-c2.A/255f)*value);
	Color ret = new Color();
	ret.R = (byte)(R*255);
	ret.G = (byte)(G*255);
	ret.B = (byte)(B*255);
	ret.A = (byte)(A*255);
	return ret;
}

private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 vec, float scale, SpriteEffects effects) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,vec,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,vec,scale,effects,0f);
}