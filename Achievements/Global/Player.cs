#INCLUDE "Achievement.cs"

public const int
	BOSS_EOC = 0,
	BOSS_EOW = 1,
	BOSS_SKELETRON = 2,
	BOSS_KINGSLIME = 3,
	BOSS_WOF = 4,
	BOSS_TWINS = 5,
	BOSS_DESTROYER = 6,
	BOSS_PRIME = 7;

public static bool[] bossState;
public static Vector2 lastPos;

public static List<int> sandBlocks = new List<int>(new int[]{53,112,116,123});

public static List<Achievement> achievements = new List<Achievement>();
public static Microsoft.Xna.Framework.Input.KeyboardState keyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

public static void Initialize() {
	achievements.Clear();
	ModWorld.notifiers.Clear();
	
	bossState = new bool[]{false,false,false,false,false,false,false,false};
	lastPos = new Vector2(-1,-1);
	ModWorld.oldSpawnMeteor = false;
	ModWorld.oldBloodMoon = false;
	ModWorld.oldInvasionSize = 0;
	ModWorld.oldInvasionType = 0;
	
	if (Main.dedServ) {
		Codable.RunGlobalMethod("ModWorld","ExternalInitAchievements",new object[]{});
		Codable.RunGlobalMethod("ModWorld","ExternalInitAchievementsDelegates",new object[]{
			(Action<string,string,string,string,string,int,Texture2D,bool>)ExternalAddAchievement,
			(Action<string[],int>)ExternalConfigAchievementNetMode,
			(Action<string[],int>)ExternalConfigAchievementDifficulty,
			(Action<string[],int>)ExternalConfigAchievementHardmode,
			(Action<string,object,Func<object,object,string>>)ExternalConfigAchievementProgress,
			(Func<string,bool>)ExternalGetAchieved,
			(Func<string,Texture2D,bool>)ExternalAchieve,
			(Action<int,string>)ExternalAchievePlayer,
			(Action<string>)ExternalAchieveAllPlayers,
			(Func<string,object[]>)ExternalGetAchievementProgress,
			(Func<string,object,Texture2D,bool>)ExternalSetAchievementProgress,
			(Func<string,object,Texture2D,bool>)ExternalAchievementProgress,
			(Action<int,string,object>)ExternalAchievementProgressPlayer,
			(Action<string,object>)ExternalAchievementProgressAllPlayers,
			(Func<string,object[]>)ExternalGetAchievementInfo
		});
	} else {
		Codable.RunGlobalMethod("ModPlayer","ExternalInitAchievements",new object[]{});
		Codable.RunGlobalMethod("ModPlayer","ExternalInitAchievementsDelegates",new object[]{
			(Action<string,string,string,string,string,int,Texture2D,bool>)ExternalAddAchievement,
			(Action<string[],int>)ExternalConfigAchievementNetMode,
			(Action<string[],int>)ExternalConfigAchievementDifficulty,
			(Action<string[],int>)ExternalConfigAchievementHardmode,
			(Action<string,object,Func<object,object,string>>)ExternalConfigAchievementProgress,
			(Func<string,bool>)ExternalGetAchieved,
			(Func<string,Texture2D,bool>)ExternalAchieve,
			(Action<int,string>)ExternalAchievePlayer,
			(Action<string>)ExternalAchieveAllPlayers,
			(Func<string,object[]>)ExternalGetAchievementProgress,
			(Func<string,object,Texture2D,bool>)ExternalSetAchievementProgress,
			(Func<string,object,Texture2D,bool>)ExternalAchievementProgress,
			(Action<int,string,object>)ExternalAchievementProgressPlayer,
			(Action<string,object>)ExternalAchievementProgressAllPlayers,
			(Func<string,object[]>)ExternalGetAchievementInfo
		});
	}
}
public void CreatePlayer(Player p) {
	Initialize();
}
public void Save(BinaryWriter bw) {
	bw.Write(achievements.Count);
	foreach (Achievement ac in achievements) ac.Write(bw);
}
public void Load(BinaryReader br, int version) {
	Initialize();
	try {
		int count = br.ReadInt32();
		while (count-- > 0) {
			string apiName = br.ReadString();
			bool achieved = br.ReadBoolean();
			foreach (Achievement ac in achievements) if (ac.apiName == apiName) {
				ac.achieved = achieved;
				ac.ReadProgress(br);
				break;
			}
		}
	} catch (Exception) {}
}

public static void ExternalAddAchievement(string apiName, string category, string title, string description, string parent = null, int value = 10, Texture2D tex = null, bool hidden = false) {
	achievements.Add(new Achievement(apiName,category,title,description,parent,value,tex,hidden));
}
public static void ExternalConfigAchievementNetMode(string[] apiNames, int netMode) { //1 - multiplayer, 0 - singleplayer, -1 - any
	foreach (Achievement ac in achievements) foreach (string apiName in apiNames) if (ac.apiName == apiName) ac.netMode = netMode;
}
public static void ExternalConfigAchievementDifficulty(string[] apiNames, int difficulty) { //1 - soft, 2 - medium, 4 - hard, --- 1+2=3 - soft+medium, 1+4=5 - soft+hard, 2+4=6 - medium+hard, 1+2+4=7 - all(default)
	foreach (Achievement ac in achievements) foreach (string apiName in apiNames) if (ac.apiName == apiName) ac.difficulty = difficulty;
}
public static void ExternalConfigAchievementHardmode(string[] apiNames, int hardMode) { //1 - pre-hardmode, 2 - hardmode, 3 - either
	foreach (Achievement ac in achievements) foreach (string apiName in apiNames) if (ac.apiName == apiName) ac.hardMode = hardMode;
}
public static void ExternalConfigAchievementProgress(string apiName, object progressMax, Func<object,object,string> progressParser = null) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) {
		if (progressMax == null) {
			ac.progress = null;
			ac.progressMax = null;
		} else if (progressMax is double) {
			ac.progress = ac.progress == null ? 0d : (double)ac.progress;
			ac.progressMax = (double)progressMax;
		} else if (progressMax is int) {
			ac.progress = ac.progress == null ? 0 : (int)ac.progress;
			ac.progressMax = (int)progressMax;
		}
		ac.progressParser = progressParser == null ? Achievement.defaultProgressParser : progressParser;
		return;
	}
}

