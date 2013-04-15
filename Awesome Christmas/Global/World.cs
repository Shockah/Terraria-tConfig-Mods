#INCLUDE "Util.cs"
#INCLUDE "Spark.cs"
#INCLUDE "Firework.cs"
#INCLUDE "FireworkTwoWave.cs"
#INCLUDE "FireworkCircleSprayer.cs"

public const int
	MSG_UNGHOST = 1,
	MSG_GHOST = 2,
	MSG_AURORA = 3,
	MSG_GUI_REQUEST = 4,
	MSG_GUI_CLOSE = 5,
	MSG_ICEACC = 6,
	MSG_OPENEDPRESENT = 7;

public static int modId;
public static bool wasDay = true;
public static bool flagFrostmaw = false;

public static int northernLights = 0;
public static float curAlpha = 0f;

public static List<int> spitterList = new List<int>();
public static List<ModGeneric.Pair<Vector2,int>> lockedBy = new List<ModGeneric.Pair<Vector2,int>>();
public static List<ModGeneric.Pair<Vector2,List<Item>>> customItems = new List<ModGeneric.Pair<Vector2,List<Item>>>();
public static List<ModWorld.Spark> sparks = new List<ModWorld.Spark>();
public static bool JustQuitted = false;
public static Func<string,Texture2D,bool> AcAchieve = null;
public static Action<int,string> AcAchievePlayer = null;

public void Initialize(int modId) {
	ModWorld.modId = modId;
	for (int i = 0; i < ModPlayer.ghosts.Length; i++) ModPlayer.ghosts[i] = false;
	wasDay = true;
	northernLights = 0;
	curAlpha = 0f;
	customItems.Clear();
	JustQuitted = false;
	spitterList.Clear();
	spitterList.Add(Config.tileDefs.ID["Fireworks Spitter - Two-Wave"]);
	spitterList.Add(Config.tileDefs.ID["Fireworks Spitter - Circle Sprayer"]);
	flagFrostmaw = false;
}

public static void ExternalInitAchievementsDelegates(
	Action<string,string,string,string,string,int,Texture2D,bool> AddAchievement,
	Action<string[],int> ConfigNetMode,
	Action<string[],int> ConfigDifficulty,
	Action<string[],int> ConfigHardmode,
	Action<string,object,Func<object,object,string>> ConfigProgress,
	Func<string,bool> GetAchieved,
	Func<string,Texture2D,bool> Achieve,
	Action<int,string> AchievePlayer,
	Action<string> AchieveAllPlayers,
	Func<string,object[]> GetProgress,
	Func<string,object,Texture2D,bool> SetProgress,
	Func<string,object,Texture2D,bool> Progress,
	Action<int,string,object> ProgressPlayer,
	Action<string,object> ProgressAllPlayers,
	Func<string,object[]> GetAchievementInfo)
{
	AcAchieve = Achieve;
	AcAchievePlayer = AchievePlayer;
	
	string s, cat;
	
	cat = "";
	s = "SHK_XMAS_PRESENT"; AddAchievement(s,cat,"Gifting Aura","Give someone a present.",null,10,Main.dedServ ? null : Main.itemTexture[Config.itemDefs.byName["Present"].type],true);
	
	cat = "Bosses";
	s = "SHK_XMAS_FROSTMAW"; AddAchievement(s,cat,"Arachnophobia","Defeat Frostmaw.",null,30,Main.dedServ ? null : Main.itemTexture[Config.itemDefs.byName["Spider Bait"].type],false);
	s = "SHK_XMAS_ROSEMAW"; AddAchievement(s,cat,"Arachnophobia","Defeat Rosemaw.","SHK_XMAS_FROSTMAW",40,Main.dedServ ? null : Main.itemTexture[Config.itemDefs.byName["Frozen Shard"].type],false);
}
public bool SaveAndQuit()
{
    JustQuitted = true;
    return true;
}
public void Save(BinaryWriter bw) {
	bw.Write(northernLights);
	bw.Write(flagFrostmaw);
	
	for (int i = 0; i < customItems.Count; i++) {
		ModGeneric.Pair<Vector2,List<Item>> pair = customItems[i];
		if (pair.B == null) pair.B = new List<Item>();
		if (pair.B.Count == 0) continue;
		if (!Main.tile[(int)pair.A.X,(int)pair.A.Y].active || !spitterList.Contains(Main.tile[(int)pair.A.X,(int)pair.A.Y].type)) continue;
		if (Main.tile[(int)pair.A.X,((int)pair.A.Y)+1].active && Main.tile[(int)pair.A.X,((int)pair.A.Y)+1].type == Main.tile[(int)pair.A.X,(int)pair.A.Y].type) continue;
		
		bw.Write(true);
		bw.Write((int)pair.A.X);
		bw.Write((int)pair.A.Y);
		bw.Write(pair.B.Count);
		foreach (Item item in pair.B) ModGeneric.ItemSave(bw,item);
	}
	bw.Write(false);
}
public void Load(BinaryReader br, int version) {
	northernLights = br.ReadInt32();
	if (!Main.dayTime) wasDay = false;
	flagFrostmaw = br.ReadBoolean();
	
	while (br.ReadBoolean()) {
		Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
		List<Item> list = new List<Item>();
		
		int count = br.ReadInt32();
		for (int i = 0; i < count; i++) list.Add(ModGeneric.ItemLoad(br));
		
		customItems.Add(new ModGeneric.Pair<Vector2,List<Item>>(v,list));
	}
}

