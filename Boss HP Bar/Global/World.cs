private static Texture2D texBack = null, texGreen = null, texRed = null;
private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

private static Dictionary<string,int> checkedMaxHP = new Dictionary<string,int>();

public static List<string> preset = new List<String>();
public static string[][] bossWormNames = {new string[]{"Eater of Worlds Head","Eater of Worlds Body","Eater of Worlds Tail"},new string[]{"The Destroyer","The Destroyer Body","The Destroyer Tail"}};
public static bool[] bossWormTotal = {true,false};

static ModWorld() {
	preset.Add("Eater of Worlds Head");
	preset.Add("Eye of Cthulhu");
	preset.Add("Skeletron"); preset.Add("Skeletron Hand");
	preset.Add("Wall of Flesh");
	preset.Add("The Destroyer");
	preset.Add("Skeletron Prime"); preset.Add("Prime Cannon"); preset.Add("Prime Saw"); preset.Add("Prime Vice"); preset.Add("Prime Laser");
	preset.Add("Retinazer");
	preset.Add("Spazmatism");
	
	if (Main.dedServ) return;
	
	Color[] bgc = new Color[20];
	
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

public static void PreDrawInterface(SpriteBatch sb) {
	int xx = 200, yy = Main.screenHeight-32, ww = Main.screenWidth-2*xx, hh = 32;
	
	List<int> ignore = new List<int>();
	List<string> names = new List<string>();
	foreach (NPC npc in Main.npc) {
		if (npc == null) continue;
		if (!npc.active) continue;
		if (npc.name == null || npc.name == "") continue;
		
		if (ignore.Contains(npc.whoAmI)) continue;
		if (!IsBoss(npc)) continue;
		
		int hp = GetBossHP(npc,ignore), hpMax = GetBossMaxHP(npc);
		if (hp <= 0) continue;
		if (hp > hpMax) hp = hpMax;
		
		string name = GetBossName(npc);
		sb.Draw(texBack,new Rectangle(xx,yy+4,ww,hh-8),Color.White);
		sb.Draw(texRed,new Rectangle(xx+2,yy+6,ww-4,hh-12),Color.White);
		sb.Draw(texGreen,new Rectangle(xx+2,yy+6,(int)((1d*hp/hpMax)*(ww-4)),hh-12),Color.White);
		DrawStringShadowed(sb,Main.fontMouseText,name,new Vector2(xx+4,yy+4),Color.White,Color.Black);
		DrawStringShadowed(sb,Main.fontMouseText,""+hp+"/"+hpMax,new Vector2(xx+ww-4-Main.fontMouseText.MeasureString(""+hp+"/"+hpMax).X,yy+4),Color.White,Color.Black);
		
		names.Add(name);
		yy -= hh;
	}
	
	List<string> toRemove = new List<string>();
	foreach (KeyValuePair<string,int> pair in checkedMaxHP) if (!names.Contains(pair.Key)) toRemove.Add(pair.Key);
	foreach (string name in toRemove) checkedMaxHP.Remove(name);
}
private static bool IsBoss(NPC npc) {
	if (npc.boss) return true;
	return preset.Contains(npc.name);
}
private static int GetBossHP(NPC npc, List<int> ignore) {
	for (int i = 0; i < bossWormNames.Length; i++) if (bossWormTotal[i]) for (int j = 0; j < bossWormNames[i].Length; j++) if (npc.name == bossWormNames[i][j]) return CountWormHP(bossWormNames[i],ignore);
	ignore.Add(npc.whoAmI);
	return npc.life;
}
private static int GetBossMaxHP(NPC npc) {
	for (int i = 0; i < bossWormNames[0].Length; i++) if (npc.name == bossWormNames[0][i]) {
		string name = GetBossName(npc);
		if (!checkedMaxHP.ContainsKey(name)) {
			int hp = 0;
			foreach (NPC npc2 in Main.npc) if (npc2 != null) for (int j = 0; j < bossWormNames[0].Length; j++) if (bossWormNames[0][j] == npc2.name) hp += npc2.lifeMax;
			checkedMaxHP[name] = hp;
		}
		return checkedMaxHP[name];
	}
	return npc.lifeMax;
}
private static string GetBossName(NPC npc) {
	for (int i = 0; i < bossWormNames.Length; i++) for (int j = 0; j < bossWormNames[i].Length; j++) if (npc.name == bossWormNames[i][j]) return npc.displayName;
	return npc.name;
}
private static int CountWormHP(string[] names, List<int> ignore) {
	int hp = 0;
	foreach (NPC npc in Main.npc) if (npc != null) {
		for (int i = 0; i < names.Length; i++) if (names[i] == npc.name) {
			if (npc.life > 0) hp += npc.life;
			ignore.Add(npc.whoAmI);
		}
	}
	return hp;
}

private static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
}