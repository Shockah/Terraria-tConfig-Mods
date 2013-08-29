#INCLUDE "NetworkHelper.cs"
#INCLUDE "OSIEffects.cs"
#INCLUDE "Effect.cs"
#INCLUDE "EffectPositionable.cs"
#INCLUDE "EffectFirefly.cs"
#INCLUDE "EffectFireflyHover.cs"
#INCLUDE "EffectFireflyStandard.cs"
#INCLUDE "EffectFireflyRainbow.cs"
#INCLUDE "EffectFireflyFlame.cs"
#INCLUDE "EffectFireflyTargets.cs"
#INCLUDE "EffectFireflyMoon.cs"
#INCLUDE "EffectFireflyCorrupt.cs"
#INCLUDE "ImplItemSlotRender.cs"
#INCLUDE "TileFireflyHandler.cs"

public static List<Effect>
	effects = new List<Effect>(),
	effectsAdd = new List<Effect>(),
	effectsRemove = new List<Effect>(),
	effectsExtraUpdate = new List<Effect>();
public static Random rand = new Random();

public static TileFireflyHandler fireflies = new TileFireflyHandler();
public static int tileIdFireflyJar, tileIdFireflyBottle;

public static bool isChecked;
public static List<Vector2>
	listJungleTargets = new List<Vector2>(),
	listCorruptTargets = new List<Vector2>(),
	listCheck = new List<Vector2>();

public static Func<string,Texture2D,bool> AcAchieve;

public static void Init() {
	fireflies = new TileFireflyHandler();
	isChecked = false;
	
	effects.Clear();
	effectsAdd.Clear();
	effectsRemove.Clear();
	effectsExtraUpdate.Clear();
	
	listCheck.Clear();
	listJungleTargets.Clear();
	listCorruptTargets.Clear();
	
	tileIdFireflyJar = Config.tileDefs.ID["Firefly in a Jar"];
	tileIdFireflyBottle = Config.tileDefs.ID["Firefly in a Bottle"];
	fireflies.Clear();
}
public void Initialize(int modId) {
	NetworkHelper.modId = modId;
	Init();
}
public void OnSetup() {
	OSIEffects.Register();
	ItemSlotRender.Register(new ImplItemSlotRender());
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
	
	string s, cat;
	
	cat = "Fireflies";
	s = "SHK_WORLDP_FLY_STANDARD"; AddAchievement(s,cat,"Firefly","Catch a Firefly.",null,10,RetrieveItemTex("Firefly in a Jar"),false);
	s = "SHK_WORLDP_FLY_CORRUPT"; AddAchievement(s,cat,"Corrupt Firefly","Catch a Corrupt Firefly.",null,10,RetrieveItemTex("Firefly in a Jar"),false);
	s = "SHK_WORLDP_FLY_CORRUPT2"; AddAchievement(s,cat,"Chaos Firefly","Catch a Chaos Firefly.","SHK_WORLDP_FLY_CORRUPT",15,RetrieveItemTex("Firefly in a Jar"),true);
	s = "SHK_WORLDP_FLY_MOON"; AddAchievement(s,cat,"Moon Firefly","Catch a Moon Firefly.",null,10,RetrieveItemTex("Firefly in a Jar"),false);
	s = "SHK_WORLDP_FLY_MOON2"; AddAchievement(s,cat,"Lively Moon Firefly","Catch a Lively Moon Firefly.","SHK_WORLDP_FLY_MOON",15,RetrieveItemTex("Firefly in a Jar"),true);
	s = "SHK_WORLDP_FLY_FLAME"; AddAchievement(s,cat,"Flame Firefly","Catch a Flame Firefly.",null,20,RetrieveItemTex("Firefly in a Jar"),false);
	s = "SHK_WORLDP_FLY_FLAME2"; AddAchievement(s,cat,"Cursed Flame Firefly","Catch a Cursed Flame Firefly.","SHK_WORLDP_FLY_FLAME",25,RetrieveItemTex("Firefly in a Jar"),true);
	s = "SHK_WORLDP_FLY_HOLY"; AddAchievement(s,cat,"Holy Firefly","Catch a Holy Firefly.",null,30,RetrieveItemTex("Firefly in a Jar"),false);
	s = "SHK_WORLDP_FLY_HOLY2"; AddAchievement(s,cat,"Rainbow Firefly","Catch a Rainbow Firefly.","SHK_WORLDP_FLY_HOLY",35,RetrieveItemTex("Firefly in a Jar"),true);
	
	ConfigHardmode(new string[]{"SHK_WORLDP_FLY_HOLY","SHK_WORLDP_FLY_RAINBOW"},2);
}
public static Texture2D RetrieveItemTex(string itemName) {
	return Main.dedServ ? null : Main.itemTexture[Config.itemDefs.byName[itemName].type];
}