#region synced interfaces
public static void Lock(Vector2 pos, Player player) {
	for (int i = 0; i < lockedBy.Count; i++) {
		ModGeneric.Pair<Vector2,int> pair = lockedBy[i];
		if ((int)pair.A.X == (int)pos.X && (int)pair.A.Y == (int)pos.Y) {
			pair.B = player.whoAmi;
			return;
		}
	}
	lockedBy.Add(new ModGeneric.Pair<Vector2,int>(pos,player.whoAmi));
}
public static void Unlock(Vector2 pos) {
	for (int i = 0; i < lockedBy.Count; i++) {
		ModGeneric.Pair<Vector2,int> pair = lockedBy[i];
		if ((int)pair.A.X == (int)pos.X && (int)pair.A.Y == (int)pos.Y) {
			lockedBy.RemoveAt(i);
			return;
		}
	}
}
public static bool IsLocked(Vector2 pos) {
	foreach (ModGeneric.Pair<Vector2,int> pair in lockedBy) {
		if ((int)pair.A.X == (int)pos.X && (int)pair.A.Y == (int)pos.Y) return true;
	}
	return false;
}
public static List<Item> GetCustomItems(Vector2 pos) {
	foreach (ModGeneric.Pair<Vector2,List<Item>> pair in customItems) {
		if ((int)pair.A.X == (int)pos.X && (int)pair.A.Y == (int)pos.Y) return pair.B;
	}
	List<Item> list = new List<Item>();
	customItems.Add(new ModGeneric.Pair<Vector2,List<Item>>(pos,list));
	return list;
}
#endregion