public static bool ExternalGetAchieved(string apiName) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) return ac.achieved;
	return false;
}
public static bool ExternalAchieve(string apiName, Texture2D tex = null) { //should only be used locally! (singleplayer or client-side in multiplayer)
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) {
		if (!ac.achieved) {
			ac.achieved = true;
			if (tex != null) ac.tex = tex;
			ModWorld.notifiers.Insert(0,new ModWorld.Notifier(ac));
			if (Main.netMode == ModWorld.MODE_CLIENT) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_ACHIEVED,-1,-1,(byte)Main.myPlayer,apiName);
			return true;
		} else return false;
	}
	return false;
}
public static void ExternalAchievePlayer(int playerId, string apiName) { //should only be used server-side!
	NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_ACHIEVE,playerId,-1,apiName);
}
public static void ExternalAchieveAllPlayers(string apiName) { //should only be used server-side!
	ExternalAchievePlayer(-1,apiName);
}

public static object[] ExternalGetAchievementProgress(string apiName) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) return new object[]{ac.progress,ac.progressMax};
	return null;
}
public static bool ExternalSetAchievementProgress(string apiName, object value, Texture2D tex = null) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) {
		ac.progress = value;
		if (ac.progress == null) return false;
		else if (ac.progress is double) {
			if ((double)ac.progress >= (double)ac.progressMax) return ExternalAchieve(apiName,tex);
		} else if (ac.progress is int) {
			if ((int)ac.progress >= (int)ac.progressMax) return ExternalAchieve(apiName,tex);
		}
		return false;
	}
	return false;
}
public static bool ExternalAchievementProgress(string apiName, object value, Texture2D tex = null) { //should only be used locally! (singleplayer or client-side in multiplayer)
	if (value == null) return ExternalSetAchievementProgress(apiName,value,tex);
	else if (value is double) return ExternalSetAchievementProgress(apiName,(double)(ExternalGetAchievementProgress(apiName)[0])+(double)(value),tex);
	else if (value is int) return ExternalSetAchievementProgress(apiName,(int)(ExternalGetAchievementProgress(apiName)[0])+(int)(value),tex);
	return false;
}
public static void ExternalAchievementProgressPlayer(int playerId, string apiName, object value) { //should only be used server-side!
	if (value == null) {
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_PROGRESS,playerId,-1,apiName,(byte)0,value);
	} else if (value is double) {
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_PROGRESS,playerId,-1,apiName,(byte)1,(double)value);
	} else if (value is int) {
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_PROGRESS,playerId,-1,apiName,(byte)2,(int)value);
	}
}
public static void ExternalAchievementProgressAllPlayers(string apiName, object value) { //should only be used server-side!
	ExternalAchievementProgressPlayer(-1,apiName,value);
}

public static Achievement GetAchievement(string apiName) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) return ac;
	return null;
}
public static object[] ExternalGetAchievementInfo(string apiName) {
	foreach (Achievement ac in achievements) if (ac.apiName == apiName) return new object[]{apiName,ac.category,ac.title,ac.description,ac.value,ac.tex,ac.hidden,ac.netMode,ac.difficulty};
	return null;
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
	string s, cat;
	
