#INCLUDE "Notifier.cs"
#INCLUDE "NPCEntry.cs"
#INCLUDE "GuiAchievements.cs"

public const int
	MSG_ACHIEVE = 1,
	MSG_ACHIEVED = 2,
	MSG_PROGRESS = 3,
	MODE_SOLO = 0,
	MODE_CLIENT = 1,
	MODE_SERVER = 2,
	TILE_COPPER = 7,
	TILE_IRON = 6,
	TILE_SILVER = 9,
	TILE_GOLD = 8,
	TILE_DEMONALTAR = 26,
	TILE_METEORITE = 37,
	TILE_HELLSTONE = 58,
	TILE_COBALT = 107,
	TILE_MYTHRIL = 108,
	TILE_ADAMANTITE = 111;

public static int modId;
public static List<Notifier> notifiers = new List<Notifier>();
public static NPCEntry[] npcEntries = new NPCEntry[Main.npc.Length];
public static bool[] tookDamage = new bool[Main.player.Length];
private static readonly Vector2[] shadowOffset = {new Vector2(-1,-1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(1,1)};

private static float buttonF = .8f;
private static bool buttonHover = false;

public static bool oldSpawnMeteor, oldBloodMoon;
public static int oldInvasionSize, oldInvasionType;

public static void Initialize(int modId) {
	ModWorld.modId = modId;
	
	if (!Main.dedServ) return;
	ModPlayer.Initialize();
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
	ModPlayer.ExternalInitAchievementsDelegates(AddAchievement,ConfigNetMode,ConfigDifficulty,ConfigHardmode,ConfigProgress,GetAchieved,Achieve,AchievePlayer,AchieveAllPlayers,GetProgress,SetProgress,Progress,ProgressPlayer,ProgressAllPlayers,GetAchievementInfo);
}

public static void NetReceive(int messageType, BinaryReader br) {
	if (Main.netMode == MODE_SERVER) {
		switch (messageType) {
			case MSG_ACHIEVED: {
				int playerID = (int)br.ReadByte();
				string acApiName = br.ReadString();
			
				Player player = Main.player[playerID];
				if (player == null) return;
				
				ModPlayer.Achievement ac = ModPlayer.GetAchievement(acApiName);
				if (ac == null) return;
				
				NetMessage.SendData(25,-1,-1,player.name+" achieved: "+ac.title+" (+"+ac.value+" point"+(ac.value == 1 ? "" : "s")+")",255,0f,255f,136f, 0);
			} break;
			default: break;
		}
	} else if (Main.netMode == MODE_CLIENT) {
		switch (messageType) {
			case MSG_ACHIEVE: {
				string acApiName = br.ReadString();
				ModPlayer.ExternalAchieve(acApiName);
			} break;
			case MSG_PROGRESS: {
				string acApiName = br.ReadString();
				switch (br.ReadByte()) {
					case 0: ModPlayer.ExternalAchievementProgress(acApiName,null); break;
					case 1: ModPlayer.ExternalAchievementProgress(acApiName,br.ReadDouble()); break;
					case 2: ModPlayer.ExternalAchievementProgress(acApiName,br.ReadInt32()); break;
					default: break;
				}
			} break;
			default: break;
		}
	}
}

public static void UpdateWorld() {
	switch (Main.netMode) {
		case MODE_SOLO: {
			if (oldSpawnMeteor) {
				if (!WorldGen.spawnMeteor) {
					ModPlayer.ExternalAchieve("TERRARIA_METEOR");
					oldSpawnMeteor = false;
				}
			} else if (WorldGen.spawnMeteor) oldSpawnMeteor = true;
			
			if (oldBloodMoon) {
				if (Main.dayTime) {
					ModPlayer.ExternalAchieve("TERRARIA_EVENT_BLOODMOON");
					oldBloodMoon = false;
				}
			} else if (!Main.dayTime && Main.bloodMoon) oldBloodMoon = true;
			
			if (oldInvasionSize > 0 && Main.invasionSize <= 0) {
				string apiName = null;
				switch (oldInvasionType) {
					case 2: apiName = "TERRARIA_EVENT_SNOWMEN"; break;
					default: apiName = "TERRARIA_EVENT_GOBLIN"; break;
				}
				ModPlayer.ExternalAchieve(apiName);
			}
			oldInvasionSize = Main.invasionSize;
			oldInvasionType = Main.invasionType;
		} break;
		case MODE_SERVER: {
			if (oldSpawnMeteor) {
				if (!WorldGen.spawnMeteor) {
					ModPlayer.ExternalAchieveAllPlayers("TERRARIA_METEOR");
					oldSpawnMeteor = false;
				}
			} else if (WorldGen.spawnMeteor) oldSpawnMeteor = true;
			
			if (oldBloodMoon) {
				if (Main.dayTime) {
					ModPlayer.ExternalAchieveAllPlayers("TERRARIA_EVENT_BLOODMOON");
					oldBloodMoon = false;
				}
			} else if (!Main.dayTime && Main.bloodMoon) oldBloodMoon = true;
			
			if (oldInvasionSize > 0 && Main.invasionSize <= 0) {
				string apiName = null;
				switch (oldInvasionType) {
					case 2: apiName = "TERRARIA_EVENT_SNOWMEN"; break;
					default: apiName = "TERRARIA_EVENT_GOBLIN"; break;
				}
				ModPlayer.ExternalAchieveAllPlayers(apiName);
			}
			oldInvasionSize = Main.invasionSize;
			oldInvasionType = Main.invasionType;
		} break;
		default: break;
	}
}
public void KillTile(int x, int y, Player player) {
	if (player == null) return;
	if (player.whoAmi != Main.myPlayer) return;
	
	switch (Main.tile[x,y].type) {
		case TILE_COPPER: ModPlayer.ExternalAchieve("TERRARIA_MINECOPPER"); break;
		case TILE_IRON: ModPlayer.ExternalAchieve("TERRARIA_MINEIRON"); break;
		case TILE_SILVER: ModPlayer.ExternalAchieve("TERRARIA_MINESILVER"); break;
		case TILE_GOLD: ModPlayer.ExternalAchieve("TERRARIA_MINEGOLD"); break;
		case TILE_METEORITE: ModPlayer.ExternalAchieve("TERRARIA_MINEMETEORITE"); break;
		case TILE_HELLSTONE: ModPlayer.ExternalAchieve("TERRARIA_MINEHELLSTONE"); break;
		case TILE_COBALT: ModPlayer.ExternalAchieve("TERRARIA_HM_MINECOBALT"); break;
		case TILE_MYTHRIL: ModPlayer.ExternalAchieve("TERRARIA_HM_MINEMYTHRIL"); break;
		case TILE_ADAMANTITE: ModPlayer.ExternalAchieve("TERRARIA_HM_MINEADAMANTITE"); break;
		case TILE_DEMONALTAR: ModPlayer.ExternalAchieve("TERRARIA_HM_DEMONALTAR"); break;
		default: break;
	}
}

public static void PreDrawInterface(SpriteBatch sb) {
	for (int i = 0; i < notifiers.Count; i++) notifiers[i].Draw(sb,i);
	GuiAchievements.Draw(sb);
}
public static bool PreDrawEscapeButtons(SpriteBatch sb) {
	string s = "Achievements";
	int xx = Main.screenWidth-226, yy = Main.screenHeight+14;
	Vector2 measure = Main.fontDeathText.MeasureString(s);
	
	DrawStringShadowed(sb,Main.fontDeathText,s,new Vector2(xx-measure.X/2,yy-measure.Y/2),Color.White,Color.Black,new Vector2(measure.X/2,measure.Y/2),buttonF-.2f);
	buttonF = buttonHover ? Math.Min(buttonF+.02f,1f) : Math.Max(buttonF-.02f,.8f);
	if (MouseIn(new Rectangle((int)(xx-measure.X),(int)(yy-measure.Y),(int)measure.X,44))) {
		if (!buttonHover) Main.PlaySound(12,-1,-1,1);
		buttonHover = true;
		Main.player[Main.myPlayer].mouseInterface = true;
		if (Main.mouseLeftRelease && Main.mouseLeft) {
			Config.tileInterface = null;
			Config.npcInterface = null;
			Main.npcShop = 0;
			Main.playerInventory = false;
			Main.signBubble = false;
			Main.editSign = false;
			Main.npcChatText = "";
			Main.player[Main.myPlayer].sign = -1;
			ModWorld.GuiAchievements.Toggle();
		}
	} else buttonHover = false;
	
	return true;
}
public static void PostDraw(SpriteBatch sb) {
	if (GuiAchievements.visible) Main.player[Main.myPlayer].mouseInterface = true;
}

public static void CustomGUIsOpen(Dictionary<int,int> A) {
	if (GuiAchievements.visible) A[0]++;
}

public static bool MouseIn(Rectangle rect) {
	return Main.mouseX >= rect.X && Main.mouseY >= rect.Y && Main.mouseX < rect.X+rect.Width && Main.mouseY < rect.Y+rect.Height;
}
public static void DrawStringShadowed(SpriteBatch sb, SpriteFont font, string text, Vector2 pos, Color color, Color colorShadow, Vector2 origin = default(Vector2), float scale = 1f, SpriteEffects effects = SpriteEffects.None) {
	foreach (Vector2 vecOff in shadowOffset) sb.DrawString(font,text,new Vector2(pos.X+vecOff.X,pos.Y+vecOff.Y),colorShadow,0f,origin,scale,effects,0f);
	sb.DrawString(font,text,pos,color,0f,origin,scale,effects,0f);
}