public void NetReceive(int messageType, BinaryReader br) {
	if (Main.netMode == 1) {
		switch (messageType) {
			case MSG_UNGHOST: {
				int playerID = (int)br.ReadByte();
				Player player = Main.player[playerID];
				if (player != null && player.active) {
					ModPlayer.ghosts[playerID] = false;
					if (playerID == Main.myPlayer) {
						player.ghost = false;
						player.Spawn();
                        Codable.RunPlayerMethod("OnSpawn",false,player,playerID);
					}
                    for(int i = 0; i < 20; i++)
                    {
                        int d =Dust.NewDust(player.position+new Vector2(-4,4),player.width+8,player.height+8,66,0,-2f,100,new Color(180,180, Main.DiscoB),2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity*=3f;
                    }
					//TODO spawn dust or some other kind of effect
				}
			} break;
			case MSG_GHOST: {
				int playerID = (int)br.ReadByte();
				Player player = Main.player[playerID];
				if (player != null && player.active) {
					player.dead = true;
					ModPlayer.ghosts[playerID] = true;
				}
			} break;
			case MSG_AURORA: {
				int aurora = br.ReadInt32();
				northernLights = aurora;
			} break;
			case MSG_GUI_REQUEST: {
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				int count = br.ReadByte();
				List<Item> items = new List<Item>();
				while (count-- > 0) items.Add(ModGeneric.ItemLoad(br));
				
				int slots = 0;
				if (Main.tile[(int)v.X,(int)v.Y].type == Config.tileDefs.ID["Fireworks Spitter - Two-Wave"]) slots = 10;
				else if (Main.tile[(int)v.X,(int)v.Y].type == Config.tileDefs.ID["Fireworks Spitter - Circle Sprayer"]) slots = 5;
				ModPlayer.GuiSpitter.Create(v,items,slots);
			} break;
			default: break;
		}
	} else if (Main.netMode == 2) {
		switch (messageType) {
			case MSG_UNGHOST: {
				int playerID = (int)br.ReadByte();
				Player player = Main.player[playerID];
				if (player != null && player.active) {
					ModPlayer.ghosts[playerID] = false;
					NetMessage.SendModData(modId,MSG_UNGHOST,-1,-1,(byte)player.whoAmi);
				}
			} break;
			case MSG_GHOST: {
				int playerID = (int)br.ReadByte();
				Player player = Main.player[playerID];
				if (player != null && player.active) {
					player.dead = true;
					ModPlayer.ghosts[playerID] = true;
					NetMessage.SendModData(modId,MSG_GHOST,-1,-1,(byte)player.whoAmi);
				}
			} break;
			case MSG_GUI_REQUEST: {
				int pID = br.ReadByte();
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				if (!IsLocked(v)) {
					MemoryStream ms = new MemoryStream();
					BinaryWriter bw = new BinaryWriter(ms);
					
					bw.Write((int)v.X);
					bw.Write((int)v.Y);
					
					List<Item> items = GetCustomItems(v);
					bw.Write((byte)items.Count);
					foreach (Item item in items) ModGeneric.ItemSave(bw,item);
					
					byte[] data = ms.ToArray();
					object[] toSend = new object[data.Length];
					for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
					NetMessage.SendModData(modId,MSG_GUI_REQUEST,pID,-1,toSend);
				}
			} break;
			case MSG_GUI_CLOSE: {
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				
				List<Item> items = GetCustomItems(v);
				items.Clear();
				int count = br.ReadByte();
				while (count-- > 0) items.Add(ModGeneric.ItemLoad(br));
				
				Unlock(v);
			} break;
			case MSG_ICEACC: {
				int pID = br.ReadByte();
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				
				Projectile proj = Main.projectile[Projectile.NewProjectile(v.X*16f+8f,v.Y*16f+8f,0f,0f,"Ice Block",0,0,pID)];
				proj.ai[0] = v.X;
				proj.ai[1] = v.Y;
				proj.timeLeft = 60*6;
			} break;
			case MSG_OPENEDPRESENT: {
				string playerName = br.ReadString();
				for (int i = 0; i < Main.player.Length; i++) {
					Player player = Main.player[i];
					if (player == null || !player.active || player.name == "") continue;
					if (player.name == playerName) AcAchievePlayer(i,"SHK_XMAS_PRESENT");
				}
			} break;
			default: break;
		}
	}
}
public void PlayerConnected(int playerID) {
	for (int i = 0; i < ModPlayer.ghosts.Length; i++) if (ModPlayer.ghosts[i]) NetMessage.SendModData(modId,MSG_GHOST,playerID,-1,i);
	if (!Main.dayTime) NetMessage.SendModData(modId,MSG_AURORA,playerID,-1,northernLights);
}

public void PostDrawTiles(SpriteBatch sb) {
	Player player = Main.player[Main.myPlayer];
	if (player.ghost) {
		if (!ModPlayer.wasGhost) {
			NetMessage.SendModData(modId,MSG_GHOST,-1,-1,(byte)player.whoAmi);
		}
		
		foreach (Player p in Main.player) {
			if (p == null || p.name == "" || p.whoAmi == Main.myPlayer) continue;
			p.ghost = ModPlayer.ghosts[p.whoAmi];
		}
	}
	ModPlayer.wasGhost = player.ghost;
}

public void UpdateWorld() {
	if (Main.netMode != 2) return;
	
	if (wasDay && !Main.dayTime) {
		northernLights = new Random().Next(60*60*60);
		NetMessage.SendModData(modId,MSG_AURORA,-1,-1,northernLights);
		wasDay = false;
	}
	if (!wasDay && Main.dayTime) wasDay = true;
	
	for (int i = 0; i < lockedBy.Count; i++) {
		ModGeneric.Pair<Vector2,int> pair = lockedBy[i];
		if (Main.player[pair.B] == null || !Main.player[pair.B].active || Main.player[pair.B].dead) lockedBy.RemoveAt(i--);
	}
}

public void PreDrawInterface(SpriteBatch sb) {
	sb.End();
	sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
	
	foreach (Spark spark in sparks) spark.Draw(sb,null,null);
	for (int i = 0; i < sparks.Count; i++) if (sparks[i].dead) sparks.RemoveAt(i--);
	
	sb.End();
	sb.Begin();
}

public void PostDrawBackground(SpriteBatch sb) {
	if (wasDay && !Main.dayTime) {
		northernLights = new Random().Next(60*60*60);
		wasDay = false;
	}
	if (!wasDay && Main.dayTime) wasDay = true;
	
	Texture2D tex = Main.goreTexture[Config.goreID["NorthernLights"]];
	northernLights++;
	
	float alphaBase = Math.Min(Main.snowTiles,500)/500f;
	if (Main.dayTime) alphaBase = 0f;
	if (Main.bloodMoon) alphaBase = 0f;
	else {
		int t = (int)Math.Abs((32400/2)-Main.time);
		alphaBase *= Math.Min(1f-(1f*t/(32400/2)),1f);
	}
	
	if (alphaBase != curAlpha) {
		if (alphaBase > curAlpha) {
			curAlpha += .01f;
			if (alphaBase < curAlpha) curAlpha = alphaBase;
		} else {
			curAlpha -= .01f;
			if (alphaBase > curAlpha) curAlpha = alphaBase;
		}
	}
	
	if (curAlpha <= 0f) return;
	
	sb.End();
	sb.Begin(SpriteSortMode.Immediate,BlendState.NonPremultiplied);
	
	const int lights = 8;
	float[] posX = new float[8], posY = new float[8], hue = new float[8], alpha = new float[8], scale = new float[8];
	
	Rectangle? rect = new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height));
	Vector2 origin = new Vector2(tex.Width/2f,tex.Height/2f);
	for (int i = 0; i < lights; i++) {
		float m = (i+1)/50f;
		float mult = (float)(Math.Pow(1.5d,i+1)*5);
		posX[i] = (float)(Math.Sin(((northernLights*m)%360)*Math.PI/180d)*mult);
		posY[i] = (float)(Math.Sin(((northernLights*m*1.3d)%360)*Math.PI/180d)*mult/2);
		hue[i] = (float)(i == 0 ? (northernLights*m*1.55d)%360 : (hue[i-1]+Math.Cos(((northernLights*m*1.22d)%360)*Math.PI/180d)*30)%360);
		alpha[i] = (float)(1f-(Math.Pow(1f*i/lights,2)));
		scale[i] = (float)((1d*i+Math.Cos(((northernLights*1.69d*m)%360)*Math.PI/180)*(1d*i*.25d))/2d/1920d*Main.screenWidth);
		
		int r, g, b;
		HsvToRgb(hue[i],1f,1f,out r,out g,out b);
		Color c = new Color(r,g,b,(byte)Math.Min(Math.Max(alpha[i]*curAlpha*255,0),255));
		sb.Draw(tex,new Vector2(Main.screenWidth/2+posX[i],Main.screenHeight/6+posY[i]),rect,c,0f,origin,scale[i],SpriteEffects.None,0f);
	}
	
	sb.End();
	sb.Begin();
}