public static List<Effect> GetAllOfType(Type type) {
	List<Effect> ret = new List<Effect>();
	foreach (Effect e in effects) if (type.IsAssignableFrom(e.GetType())) ret.Add(e);
	return ret;
}
public static List<Effect> GetAllOfExactType(Type type) {
	List<Effect> ret = new List<Effect>();
	foreach (Effect e in effects) if (type == e.GetType()) ret.Add(e);
	return ret;
}

public void PreDrawTilesEachTick(SpriteBatch sb) {
	sb.End();
	sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
	
	fireflies.DrawAll(sb);
	
	sb.End();
	sb.Begin();
}

public void PostUpdate() {
	if (!isChecked) {
		if (Main.netMode != 1) {
			for (int y = 0; y < Main.tile.GetLength(1); y++) for (int x = 0; x < Main.tile.GetLength(0); x++) {
				Tile tile = Main.tile[x,y];
				if (tile == null) continue;
				if (tile.active) {
					if (tile.type == 61 && tile.frameX == 144) listJungleTargets.Add(new Vector2(x,y));
					else if (tile.type == 26 && tile.frameX == 0 && tile.frameY == 0) listCorruptTargets.Add(new Vector2(x,y));
					else if (tile.type == 31 && tile.frameX == 0 && tile.frameY == 0) listCorruptTargets.Add(new Vector2(x,y));
				}
			}
		}
		isChecked = true;
	}
	foreach (Vector2 v in listCheck) {
		int x = (int)v.X, y = (int)v.Y;
		Tile tileUp = Main.tile[x,y-1];
		if (tileUp.active && tileUp.type == 61 && tileUp.frameX == 144) {
			listJungleTargets.Add(v);
			if (Main.netMode == 2) {
				using (MemoryStream ms = new MemoryStream())
				using (BinaryWriter bw = new BinaryWriter(ms)) {
					bw.Write(true);
					NetworkHelper.Write(bw,v);
					NetworkHelper.Send(NetworkHelper.JUNGLETARGET,ms);
				}
			}
		}
	}
	listCheck.Clear();
	
	List<Player> players = new List<Player>();
	foreach (Player player in Main.player) if (player.active && !player.ghost && player.statLife > 0) players.Add(player);
	
	foreach (Effect e in effectsRemove) effects.Remove(e);
	effectsRemove.Clear();
	foreach (Effect e in effectsAdd) {
		for (int i = 0; i < effects.Count; i++) {
			if (effects[i].depth < e.depth) {
				effects.Insert(i,e);
				goto L;
			}
		}
		effects.Add(e);
		L: {}
	}
	effectsAdd.Clear();
	if (!Main.gamePaused) {
		foreach (Effect e in effects) e.Update(players);
		fireflies.UpdateAll();
		foreach (Effect e in effectsExtraUpdate) e.Update(players);
		effectsExtraUpdate.Clear();
	}
}

