#INCLUDE "Quest.cs"
#INCLUDE "QuestMonster.cs"
#INCLUDE "QuestGather.cs"
#INCLUDE "Reward.cs"
#INCLUDE "GuiQuests.cs"

public static Quest[] quest = new Quest[5];
public static int questsFinished = 0;
public static Microsoft.Xna.Framework.Input.KeyboardState keyState;

public static Func<string,Texture2D,bool> AcAchieve;
public static Func<string,object,Texture2D,bool> AcSetProgress;

public void Initialize() {
	for (int i = 0; i < quest.Length; i++) quest[i] = null;
	questsFinished = 0;
}
public void CreatePlayer(Player p) {
	Initialize();
}
public void Save(BinaryWriter bw) {
	bw.Write(questsFinished);
	for (int i = 0; i < quest.Length; i++) {
		if (quest[i] != null) quest[i].save(bw);
		else bw.Write((byte)255);
	}
}
public void Load(BinaryReader br, int version) {
	Initialize();
	try {
		questsFinished = br.ReadInt32();
		for (int i = 0; i < quest.Length; i++) {
			int type = (int)br.ReadByte();
			switch (type) {
				case 0: quest[i] = new QuestMonster(br.ReadInt32(),new Reward(br.ReadString(),br.ReadInt32()),Config.npcDefs.byName[br.ReadString()]); break;
				case 1: quest[i] = new QuestGather(br.ReadInt32(),new Reward(br.ReadString(),br.ReadInt32()),Config.itemDefs.byName[br.ReadString()]); break;
				default: continue;
			}
			quest[i].progress = br.ReadInt32();
		}
	} catch (Exception) {}
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
	AcSetProgress = SetProgress;
	
	string s, cat;
	
	cat = "Shockah's mods->Quest Mod";
	s = "SHK_QUEST_1"; AddAchievement(s,cat,"Quest Completed!","Complete a quest.",null,10,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	s = "SHK_QUEST_50"; AddAchievement(s,cat,"Questor","Complete 50 quests.","SHK_QUEST_1",30,Config.goreID.ContainsKey("AC_"+s) ? Main.goreTexture[Config.goreID["AC_"+s]] : null,false);
	
	ConfigProgress("SHK_QUEST_50",50,null);
}

public void UpdatePlayer(Player player) {
	if (player.whoAmi != Main.myPlayer) return;
	
	Microsoft.Xna.Framework.Input.KeyboardState newState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
	if (newState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Home) && !keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Home)) {
		GuiQuests.Create();
	}
	keyState = newState;
	
	for (int i = 0; i < quest.Length; i++) {
		if (quest[i] == null) continue;
		if (!(quest[i] is QuestGather)) continue;
		
		QuestGather q = (QuestGather)quest[i];
		bool took = false;
		for (int j = 0; j < 40; j++) if (player.inventory[j].name == q.gather.name) {
			int take = Math.Min(Math.Max(player.inventory[j].stack,0),q.progressTotal-q.progress);
			q.progress += take;
			if (take > 0) took = true;
			player.inventory[j].stack -= take;
			if (player.inventory[j].stack <= 0) player.inventory[j] = new Item();
		}
		if (q.progress >= q.progressTotal) {
			q.endQuest();
			ModPlayer.quest[i] = null;
		}
		if (took) break;
	}
}