#region HsvToRgb
public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b) {
	double H = h;
	while (H < 0) { H += 360; };
	while (H >= 360) { H -= 360; };
	double R, G, B;
	
	if (V <= 0) { R = G = B = 0; }
	else if (S <= 0) { R = G = B = V; }
	else {
		double hf = H / 60.0;
		int i = (int)Math.Floor(hf);
		double f = hf - i;
		double pv = V * (1 - S);
		double qv = V * (1 - S * f);
		double tv = V * (1 - S * (1 - f));
		switch (i) {
			case 0:
				R = V;
				G = tv;
				B = pv;
			break;
			case 1:
				R = qv;
				G = V;
				B = pv;
			break;
			case 2:
				R = pv;
				G = V;
				B = tv;
			break;
			case 3:
				R = pv;
				G = qv;
				B = V;
			break;
			case 4:
				R = tv;
				G = pv;
				B = V;
			break;
			case 5:
				R = V;
				G = pv;
				B = qv;
			break;
			case 6:
				R = V;
				G = tv;
				B = pv;
			break;
			case -1:
				R = V;
				G = pv;
				B = qv;
			break;
			default:
				R = G = B = V;
			break;
		}
	}
	r = Clamp((int)(R * 255.0));
	g = Clamp((int)(G * 255.0));
	b = Clamp((int)(B * 255.0));
}