	cat = "Terraria";
	s = "TERRARIA_KILLPINKY"; AddAchievement(s,cat,"HOW COULD YOU?!","Slay Pinky.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,true);
	s = "TERRARIA_GETACC"; AddAchievement(s,cat,"Gearing up","Equip your first accessory.",null,10,Main.itemTexture[Config.itemDefs.byName["Cloud in a Bottle"].type],false);
	s = "TERRARIA_GETANGELSTATUE"; AddAchievement(s,cat,"What do I do with it?","Find an Angel Statue.",null,10,Main.itemTexture[Config.itemDefs.byName["Angel Statue"].type],false);
	s = "TERRARIA_FIRSTHEALTH"; AddAchievement(s,cat,"Heart Breaker","Use your first Life Crystal.",null,10,Main.itemTexture[Config.itemDefs.byName["Life Crystal"].type],false);
	s = "TERRARIA_MAXHEALTH"; AddAchievement(s,cat,"Fit as a fiddle","Reach max health.","TERRARIA_FIRSTHEALTH",30,Main.itemTexture[Config.itemDefs.byName["Life Crystal"].type],false);
	s = "TERRARIA_FIRSTMANA"; AddAchievement(s,cat,"May the Force be with you","Use your first Mana Crystal.",null,10,Main.itemTexture[Config.itemDefs.byName["Mana Crystal"].type],false);
	s = "TERRARIA_MAXMANA"; AddAchievement(s,cat,"Parry Hotter","Reach max mana.","TERRARIA_FIRSTMANA",30,Main.itemTexture[Config.itemDefs.byName["Mana Crystal"].type],false);
	s = "TERRARIA_LONGFALL"; AddAchievement(s,cat,"Are we there yet?","Fall at least 500ft.",null,20,Main.itemTexture[Config.itemDefs.byName["Feather"].type],true);
	s = "TERRARIA_METEOR"; AddAchievement(s,cat,"A gift from SPAAAAAAAAACE!","Get a Meteorite to fall.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_GET10P"; AddAchievement(s,cat,"Bling Bling","Acquire 10 Platinum Coins.",null,30,Main.itemTexture[Config.itemDefs.byName["Platinum Coin"].type],false);
	s = "TERRARIA_KILLGUIDE"; AddAchievement(s,cat,"Winning!","Kill the Guide.",null,20,Main.itemTexture[Config.itemDefs.byName["Guide Voodoo Doll"].type],true);
	s = "TERRARIA_TOTALDMG100K"; AddAchievement(s,cat,"Massive Damage","Deal a total of 100,000 damage.",null,20,Main.itemTexture[Config.itemDefs.byName["Night's Edge"].type],false);
	s = "TERRARIA_TOTALDMG10M"; AddAchievement(s,cat,"Hyperdamage","Deal a total of 10,000,000 damage.","TERRARIA_TOTALDMG100K",20,Main.itemTexture[Config.itemDefs.byName["Excalibur"].type],false);
	s = "TERRARIA_TIMEPLAYED1"; AddAchievement(s,cat,"Work Work","Play for an hour.",null,10,Main.itemTexture[Config.itemDefs.byName["Copper Watch"].type],false);
	s = "TERRARIA_TIMEPLAYED2"; AddAchievement(s,cat,"Time waster","Play for 5 hours.","TERRARIA_TIMEPLAYED1",20,Main.itemTexture[Config.itemDefs.byName["Silver Watch"].type],false);
	s = "TERRARIA_TIMEPLAYED3"; AddAchievement(s,cat,"No-life","Play for 20 hours.","TERRARIA_TIMEPLAYED2",30,Main.itemTexture[Config.itemDefs.byName["Gold Watch"].type],false);
	s = "TERRARIA_TOTALMOVE10K"; AddAchievement(s,cat,"Stepping out","Move a total of 10,000ft.",null,10,Main.itemTexture[Config.itemDefs.byName["Hermes Boots"].type],false);
	s = "TERRARIA_TOTALMOVE100K"; AddAchievement(s,cat,"Wanderer","Move a total of 100,000ft.","TERRARIA_TOTALMOVE10K",20,Main.itemTexture[Config.itemDefs.byName["Rocket Boots"].type],false);
	s = "TERRARIA_TOTALMOVE1M"; AddAchievement(s,cat,"Globe-trotter","Move a total of 1,000,000ft.","TERRARIA_TOTALMOVE100K",30,Main.itemTexture[Config.itemDefs.byName["Spectre Boots"].type],false);
	s = "TERRARIA_MISMATCHED"; AddAchievement(s,cat,"Mismatched","Wear all different pieces of armor.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,true);
	s = "TERRARIA_MAGICMIRROR"; AddAchievement(s,cat,"I must go, my people need me","Use a Magic Mirror.",null,10,Main.itemTexture[Config.itemDefs.byName["Magic Mirror"].type],true);
	s = "TERRARIA_ALE"; AddAchievement(s,cat,"Tipsy","Drink Ale.",null,10,Main.itemTexture[Config.itemDefs.byName["Ale"].type],true);
	s = "TERRARIA_GLOWINGMUSHROOM"; AddAchievement(s,cat,"Trippin'","Acquire a Glowing Mushroom.",null,10,Main.itemTexture[Config.itemDefs.byName["Glowing Mushroom"].type],true);
	s = "TERRARIA_WHOOPIECUSHION"; AddAchievement(s,cat,"Quick, someone light a match!","Use a Whoopie Cushion.",null,10,Main.itemTexture[Config.itemDefs.byName["Whoopie Cushion"].type],true);
	
	cat = "Terraria->NPCs";
	s = "TERRARIA_NPC_MERCHANT"; AddAchievement(s,cat,"It's a Pawn Shop","Get Merchant to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Merchant"].type)],false);
	s = "TERRARIA_NPC_NURSE"; AddAchievement(s,cat,"Scrubs","Get Nurse to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Nurse"].type)],false);
	s = "TERRARIA_NPC_DEMOLITIONIST"; AddAchievement(s,cat,"Oh, sorry, did you need that leg?","Get Demolitionist to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Demolitionist"].type)],false);
	s = "TERRARIA_NPC_DRYAD"; AddAchievement(s,cat,"You should try harder!","Get Dryad to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Dryad"].type)],false);
	s = "TERRARIA_NPC_ARMSDEALER"; AddAchievement(s,cat,"More 'bang' for your buck","Get Arms Dealer to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Arms Dealer"].type)],false);
	s = "TERRARIA_NPC_CLOTHIER"; AddAchievement(s,cat,"Curse - undone","Get Clothier to spawn.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Clothier"].type)],false);
	s = "TERRARIA_NPC_MECHANIC"; AddAchievement(s,cat,"The shocking truth","Unbind Mechanic.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Mechanic"].type)],false);
	s = "TERRARIA_NPC_GOBLINTINKERER"; AddAchievement(s,cat,"The enemy of my enemy","Unbind Goblin Tinkerer.",null,10,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Goblin Tinkerer"].type)],false);
	
	cat = "Terraria->Death";
	s = "TERRARIA_DEATHDUNGEONGUARDIAN"; AddAchievement(s,cat,"Leeroy Jenkins","Get killed by a Dungeon Guardian.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,true);
	s = "TERRARIA_DEATHCORRUPTGOLDFISH"; AddAchievement(s,cat,"In Soviet Russia...","Get killed by a Corrupt Goldfish.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,true);
	s = "TERRARIA_DEATHMIMIC"; AddAchievement(s,cat,"Rick Roll'd","Get killed by a Mimic.",null,10,Main.itemTexture[Config.itemDefs.byName["Gold Chest"].type],true);
	s = "TERRARIA_DEATHSAND"; AddAchievement(s,cat,"Sand is overpowered","Get killed by sand.",null,10,Main.itemTexture[Config.itemDefs.byName["Sand Block"].type],true);
	
	cat = "Terraria->Biomes";
	s = "TERRARIA_BIOMEUNDERWORLD"; AddAchievement(s,cat,"Sinner","Reach the Underworld.",null,20,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_BIOMEJUNGLE"; AddAchievement(s,cat,"Welcome to the Jungle","Find the Jungle biome.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_BIOMECORRUPTION"; AddAchievement(s,cat,"I have a bad feeling about this","Enter a Corruption biome.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_BIOMEHALLOW"; AddAchievement(s,cat,"This IS Disney Land!","Enter a Hallow biome.",null,30,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	
	cat = "Terraria->Crafting";
	s = "TERRARIA_GETGRAPPLINGHOOK"; AddAchievement(s,cat,"Hooked","Acquire a Grappling Hook.",null,10,Main.itemTexture[Config.itemDefs.byName["Grappling Hook"].type],false);
	s = "TERRARIA_GETIVYWHIP"; AddAchievement(s,cat,"Vinely Inspired","Acquire an Ivy Whip.","TERRARIA_GETGRAPPLINGHOOK",10,Main.itemTexture[Config.itemDefs.byName["Ivy Whip"].type],false);
	s = "TERRARIA_GETSTARCANNON"; AddAchievement(s,cat,"Shooting Stars","Acquire a Star Cannon.",null,20,Main.itemTexture[Config.itemDefs.byName["Star Cannon"].type],false);
	
	cat = "Terraria->Mining";
	s = "TERRARIA_MINECOPPER"; AddAchievement(s,cat,"Struck Copper","Mine Copper Ore.",null,10,Main.itemTexture[Config.itemDefs.byName["Copper Ore"].type],false);
	s = "TERRARIA_MINEIRON"; AddAchievement(s,cat,"Struck Iron","Mine Iron Ore.",null,10,Main.itemTexture[Config.itemDefs.byName["Iron Ore"].type],false);
	s = "TERRARIA_MINESILVER"; AddAchievement(s,cat,"Struck Silver","Mine Silver Ore.",null,10,Main.itemTexture[Config.itemDefs.byName["Silver Ore"].type],false);
	s = "TERRARIA_MINEGOLD"; AddAchievement(s,cat,"Struck Gold","Mine Gold Ore.",null,10,Main.itemTexture[Config.itemDefs.byName["Gold Ore"].type],false);
	s = "TERRARIA_MINEMETEORITE"; AddAchievement(s,cat,"Struck Meteorite","Mine Meteorite.",null,20,Main.itemTexture[Config.itemDefs.byName["Meteorite"].type],false);
	s = "TERRARIA_MINEHELLSTONE"; AddAchievement(s,cat,"Struck Hellstone","Mine Hellstone.",null,20,Main.itemTexture[Config.itemDefs.byName["Hellstone"].type],false);
	
	cat = "Terraria->Multiplayer";
	s = "TERRARIA_MP"; AddAchievement(s,cat,"Double the fun!","Join a multiplayer game.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_PVPINVISIBLE"; AddAchievement(s,cat,"Et tu, Brutus?","Kill another player in PVP while being invisible.",null,20,Main.itemTexture[Config.itemDefs.byName["Invisibility Potion"].type],true);
	
	cat = "Terraria->Bosses";
	s = "TERRARIA_BOSS_EOC"; AddAchievement(s,cat,"Eye Sore","Defeat the Eye of Cthulhu.",null,20,Main.itemTexture[Config.itemDefs.byName["Suspicious Looking Eye"].type],false);
	s = "TERRARIA_BOSS_EOW"; AddAchievement(s,cat,"You want spice with that?","Defeat the Eater of Worlds.",null,20,Main.itemTexture[Config.itemDefs.byName["Worm Food"].type],false);
	s = "TERRARIA_BOSS_SKELETRON"; AddAchievement(s,cat,"Exorcist","Defeat Skeletron.",null,20,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "TERRARIA_BOSS_KINGSLIME"; AddAchievement(s,cat,"Look at the size of that thing!","Defeat King Slime.",null,20,Main.itemTexture[Config.itemDefs.byName["Slime Crown"].type],false);
	s = "TERRARIA_BOSS_WOF"; AddAchievement(s,cat,"Welcome to Hardmode","Defeat the Wall of Flesh.",null,30,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	
	cat = "Terraria->Events";
	s = "TERRARIA_EVENT_BLOODMOON"; AddAchievement(s,cat,"Git off mah lawn","Survive Blood Moon.",null,10,Main.itemTexture[Config.itemDefs.byName["Deathweed"].type],false);
	s = "TERRARIA_EVENT_GOBLIN"; AddAchievement(s,cat,"It's quiet... too quiet","Defeat the Goblin Army.",null,20,Main.itemTexture[Config.itemDefs.byName["Goblin Battle Standard"].type],false);
	
	cat = "Terraria->Hardmode";
	s = "TERRARIA_EVENT_SNOWMEN"; AddAchievement(s,cat,"Winter Warland","Defeat the Frost Legion.",null,40,Main.itemTexture[Config.itemDefs.byName["Snow Globe"].type],false);
	s = "TERRARIA_HM_KILLWYVERNNODMG"; AddAchievement(s,cat,"Dragonborn","Kill a Wyvern without taking damage.",null,40,Main.itemTexture[Config.itemDefs.byName["Soul of Flight"].type],false);
	
	cat = "Terraria->Hardmode->NPCs";
	s = "TERRARIA_NPC_WIZARD"; AddAchievement(s,cat,"Chafing is more important than you","Unbind Wizard.",null,30,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Wizard"].type)],false);
	s = "TERRARIA_NPC_SANTACLAUS"; AddAchievement(s,cat,"You thought I wasn't real?","Get Santa Claus to spawn.",null,30,Main.npcHeadTexture[NPC.TypeToNum(Config.npcDefs.byName["Santa Claus"].type)],false);
	
	cat = "Terraria->Hardmode->Crafting";
	s = "TERRARIA_GETMEGASHARK"; AddAchievement(s,cat,"Land Shark","Acquire a Megashark.",null,30,Main.itemTexture[Config.itemDefs.byName["Megashark"].type],false);
	s = "TERRARIA_GETREDPHASESABER"; AddAchievement(s,cat,"I am your father","Acquire a Red Phasesaber.",null,30,Main.itemTexture[Config.itemDefs.byName["Red Phaseblade"].type],true);
	s = "TERRARIA_GETEXCALIBUR"; AddAchievement(s,cat,"All Hail King Arthur!","Acquire an Excalibur.",null,30,Main.itemTexture[Config.itemDefs.byName["Excalibur"].type],false);
	s = "TERRARIA_GETGUNGNIR"; AddAchievement(s,cat,"The Swaying One","Acquire a Gungnir.",null,30,Main.itemTexture[Config.itemDefs.byName["Gungnir"].type],false);
	s = "TERRARIA_GETRAINBOWROD"; AddAchievement(s,cat,"IT'S A DOUBLE RAINBOW!!!","Acquire a Rainbow Rod.",null,30,Main.itemTexture[Config.itemDefs.byName["Rainbow Rod"].type],false);
	s = "TERRARIA_GETWINGS"; AddAchievement(s,cat,"Gives You Wiiings!","Acquire Wings.",null,30,Main.itemTexture[Config.itemDefs.byName["Angel Wings"].type],false);
	
	cat = "Terraria->Hardmode->Mining";
	s = "TERRARIA_HM_DEMONALTAR"; AddAchievement(s,cat,"Pwnage","Break a Demon Altar.",null,30,Main.itemTexture[Config.itemDefs.byName["Pwnhammer"].type],false);
	s = "TERRARIA_HM_MINECOBALT"; AddAchievement(s,cat,"Struck Cobalt","Mine Cobalt Ore.",null,30,Main.itemTexture[Config.itemDefs.byName["Cobalt Ore"].type],false);
	s = "TERRARIA_HM_MINEMYTHRIL"; AddAchievement(s,cat,"Struck Mythril","Mine Mythril Ore.",null,30,Main.itemTexture[Config.itemDefs.byName["Mythril Ore"].type],false);
	s = "TERRARIA_HM_MINEADAMANTITE"; AddAchievement(s,cat,"Struck Adamantite","Mine Adamantite Ore.",null,30,Main.itemTexture[Config.itemDefs.byName["Adamantite Ore"].type],false);
	
	cat = "Terraria->Hardmode->Bosses";
	s = "TERRARIA_HM_BOSS_TWINS"; AddAchievement(s,cat,"An Eye for an Eye","Defeat The Twins.",null,40,Main.itemTexture[Config.itemDefs.byName["Mechanical Eye"].type],false);
	s = "TERRARIA_HM_BOSS_DESTROYER"; AddAchievement(s,cat,"Now With Lazers?!?!","Defeat The Destroyer.",null,40,Main.itemTexture[Config.itemDefs.byName["Mechanical Worm"].type],false);
	s = "TERRARIA_HM_BOSS_PRIME"; AddAchievement(s,cat,"Terror Gets an Upgrade","Defeat Skeletron Prime.",null,40,Main.itemTexture[Config.itemDefs.byName["Mechanical Skull"].type],false);
	
	ConfigNetMode(new string[]{"TERRARIA_MP","TERRARIA_PVPINVISIBLE"},1);
	ConfigDifficulty(new string[]{"TERRARIA_DEATHDUNGEONGUARDIAN","TERRARIA_DEATHSAND","TERRARIA_DEATHCORRUPTGOLDFISH","TERRARIA_DEATHMIMIC"},3);
	ConfigHardmode(new string[]{"TERRARIA_EVENT_SNOWMEN","TERRARIA_GETMEGASHARK","TERRARIA_GETREDPHASESABER","TERRARIA_HM_MINECOBALT",
		"TERRARIA_HM_MINEMYTHRIL","TERRARIA_HM_MINEADAMANTITE","TERRARIA_HM_BOSS_TWINS","TERRARIA_HM_BOSS_DESTROYER","TERRARIA_HM_BOSS_PRIME",
		"TERRARIA_GETEXCALIBUR","TERRARIA_GETGUNGNIR","TERRARIA_GETRAINBOWROD","TERRARIA_GETWINGS","TERRARIA_HM_DEMONALTAR",
		"TERRARIA_NPC_WIZARD","TERRARIA_NPC_SANTACLAUS","TERRARIA_HM_KILLWYVERNNODMG","TERRARIA_BIOMEHALLOW","TERRARIA_DEATHMIMIC"},2);
	
	if (GetProgress("TERRARIA_MAXHEALTH")[1] == null) ConfigProgress("TERRARIA_MAXHEALTH",400,null);
	if (GetProgress("TERRARIA_MAXMANA")[1] == null) ConfigProgress("TERRARIA_MAXMANA",200,null);
	ConfigProgress("TERRARIA_MAXMANA",Codable.RunGlobalMethod("ModWorld","ExternalGetMaxMana",new object[]{}) ? (int)Codable.customMethodReturn : 200,null);
	ConfigProgress("TERRARIA_GET10P",10000000,FormatAcCoins);
	ConfigProgress("TERRARIA_TOTALDMG100K",100000,null);
	ConfigProgress("TERRARIA_TOTALDMG10M",10000000,null);
	ConfigProgress("TERRARIA_TIMEPLAYED1",60*60*60,FormatAcTime);
	ConfigProgress("TERRARIA_TIMEPLAYED2",60*60*60*5,FormatAcTime);
	ConfigProgress("TERRARIA_TIMEPLAYED3",60*60*60*20,FormatAcTime);
	ConfigProgress("TERRARIA_TOTALMOVE10K",10000d,FormatAcDist);
	ConfigProgress("TERRARIA_TOTALMOVE100K",100000d,FormatAcDist);
	ConfigProgress("TERRARIA_TOTALMOVE1M",1000000d,FormatAcDist);
}

public static string FormatAcCoins(object o1, object o2) {
	if (o1 is int) return FormatAcCoins2((int)o1)+" / "+FormatAcCoins2((int)o2);
	return null;
}
public static string FormatAcCoins2(int value) {
	int P = 0, G = 0, S = 0, C = 0;
	C = value % 100; value /= 100;
	S = value % 100; value /= 100;
	G = value % 100; value /= 100;
	P = value;
	
	string s = "";
	if (P > 0) s += (s == "" ? "" : " ")+P+"p";
	if (P+G > 0) s += (s == "" ? "" : " ")+G+"g";
	if (P+G+S > 0) s += (s == "" ? "" : " ")+S+"s";
	s += (s == "" ? "" : " ")+C+"c";
	return s;
}

public static string FormatAcTime(object o1, object o2) {
	if (o1 is int) return FormatAcTime2((int)o1)+" / "+FormatAcTime2((int)o2);
	return null;
}
public static string FormatAcTime2(int value) {
	int d = 0, h = 0, m = 0, s = 0;
	value /= 60;
	s = value % 60; value /= 60;
	m = value % 60; value /= 60;
	h = value % 24; value /= 24;
	d = value;
	
	string ret = "";
	if (d > 0) ret += (ret == "" ? "" : " ")+d+"d";
	if (d+h > 0) ret += (ret == "" ? "" : " ")+h+"h";
	if (d+h+m > 0) ret += (ret == "" ? "" : " ")+m+"m";
	ret += (ret == "" ? "" : " ")+s+"s";
	return ret;
}

public static string FormatAcDist(object o1, object o2) {
	if (o1 is double) return FormatAcDist2((int)Math.Floor((double)o1))+" / "+FormatAcDist2((int)Math.Ceiling((double)o2));
	return null;
}
public static string FormatAcDist2(int value) {
	string s = "";
	while (value > 0) {
		int v = value % 1000;
		value -= v;
		value /= 1000;
		s = value != 0 ? ","+String.Format("{0:000}",v)+s : ""+v+s;
	}
	if (s == "") s = "0";
	return s+"ft";
}

public void UpdatePlayer(Player player) {
	if (player == null) return;
	if (player.name == "") return;
	if (player.whoAmi != Main.myPlayer) return;
	
	for (int i = 0; i < ModWorld.notifiers.Count; i++) {
		ModWorld.Notifier n = ModWorld.notifiers[i];
		n.Update(i);
		if (n.dead) ModWorld.notifiers.RemoveAt(i--);
	}
	
	Microsoft.Xna.Framework.Input.KeyboardState newState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
	if (newState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) && !keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) {
		if (ModWorld.GuiAchievements.visible) {
			Main.playerInventory = false;
			ModWorld.GuiAchievements.visible = false;
		}
	}
	keyState = newState;
	
	CheckAcGetAcc(player);
	CheckAcBoss(player);
	if (player.position.Y/16f > Main.maxTilesY-200) ExternalAchieve("TERRARIA_BIOMEUNDERWORLD");
	if (player.zoneJungle) ExternalAchieve("TERRARIA_BIOMEJUNGLE");
	if (player.zoneEvil) ExternalAchieve("TERRARIA_BIOMECORRUPTION");
	if (player.zoneHoly) ExternalAchieve("TERRARIA_BIOMEHALLOW");
	ExternalSetAchievementProgress("TERRARIA_MAXHEALTH",player.statLifeMax);
	ExternalSetAchievementProgress("TERRARIA_MAXMANA",player.statManaMax);
	if (player.statLifeMax > 100) ExternalAchieve("TERRARIA_FIRSTHEALTH");
	if (player.statManaMax > 0) ExternalAchieve("TERRARIA_FIRSTMANA");
	if (player.position.Y/16f-player.fallStart >= 250) ExternalAchieve("TERRARIA_LONGFALL");
	if (Main.netMode == 1) ExternalAchieve("TERRARIA_MP");
	
	ExternalAchievementProgress("TERRARIA_TIMEPLAYED1",1);
	ExternalAchievementProgress("TERRARIA_TIMEPLAYED2",1);
	ExternalAchievementProgress("TERRARIA_TIMEPLAYED3",1);
	
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) continue;
		if (player.inventory[i].name == "Angel Statue") ExternalAchieve("TERRARIA_GETANGELSTATUE");
		if (player.inventory[i].name == "Red Phasesaber") ExternalAchieve("TERRARIA_GETREDPHASESABER");
		if (player.inventory[i].name == "Grappling Hook") ExternalAchieve("TERRARIA_GETGRAPPLINGHOOK");
		if (player.inventory[i].name == "Ivy Whip") ExternalAchieve("TERRARIA_GETIVYWHIP");
		if (player.inventory[i].name == "Megashark") ExternalAchieve("TERRARIA_GETMEGASHARK");
		if (player.inventory[i].name == "Star Cannon") ExternalAchieve("TERRARIA_GETSTARCANNON");
		if (player.inventory[i].name == "Excalibur") ExternalAchieve("TERRARIA_GETEXCALIBUR");
		if (player.inventory[i].name == "Gungnir") ExternalAchieve("TERRARIA_GETGUNGNIR");
		if (player.inventory[i].name == "Rainbow Rod") ExternalAchieve("TERRARIA_GETRAINBOWROD");
		if (player.inventory[i].name == "Angel Wings" || player.inventory[i].name == "Demon Wings") ExternalAchieve("TERRARIA_GETWINGS");
		if (player.inventory[i].name == "Glowing Mushroom") ExternalAchieve("TERRARIA_GLOWINGMUSHROOM");
	}
	
	int count = 0;
	for (int i = 40; i <= 43; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) continue;
		if (player.inventory[i].name == "Copper Coin") count += player.inventory[i].stack;
		else if (player.inventory[i].name == "Silver Coin") count += player.inventory[i].stack*100;
		else if (player.inventory[i].name == "Gold Coin") count += player.inventory[i].stack*10000;
		else if (player.inventory[i].name == "Platinum Coin") count += player.inventory[i].stack*1000000;
	}
	ExternalSetAchievementProgress("TERRARIA_GET10P",count);
	
	if (lastPos.X != -1 && lastPos.Y != -1) {
		double dist = Math.Sqrt(Math.Pow(lastPos.X-player.position.X,2)+Math.Pow(lastPos.Y-player.position.Y,2))/8d;
		if (dist < 20) {
			ExternalAchievementProgress("TERRARIA_TOTALMOVE10K",dist);
			ExternalAchievementProgress("TERRARIA_TOTALMOVE100K",dist);
			ExternalAchievementProgress("TERRARIA_TOTALMOVE1M",dist);
		}
	}
	lastPos = new Vector2(player.position.X,player.position.Y);
	
	for (int i = 0; i < Main.npc.Length; i++) {
		if (Main.npc[i] == null) continue;
		if (Main.npc[i].name == "") continue;
		if (Main.npc[i].life <= 0) continue;
		
		if (Main.npc[i].name == "Merchant") ExternalAchieve("TERRARIA_NPC_MERCHANT");
		if (Main.npc[i].name == "Nurse") ExternalAchieve("TERRARIA_NPC_NURSE");
		if (Main.npc[i].name == "Demolitionist") ExternalAchieve("TERRARIA_NPC_DEMOLITIONIST");
		if (Main.npc[i].name == "Dryad") ExternalAchieve("TERRARIA_NPC_DRYAD");
		if (Main.npc[i].name == "Arms Dealer") ExternalAchieve("TERRARIA_NPC_ARMSDEALER");
		if (Main.npc[i].name == "Clothier") ExternalAchieve("TERRARIA_NPC_CLOTHIER");
		if (Main.npc[i].name == "Mechanic") ExternalAchieve("TERRARIA_NPC_MECHANIC");
		if (Main.npc[i].name == "Goblin Tinkerer") ExternalAchieve("TERRARIA_NPC_GOBLINTINKERER");
		if (Main.npc[i].name == "Wizard") ExternalAchieve("TERRARIA_NPC_WIZARD");
		if (Main.npc[i].name == "Santa Claus") ExternalAchieve("TERRARIA_NPC_SANTACLAUS");
	}
}

public void CheckAcMismatched(Player player) {
	for (int i = 0; i <= 2; i++) if (player.armor[i] == null || player.armor[i].name == "" || player.armor[i].stack <= 0) return;
	if (Config.armorSets[player.armor[0].name] != Config.armorSets[player.armor[1].name] && Config.armorSets[player.armor[1].name] != Config.armorSets[player.armor[2].name] && Config.armorSets[player.armor[0].name] != Config.armorSets[player.armor[2].name]) ExternalAchieve("TERRARIA_MISMATCHED");
}
public void CheckAcGetAcc(Player player) {
	for (int i = 3; i <= 7; i++) {
		if (player.armor[i] == null || player.armor[i].name == null || player.armor[i].name == "" || player.armor[i].stack <= 0) continue;
		if (player.armor[i].accessory) {
			ExternalAchieve("TERRARIA_GETACC",Main.itemTexture[player.armor[i].type]);
			if (player.armor[i].name == "Angel Wings" || player.armor[i].name == "Demon Wings") ExternalAchieve("TERRARIA_GETWINGS");
			return;
		}
	}
	
	if (Codable.RunGlobalMethod("ModWorld","ExternalGetAccessorySlots",new object[]{})) {
		Item[] acc = (Item[])Codable.customMethodReturn;
		for (int i = 0; i < acc.Length; i++) {
			if (acc[i] == null || acc[i].name == null || acc[i].name == "" || acc[i].stack <= 0) continue;
			if (acc[i].accessory) {
				ExternalAchieve("TERRARIA_GETACC",Main.itemTexture[acc[i].type]);
				if (acc[i].name == "Angel Wings" || acc[i].name == "Demon Wings") ExternalAchieve("TERRARIA_GETWINGS");
				return;
			}
		}
	}
}
public void CheckAcBoss(Player player) {
	bool[] newState = new bool[bossState.Length];
	for (int i = 0; i < newState.Length; i++) newState[i] = false;

	foreach (NPC npc in Main.npc) {
		if (npc == null) continue;
		if (npc.name == null || npc.name == "") continue;
		if (npc.life <= 0) continue;
		
		switch (npc.name) {
			case "Eye of Cthulhu": newState[BOSS_EOC] = true; break;
			case "Eater of Worlds Head": case "Eater of Worlds Body": case "Eater of Worlds Tail": newState[BOSS_EOW] = true; break;
			case "Skeletron Head": newState[BOSS_SKELETRON] = true; break;
			case "King Slime": newState[BOSS_KINGSLIME] = true; break;
			case "Wall of Flesh": newState[BOSS_WOF] = true; break;
			case "Retinazer": case "Spazmatism": newState[BOSS_TWINS] = true; break;
			case "The Destroyer": case "The Destroyer Body": case "The Destroyer Tail": newState[BOSS_DESTROYER] = true; break;
			case "Skeletron Prime": newState[BOSS_PRIME] = true; break;
			default: break;
		}
	}
	
	for (int i = 0; i < newState.Length; i++) if (bossState[i] && !newState[i]) {
		switch (i) {
			case BOSS_EOC: ExternalAchieve("TERRARIA_BOSS_EOC"); break;
			case BOSS_EOW: ExternalAchieve("TERRARIA_BOSS_EOW"); break;
			case BOSS_SKELETRON: ExternalAchieve("TERRARIA_BOSS_SKELETRON"); break;
			case BOSS_KINGSLIME: ExternalAchieve("TERRARIA_BOSS_KINGSLIME"); break;
			case BOSS_WOF: ExternalAchieve("TERRARIA_BOSS_WOF"); break;
			case BOSS_TWINS: ExternalAchieve("TERRARIA_HM_BOSS_TWINS"); break;
			case BOSS_DESTROYER: ExternalAchieve("TERRARIA_HM_BOSS_DESTROYER"); break;
			case BOSS_PRIME: ExternalAchieve("TERRARIA_HM_BOSS_PRIME"); break;
			default: break;
		}
	}
	bossState = newState;
}

public void PostKill(Player player, double dmg, int hitDirection, bool pvp, string deathText) {
	if (player == null || player.whoAmi != Main.myPlayer) return;
	int tileX = (int)Math.Floor(player.position.X/16d), tileY = (int)Math.Floor(player.position.Y/16d);
	for (int yy = 0; yy < 3; yy++) for (int xx = 0; xx < 2; xx++) {
		if (Main.tile[tileX+xx,tileY+yy].active && sandBlocks.Contains(Main.tile[tileX+xx,tileY+yy].type)) {
			ExternalAchieve("TERRARIA_DEATHSAND");
			return;
		}
	}
}

public void DealtPlayer(Player player, double damage, NPC npc) {
	ModWorld.tookDamage[player.whoAmi] = true;
	
	if (player.whoAmi != Main.myPlayer) return;
	if (player.statLife > 0) return;
	
	if (npc.name == "Dungeon Guardian") ExternalAchieve("TERRARIA_DEATHDUNGEONGUARDIAN");
	if (npc.name == "Corrupt Goldfish") ExternalAchieve("TERRARIA_DEATHCORRUPTGOLDFISH");
	if (npc.name == "Mimic") ExternalAchieve("TERRARIA_DEATHMIMIC");
}
public void DealtPVP(Player source, ref int damage, Player target) {
	if (source.whoAmi != Main.myPlayer) return;
	
	ModPlayer.ExternalAchievementProgress("TERRARIA_TOTALDMG100K",damage);
	ModPlayer.ExternalAchievementProgress("TERRARIA_TOTALDMG10M",damage);
	
	if (target.statLife > 0) return;
	if (source.invis) {
		for (int i = 0; i <= 2; i++) if (source.armor[i].name != "") return;
		for (int i = 8; i <= 10; i++) if (source.armor[i].name != "") return;
		ExternalAchieve("TERRARIA_PVPINVISIBLE");
	}
}