public void UpdateWorld() {
	List<Player> players = new List<Player>();
	foreach (Player player in Main.player) if (player.active && !player.ghost && player.statLife > 0) players.Add(player);
	if (players.Count == 0) return;
	Player playerRand = players[rand.Next(players.Count)];
	
	const int screenW = 1920, screenH = 1080;
	List<Effect>
		listFlies = GetAllOfExactType(typeof(EffectFireflyStandard)),
		listRainbowflies = GetAllOfExactType(typeof(EffectFireflyRainbow)),
		listHellflies = GetAllOfExactType(typeof(EffectFireflyFlame)),
		listJungleflies = GetAllOfExactType(typeof(EffectFireflyMoon)),
		listCorruptflies = GetAllOfExactType(typeof(EffectFireflyCorrupt));
	
	foreach (Player player in players) {
		if (player.zone["Overworld"] && !player.zone["Hallow"] && !player.zone["Corruption"] && !player.zone["Jungle"] && !Main.dayTime && listFlies.Count < EffectFireflyStandard.GetMaxCount(players) && Main.rand.Next(200) == 0) {
			EffectFirefly ef = new EffectFireflyStandard(new Random(rand.Next()));
			for (int tries = 0; tries < 50; tries++) {
				Vector2 v = new Vector2(player.position.X-screenW/2+rand.Next(screenW),0);
				if (!ef.IsTileSolid((int)(v.X/16),(int)(v.Y/16))) {
					ef.Create(v);
					ef.Spawn();
					break;
				}
			}
		}
		
		if (player.zone["Hallow"] && !Main.dayTime && listRainbowflies.Count < EffectFireflyRainbow.GetMaxCount(players) && Main.rand.Next(100) == 0) {
			EffectFirefly ef = new EffectFireflyRainbow(new Random(rand.Next()));
			for (int tries = 0; tries < 50; tries++) {
				Vector2 v = new Vector2(player.position.X-screenW/2+rand.Next(screenW),0);
				if (!ef.IsTileSolid((int)(v.X/16),(int)(v.Y/16))) {
					ef.Create(v);
					ef.Spawn();
					break;
				}
			}
		}
		
		if (player.zone["Hell"] && listHellflies.Count < EffectFireflyFlame.GetMaxCount(players) && Main.rand.Next(200) == 0) {
			EffectFirefly ef = new EffectFireflyFlame(new Random(rand.Next()));
			for (int tries = 0; tries < 50; tries++) {
				Vector2 v = new Vector2(player.position.X-screenW/2+rand.Next(screenW),player.position.Y-screenH/2+rand.Next(screenH));
				if (!ef.IsTileSolid((int)(v.X/16),(int)(v.Y/16))) {
					ef.Create(v);
					ef.Spawn();
					break;
				}
			}
		}
		
		if (player.zone["Jungle"] && player.zone["RockLayer"] && listJungleflies.Count < EffectFireflyMoon.GetMaxCount(players) && Main.rand.Next(300) == 0) {
			EffectFirefly ef = new EffectFireflyMoon(new Random(rand.Next()));
			for (int tries = 0; tries < 50; tries++) {
				Vector2 v = new Vector2(player.position.X-screenW/2+rand.Next(screenW),player.position.Y-screenH/2+rand.Next(screenH));
				if (!ef.IsTileSolid((int)(v.X/16),(int)(v.Y/16))) {
					ef.Create(v);
					ef.Spawn();
					break;
				}
			}
		}
		
		if (player.zone["Corruption"] && (player.zone["DirtLayer"] || player.zone["RockLayer"]) && listCorruptflies.Count < EffectFireflyCorrupt.GetMaxCount(players) && Main.rand.Next(250) == 0) {
			EffectFirefly ef = new EffectFireflyCorrupt(new Random(rand.Next()));
			for (int tries = 0; tries < 50; tries++) {
				Vector2 v = new Vector2(player.position.X-screenW/2+rand.Next(screenW),player.position.Y-screenH/2+rand.Next(screenH));
				if (!ef.IsTileSolid((int)(v.X/16),(int)(v.Y/16))) {
					ef.Create(v);
					ef.Spawn();
					break;
				}
			}
		}
	}
}

public bool hardUpdateWorld(int x, int y) {
	if (Main.netMode != 1) listCheck.Add(new Vector2(x,y));
	return true;
}