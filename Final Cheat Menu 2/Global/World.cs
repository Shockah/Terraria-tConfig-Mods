#INCLUDE "GuiCheat.cs"
#INCLUDE "GuiItem.cs"
#INCLUDE "GuiPrefix.cs"
#INCLUDE "GuiNPC.cs"
#INCLUDE "GuiBuff.cs"
#INCLUDE "GuiMisc.cs"
#INCLUDE "CheatNotification.cs"

public const int
	MSG_CHEAT_NOTIFICATION = 1,
	MSG_SPAWN_NPC = 2,
	MSG_SET_TIME = 3,
	MSG_SWITCH_NOCLIP = 4,
	MSG_SWITCH_GODMODE = 5,
	MSG_SWITCH_NOSPAWNS = 6;
public static int modId;

public static Texture2D
	texItem, texPrefix, texNPC, texBuff, texMisc,
	texAUp, texAUp2, texAUp3, texADown, texADown2, texADown3,
	texALeft, texALeft2, texALeft3, texARight, texARight2, texARight3,
	texItemBlank, texNPCBlank, texNPCBlank2, texBuffBlank,
	texBuffPos, texBuffNeg,
	texClock, texClock2, texClock3;
public static bool resetChat = false;

public static bool[] enabledNoclip, enabledGodmode;
public static bool enabledAllLight, enabledNoSpawns;

public void Initialize(int modId) {
	ModWorld.modId = modId;
	if (!Main.dedServ) return;
	Init();
}
public static void Init() {
	GuiItem.Init();
	GuiPrefix.Init();
	GuiNPC.Init();
	GuiBuff.Init();
	GuiMisc.Init();
	resetChat = false;
	CheatNotification.Init();
	
	enabledNoclip = new bool[Main.player.Length];
	for (int i = 0; i < enabledNoclip.Length; i++) enabledNoclip[i] = false;
	
	enabledGodmode = new bool[Main.player.Length];
	for (int i = 0; i < enabledGodmode.Length; i++) enabledGodmode[i] = false;
	
	enabledAllLight = false;
	enabledNoSpawns = false;
	
	if (Main.dedServ) return;
	
	texItem = Main.goreTexture[Config.goreID["FCM_Item"]];
	texPrefix = Main.goreTexture[Config.goreID["FCM_Prefix"]];
	texNPC = Main.goreTexture[Config.goreID["FCM_NPC"]];
	texBuff = Main.goreTexture[Config.goreID["FCM_Buff"]];
	texMisc = Main.goreTexture[Config.goreID["FCM_Misc"]];
	
	texAUp = Main.goreTexture[Config.goreID["FCM_Arrow_Up"]];
	texAUp2 = Main.goreTexture[Config.goreID["FCM_Arrow_Up2"]];
	texAUp3 = Main.goreTexture[Config.goreID["FCM_Arrow_Up3"]];
	texADown = Main.goreTexture[Config.goreID["FCM_Arrow_Down"]];
	texADown2 = Main.goreTexture[Config.goreID["FCM_Arrow_Down2"]];
	texADown3 = Main.goreTexture[Config.goreID["FCM_Arrow_Down3"]];
	
	texALeft = Main.goreTexture[Config.goreID["FCM_Arrow_Left"]];
	texALeft2 = Main.goreTexture[Config.goreID["FCM_Arrow_Left2"]];
	texALeft3 = Main.goreTexture[Config.goreID["FCM_Arrow_Left3"]];
	texARight = Main.goreTexture[Config.goreID["FCM_Arrow_Right"]];
	texARight2 = Main.goreTexture[Config.goreID["FCM_Arrow_Right2"]];
	texARight3 = Main.goreTexture[Config.goreID["FCM_Arrow_Right3"]];
	
	texItemBlank = Main.goreTexture[Config.goreID["FCM_ItemBlank"]];
	texNPCBlank = Main.goreTexture[Config.goreID["FCM_NPCBlank"]];
	texNPCBlank2 = Main.goreTexture[Config.goreID["FCM_NPCBlank2"]];
	texBuffBlank = Main.goreTexture[Config.goreID["FCM_BuffBlank"]];
	
	texBuffPos = Main.goreTexture[Config.goreID["FCM_Buff_Positive"]];
	texBuffNeg = Main.goreTexture[Config.goreID["FCM_Buff_Negative"]];
	
	texClock = Main.goreTexture[Config.goreID["FCM_Clock"]];
	texClock2 = Main.goreTexture[Config.goreID["FCM_Clock2"]];
	texClock3 = Main.goreTexture[Config.goreID["FCM_Clock3"]];
}