public static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v)
{
    float max = Math.Max(Math.Max(r,g),b);
	float min = Math.Min(Math.Min(r,g),b);
	float C = max-min;
	
	v = max;
	s = C == 0 || v == 0 ? 0 : 1f*C/v;
	h = 0;
	
	if (max == r) {
		h = ((g-b)*1f/C)%6;
	} else if (max == g) {
		h = ((b-r)*1f/C)+2;
	} else if (max == b) {
		h = ((r-g)*1f/C)+4;
	}
	h *= 60;
}

private static int Clamp(int i) {
	if (i < 0) return 0;
	if (i > 255) return 255;
	return i;
}
#endregion

public void PreLightTiles(SpriteBatch SP)
{
	float alphaBase = Math.Min(Main.snowTiles,500)/500f;
	if (Main.dayTime) alphaBase = 0;
	else {
		int t = (int)Math.Abs((32400/2)-Main.time);
		alphaBase *= Math.Min(1f-(1f*t/(32400/2)),1f);
	}
	if (alphaBase <= 0f) return;

    float self = (float)(northernLights*0.02f*1.55d)%360;
    Vector3 cols = new Vector3();
    for(int i = 0; i < 8; i++)
    {
		float m = (i+1)/50f;
        self = (float)((self+Math.Cos(((northernLights*m*1.22d)%360)*Math.PI/180d)*30)%360);
        int r, g, b;
        HsvToRgb(self,1f,1f,out r,out g,out b);
        cols+=new Vector3(r,g,b);
    }
    cols/=8f;
    Color c = new Color((int)(cols.X*alphaBase),(int)(cols.Y*alphaBase),(int)(cols.Z*alphaBase));
    Main.bgColor = Color.Lerp(Main.bgColor,BrightestColor(c,Main.bgColor),0.5f);
    Main.tileColor = Color.Lerp(Main.tileColor,BrightestColor(c,Main.tileColor),0.5f);
}

public Color BrightestColor(Color c1,Color c2)
{
  return new Color(Math.Max(c1.R,c2.R),Math.Max(c1.G,c2.G),Math.Max(c1.B,c2.B));
}

public void ExternalGetBossPhase(List<NPC> parts, string name, Vector2 centerPos, int life, int lifeMax, Object[] ret) {
	switch (name) {
		case "Frostmaw": {
			ret[0] = life <= lifeMax*.75f ? (life <= lifeMax/2 ? 0 : 1) : 2;
			switch ((int)ret[0]) {
				case 0: ret[1] = 1f*life/(lifeMax/2); break;
				case 1: ret[1] = 1f*(life-(lifeMax/2))/(lifeMax/4); break;
				case 2: ret[1] = 1f*(life-(lifeMax*.75f))/(lifeMax/4); break;
			}
		} break;
		case "Rosemaw": {
			ret[0] = life <= lifeMax*.75f ? 0 : 1;
			switch ((int)ret[0]) {
				case 0: ret[1] = 1f*life/(lifeMax*.75f); break;
				case 1: ret[1] = 1f*(life-(lifeMax*.75f))/(lifeMax/4); break;
			}
		} break;
	}
}