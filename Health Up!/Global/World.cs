private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};
private static readonly Color[] heartColors = {Color.Red,Color.Orange,Color.Yellow,Color.GreenYellow,Color.Lime,Color.Cyan,Color.RoyalBlue,Color.MediumOrchid,Color.Pink};
private static readonly Color[] starColors = {Color.Cyan,Color.CornflowerBlue,Color.Blue,Color.DarkViolet,Color.Magenta,Color.HotPink,Color.LightPink};

public void Initialize() {
	Main.heartTexture = Main.goreTexture[Config.goreID["HealthUpHeart"]];
	Main.manaTexture = Main.goreTexture[Config.goreID["HealthUpStar"]];
}

public static bool PreDrawLifeHearts(SpriteBatch sb) {
	if (Settings.GetBool("customDisplay")) return true;
	
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	if (p.ghost) return false;
	
	Texture2D tex = Main.heartTexture, tex2 = Main.goreTexture[Config.goreID["HealthUpHeart2"]];
	int x = Main.screenWidth-300, y = 41;
	
	const int valueHeart = 20;
	const int heartOffset = 26, heartPerLine = 10, heartLines = 2;
	
	string text = Lang.inter[0]+" "+p.statLife+"/"+p.statLifeMax;
	
	int i = 0;
	int value = p.statLife;
	
	DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(x+((Math.Min(heartPerLine,value/20)-1)*heartOffset-Main.fontMouseText.MeasureString(text).X)/2f,6),Color.White,Color.Black,default(Vector2),1f,SpriteEffects.None);
	while (value != 0) {
		int value2 = Math.Min(valueHeart,value);
		value -= value2;
		
		int xx = i%heartPerLine, yy = i%(heartPerLine*heartLines)/heartPerLine, set = i/(heartPerLine*heartLines);
		float scale = value == 0 ? Main.cursorScale : 1f;
		
		Color c = Alpha(HealthUpGetHealthColor(set),.25f+(.75f*value2/valueHeart));
		sb.Draw(tex,new Vector2(x+xx*heartOffset,y+yy*heartOffset),new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height)),c,value == 0 ? scale*2f-2.1f : 0f,new Vector2(tex.Width/2,tex.Height/2),c.A/255f,SpriteEffects.None,0f);
		sb.Draw(tex2,new Vector2(x+xx*heartOffset,y+yy*heartOffset),new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height)),Merge(c,Color.White,.5f,true),value == 0 ? scale*2f-2.1f : 0f,new Vector2(tex.Width/2,tex.Height/2),c.A/255f,SpriteEffects.None,0f);
		i++;
	}
	
	return false;
}
public static Color HealthUpGetHealthColor(int set) {
	if (set <= heartColors.Length-1) return heartColors[set];
	return Color.White;
}

public static bool PreDrawManaStars(SpriteBatch sb) {
	if (Settings.GetBool("customDisplay")) return true;
	
	Player p = Main.player[Main.myPlayer];
	if (p == null) return false;
	if (p.ghost) return false;
	if (p.statManaMax2 <= 0) return false;
	
	const int valueStar = 20, starPerLine = 10;
	
	Texture2D tex = Main.manaTexture, tex2 = Main.goreTexture[Config.goreID["HealthUpStar2"]];
	int x = Main.screenWidth-25, y = 30+tex.Height/2;
	
	string text = Lang.inter[2];
	
	int i = 0;
	int value = p.statMana;
	
	DrawStringShadowed(sb,Main.fontMouseText,text,new Vector2(Main.screenWidth-50,6),Color.White,Color.Black,default(Vector2),1f,SpriteEffects.None);
	while (value != 0) {
		int value2 = Math.Min(valueStar,value);
		value -= value2;
		
		int yy = i%starPerLine, set = i/starPerLine;
		float scale = value == 0 ? Main.cursorScale : 1f;
		
		Color c = Alpha(HealthUpGetManaColor(set),.25f+(.75f*value2/valueStar));
		sb.Draw(tex,new Vector2(x,y+yy*26),new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height)),c,value == 0 ? scale*2f*1.11f-2.1f*1.11f : 0f,new Vector2(tex.Width/2,tex.Height/2),c.A/255f,SpriteEffects.None,0f);
		sb.Draw(tex2,new Vector2(x,y+yy*26),new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height)),Merge(c,Color.White,.5f,true),0f,new Vector2(tex.Width/2,tex.Height/2),c.A/255f,SpriteEffects.None,0f);
		i++;
	}
	
	return false;
}
public static Color HealthUpGetManaColor(int set) {
	if (set <= starColors.Length-1) return starColors[set];
	return Color.White;
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

public static int ExternalGetMaxHealth() {
	return 400*Settings.GetInt("multLife");
}
public static int ExternalGetMaxMana() {
	return 200*Settings.GetInt("multMana");
}
public static bool ExternalGetCustomBarDisplay() {
	return Settings.GetBool("customDisplay");
}