public void PlayerConnected(int playerID) {
	foreach (Player player in Main.player) {
		if (player == null || !player.active || player.name == "") continue;
		if (playerID == player.whoAmi) continue;
		
		if (enabledNoclip[player.whoAmi]) NetMessage.SendModData(modId,MSG_SWITCH_NOCLIP,playerID,-1,player.whoAmi);
		if (enabledGodmode[player.whoAmi]) NetMessage.SendModData(modId,MSG_SWITCH_GODMODE,playerID,-1,player.whoAmi);
	}
	enabledNoclip[playerID] = false;
	enabledGodmode[playerID] = false;
	if (enabledNoSpawns) NetMessage.SendModData(modId,MSG_SWITCH_NOSPAWNS,playerID,-1);
	NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,playerID,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
}
public void NetReceive(int messageType, BinaryReader br) {
	if (Main.netMode == 1) {
		switch (messageType) {
			case MSG_SET_TIME: {
				Main.time = br.ReadInt32();
				Main.dayTime = br.ReadBoolean();
				Main.moonPhase = (int)br.ReadByte();
				Main.bloodMoon = br.ReadBoolean();
				Main.hardMode = br.ReadBoolean();
				Main.dayRate = br.ReadInt32();
			} break;
			case MSG_SWITCH_NOCLIP: {
				int pID = (int)br.ReadByte();
				enabledNoclip[pID] = !enabledNoclip[pID];
			} break;
			case MSG_SWITCH_GODMODE: {
				int pID = (int)br.ReadByte();
				enabledGodmode[pID] = !enabledGodmode[pID];
			} break;
			case MSG_SWITCH_NOSPAWNS: {
				enabledNoSpawns = !enabledNoSpawns;
			} break;
			default: break;
		}
	} else if (Main.netMode == 2) {
		switch (messageType) {
			case MSG_CHEAT_NOTIFICATION: {
				int pID = (int)br.ReadByte();
				string msg = br.ReadString();
				NetMessage.SendData(25,-1,-1,"[FCM] ("+Main.player[pID].name+") "+msg,255,255f,0f,0f,0);
			} break;
			case MSG_SPAWN_NPC: {
				Vector2 startSpawn = new Vector2(br.ReadSingle(),br.ReadSingle());
				float radius = br.ReadSingle();
				float field = (float)(Math.PI*Math.Pow(radius,2));
				int amount = (int)Math.Max(1,field/2000);
				string npcName = br.ReadString();
				int netId = br.ReadInt32();
				
				Random rnd = new Random();
				for (int i = 0; i < amount; i++) {
					Vector2 v = amount == 1 ? startSpawn : startSpawn+GuiCheat.Util.Vector((float)(radius*rnd.NextDouble()),(float)(rnd.NextDouble()*360d));
					Main.npc[NPC.NewNPC((int)v.X,(int)v.Y,npcName)].netDefaults(netId);
				}
			} break;
			case MSG_SET_TIME: {
				Main.time = br.ReadInt32();
				Main.dayTime = br.ReadBoolean();
				Main.moonPhase = (int)br.ReadByte();
				Main.bloodMoon = br.ReadBoolean();
				Main.hardMode = br.ReadBoolean();
				Main.dayRate = br.ReadInt32();
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SET_TIME,-1,-1,(int)Main.time,Main.dayTime,(byte)Main.moonPhase,Main.bloodMoon,Main.hardMode,Main.dayRate);
			} break;
			case MSG_SWITCH_NOCLIP: {
				int pID = (int)br.ReadByte();
				enabledNoclip[pID] = !enabledNoclip[pID];
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_NOCLIP,-1,pID,(byte)pID);
			} break;
			case MSG_SWITCH_GODMODE: {
				int pID = (int)br.ReadByte();
				enabledGodmode[pID] = !enabledGodmode[pID];
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_GODMODE,-1,pID,(byte)pID);
			} break;
			case MSG_SWITCH_NOSPAWNS: {
				int pID = (int)br.ReadByte();
				enabledNoSpawns = !enabledNoSpawns;
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_SWITCH_NOSPAWNS,-1,pID);
			} break;
			default: break;
		}
	}
}

public static bool IsBlankItem(Item item) {
	return item == null || item.type == 0 || item.name == null || item.name == "" || item.name == "Unloaded Item" || item.stack <= 0;
}
public static Item CloneItem(Item item) {
	Item ret = new Item();
	if (IsBlankItem(item)) return ret;
	
	ret.SetDefaults(item.name);
	ret.stack = item.stack;
	ret.Prefix(item.prefix);
	
	MemoryStream ms = new MemoryStream();
	BinaryWriter bw = new BinaryWriter(ms);
	Prefix.SavePrefix(bw,item);
	Codable.SaveCustomData(item,bw);
	
	ms.Seek(0,SeekOrigin.Begin);
	BinaryReader br = new BinaryReader(ms);
	Prefix.LoadPrefix(br,ret,"player");
	Codable.LoadCustomData(ret,br,5,true);
	
	return ret;
}

public static void MouseText(String text) {
	Main.toolTip = new Item();
	Main.buffString = "";
	Config.mainInstance.MouseText(text);
}
public static void ItemMouseText(Item item) {
	if (item == null || item.type == 0) return;
	string tip = item.name;
	Main.toolTip = CloneItem(item);
	
	if (item.stack > 1) tip += " ("+item.stack+")";
	Config.mainInstance.MouseText(tip,item.rare,0);
}

public bool PreDrawInterface(SpriteBatch sb) {
	Player player = Main.player[Main.myPlayer];
	
	if (resetChat) Main.chatMode = false;
	resetChat = false;
	
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCheat) {
		Config.tileInterface.SetLocation(new Vector2(player.position.X/16f,player.position.Y/16f));
		((GuiCheat)Config.tileInterface.code).PreDrawInterface(sb);
	}
	
	if (Main.playerInventory) {
		Color c;
		Vector2 v;
		int xx = 0;
		
		c = Config.tileInterface != null && Config.tileInterface.code is GuiItem ? Color.White : Color.Gray;
		v = new Vector2(8+xx,Main.screenHeight-8-texItem.Height);
		sb.Draw(texItem,v,GetTexRectangle(texItem),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		xx += texItem.Width+2;
		
		c = Config.tileInterface != null && Config.tileInterface.code is GuiPrefix ? Color.White : Color.Gray;
		v = new Vector2(8+xx,Main.screenHeight-8-texPrefix.Height);
		sb.Draw(texPrefix,v,GetTexRectangle(texPrefix),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		xx += texPrefix.Width+2;
		
		c = Config.tileInterface != null && Config.tileInterface.code is GuiNPC ? Color.White : Color.Gray;
		v = new Vector2(8+xx,Main.screenHeight-8-texNPC.Height);
		sb.Draw(texNPC,v,GetTexRectangle(texNPC),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		xx += texNPC.Width+2;
		
		c = Config.tileInterface != null && Config.tileInterface.code is GuiBuff ? Color.White : Color.Gray;
		v = new Vector2(8+xx,Main.screenHeight-8-texBuff.Height);
		sb.Draw(texBuff,v,GetTexRectangle(texBuff),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		xx += texBuff.Width+2;
		
		c = Config.tileInterface != null && Config.tileInterface.code is GuiMisc ? Color.White : Color.Gray;
		v = new Vector2(8+xx,Main.screenHeight-8-texMisc.Height);
		sb.Draw(texMisc,v,GetTexRectangle(texMisc),c,0f,default(Vector2),1f,SpriteEffects.None,0f);
		xx += texMisc.Width+2;
	}
	
	return true;
}
public void PostDraw(SpriteBatch sb) {
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCheat) {
		((GuiCheat)Config.tileInterface.code).PostDraw(sb);
	}
	
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCheat) {
		if (((GuiCheat)Config.tileInterface.code).PretendChat()) {
			resetChat = true;
			Main.chatMode = true;
			Main.inputTextEnter = Main.chatRelease = false;
		}
	}
	
	if (Main.playerInventory) {
		Vector2 v;
		int xx = 0;
		
		v = new Vector2(8+xx,Main.screenHeight-8-texItem.Height);
		if (MouseRegion(v,new Vector2(texItem.Width,texItem.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Items");
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (Config.tileInterface != null && Config.tileInterface.code is GuiItem) Config.tileInterface = null;
				else GuiItem.Create();
			}
		}
		xx += texItem.Width+2;
		
		v = new Vector2(8+xx,Main.screenHeight-8-texPrefix.Height);
		if (MouseRegion(v,new Vector2(texPrefix.Width,texPrefix.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Prefixes");
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (Config.tileInterface != null && Config.tileInterface.code is GuiPrefix) Config.tileInterface = null;
				else GuiPrefix.Create();
			}
		}
		xx += texPrefix.Width+2;
		
		v = new Vector2(8+xx,Main.screenHeight-8-texNPC.Height);
		if (MouseRegion(v,new Vector2(texNPC.Width,texNPC.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("NPCs");
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (Config.tileInterface != null && Config.tileInterface.code is GuiNPC) Config.tileInterface = null;
				else GuiNPC.Create();
			}
		}
		xx += texNPC.Width+2;
		
		v = new Vector2(8+xx,Main.screenHeight-8-texBuff.Height);
		if (MouseRegion(v,new Vector2(texBuff.Width,texBuff.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Buffs");
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (Config.tileInterface != null && Config.tileInterface.code is GuiBuff) Config.tileInterface = null;
				else GuiBuff.Create();
			}
		}
		xx += texBuff.Width+2;
		
		v = new Vector2(8+xx,Main.screenHeight-8-texMisc.Height);
		if (MouseRegion(v,new Vector2(texMisc.Width,texMisc.Height))) {
			Main.player[Main.myPlayer].mouseInterface = true;
			MouseText("Misc");
			if (Main.mouseLeft && Main.mouseLeftRelease) {
				if (Config.tileInterface != null && Config.tileInterface.code is GuiMisc) Config.tileInterface = null;
				else GuiMisc.Create();
			}
		}
		xx += texMisc.Width+2;
	}
}

public void ModifyLightVision(float[] negLightRef) {
	if (enabledAllLight) {
		negLightRef[0] = 1f;
		negLightRef[1] = 1f;
	}
}

public bool PreDrawAvailableRecipes(SpriteBatch sb) {
    Dictionary<int,int> A = new Dictionary<int,int>();
    A.Add(0,0);
    Codable.RunGlobalMethod("ModWorld","AnySinisterMenus",A);
    return A[0] <= 0;
}
public static void AnySinisterMenus(Dictionary<int,int> A) {
	if (Config.tileInterface != null && Config.tileInterface.code is GuiCheat) A[0]++;
}

public static bool MouseRegion(int x, int y, int w, int h) {
	return Main.mouseX >= x && Main.mouseY >= y && Main.mouseX < x+w && Main.mouseY < y+h;
}
public static bool MouseRegion(Vector2 v1, Vector2 v2) {
	return Main.mouseX >= v1.X && Main.mouseY >= v1.Y && Main.mouseX < v1.X+v2.X && Main.mouseY < v1.Y+v2.Y;
}

public static bool MouseRegionCircle(int x, int y, float r) {
	return Math.Sqrt(Math.Pow(x-Main.mouseX,2)+Math.Pow(y-Main.mouseY,2)) <= r;
}
public static bool MouseRegionCircle(Vector2 v, float r) {
	return Math.Sqrt(Math.Pow(v.X-Main.mouseX,2)+Math.Pow(v.Y-Main.mouseY,2)) <= r;
}

public static Rectangle? GetTexRectangle(Texture2D tex) {
	return new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height));
}
public static Vector2 GetTexCenter(Texture2D tex) {
	return new Vector2(tex.Width/2f,tex.Height/2f);
}