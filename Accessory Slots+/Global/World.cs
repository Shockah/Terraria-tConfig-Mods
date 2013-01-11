#INCLUDE "Pair.cs"
#INCLUDE "GuiAccessorySlots.cs"

public const int
	MSG_ITEM = 1,
	MSG_ITEMPACK = 2,
	MSG_REQUEST = 3;

public static Func<string,Texture2D,bool> AcAchieve;

public static int modId;
public static InterfaceObj gui;
public static Item[][] accessories;

public void Initialize(int modId) {
	ModWorld.modId = modId;
	if (!Main.dedServ) return;
	Init();
}
public static void Init() {
	if (!Main.dedServ) {
		gui = new InterfaceObj(new GuiAccessorySlots(),ModGeneric.extraSlots+1,0);
		int x = Main.screenWidth-139, y = 364;
		for (int i = 0; i < ModGeneric.extraSlots; i++) gui.AddItemSlot(x-i/3*48,y+(i%3)*48);
		gui.AddItemSlot(-100,-100);
	}
	
	accessories = new Item[Main.player.Length][];
	for (int i = 0; i < accessories.Length; i++) {
		accessories[i] = new Item[ModGeneric.extraSlots];
		for (int j = 0; j < accessories[i].Length; j++) accessories[i][j] = new Item();
	}
	
	Codable.RunGlobalMethod("ModWorld","ExternalInitAccessoriesDelegates",new object[]{
		(Func<int>)ExternalGetNumAccessorySlots,
		(Func<Item[]>)ExternalGetAccessorySlots,
		(Func<int,Item[]>)ExternalGetAccessorySlotsFor,
		(Action<int,Item>)ExternalSetAccessorySlot,
		(Action<int,int,Item>)ExternalSetAccessorySlotFor
	});
}

public void PlayerConnected(int playerID) {
	foreach (Player player in Main.player) {
		if (player == null || !player.active || player.name == "") continue;
		if (playerID == player.whoAmi) continue;
		
		MemoryStream ms = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(ms);
		
		bw.Write((byte)player.whoAmi);
		foreach (Item item in accessories[player.whoAmi]) ItemSave(bw,item);
		
		byte[] data = ms.ToArray();
		object[] toSend = new object[data.Length];
		for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
		NetMessage.SendModData(modId,MSG_ITEMPACK,playerID,-1,toSend);
	}
	
	NetMessage.SendModData(modId,MSG_REQUEST,playerID,-1);
}
public void NetReceive(int messageType, BinaryReader br) {
	if (Main.netMode == 1) {
		switch (messageType) {
			case MSG_ITEM: {
				int playerID = br.ReadByte();
				int slot = br.ReadByte();
				accessories[playerID][slot].RunMethod("OnUnequip",Main.player[playerID],-slot-1);
				accessories[playerID][slot] = ItemLoad(br);
				accessories[playerID][slot].RunMethod("OnEquip",Main.player[playerID],-slot-1);
			} break;
			case MSG_ITEMPACK: {
				int playerID = br.ReadByte();
				for (int i = 0; i < ModGeneric.extraSlots; i++) {
					accessories[playerID][i].RunMethod("OnUnequip",Main.player[playerID],-i-1);
					accessories[playerID][i] = ItemLoad(br);
					accessories[playerID][i].RunMethod("OnEquip",Main.player[playerID],-i-1);
				}
			} break;
			case MSG_REQUEST: {
				MemoryStream ms = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(ms);
				
				bw.Write((byte)Main.myPlayer);
				foreach (Item item in accessories[Main.myPlayer]) ItemSave(bw,item);
				
				byte[] data = ms.ToArray();
				object[] toSend = new object[data.Length];
				for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
				NetMessage.SendModData(modId,MSG_ITEMPACK,-1,-1,toSend);
			} break;
			default: break;
		}
	} else if (Main.netMode == 2) {
		switch (messageType) {
			case MSG_ITEM: {
				int playerID = br.ReadByte();
				int slot = br.ReadByte();
				accessories[playerID][slot].RunMethod("OnUnequip",Main.player[playerID],-slot-1);
				accessories[playerID][slot] = ItemLoad(br);
				accessories[playerID][slot].RunMethod("OnEquip",Main.player[playerID],-slot-1);
				
				MemoryStream ms = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(ms);
				
				bw.Write((byte)playerID);
				bw.Write((byte)slot);
				ItemSave(bw,accessories[playerID][slot]);
				
				byte[] data = ms.ToArray();
				object[] toSend = new object[data.Length];
				for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
				NetMessage.SendModData(modId,MSG_ITEM,-1,playerID,toSend);
			} break;
			case MSG_ITEMPACK: {
				int playerID = br.ReadByte();
				for (int i = 0; i < ModGeneric.extraSlots; i++) {
					accessories[playerID][i].RunMethod("OnUnequip",Main.player[playerID],-i-1);
					accessories[playerID][i] = ItemLoad(br);
					accessories[playerID][i].RunMethod("OnEquip",Main.player[playerID],-i-1);
				}
				
				MemoryStream ms = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(ms);
				
				bw.Write((byte)playerID);
				foreach (Item item in accessories[playerID]) ItemSave(bw,item);
				
				byte[] data = ms.ToArray();
				object[] toSend = new object[data.Length];
				for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
				NetMessage.SendModData(modId,MSG_ITEMPACK,-1,playerID,toSend);
			} break;
			default: break;
		}
	}
}

public static void ItemSave(BinaryWriter bw, Item item) {
	if (item == null) item = new Item();
	bw.Write(item.type != 0);
	if (item.type != 0) {
		bw.Write(item.name);
		bw.Write((byte)item.stack);
		bw.Write((byte)item.prefix);
		Prefix.SavePrefix(bw,item);
		Codable.SaveCustomData(item,bw);
	}
}
public static Item ItemLoad(BinaryReader br) {
	Item item = new Item();
	try {
		if (!br.ReadBoolean()) return item;
		item.SetDefaults(br.ReadString());
		item.stack = (int)br.ReadByte();
		item.Prefix((int)br.ReadByte());
		Prefix.LoadPrefix(br,item,"player");
		Codable.LoadCustomData(item,br,5,true);
		return item;
	} catch (Exception) {
		return new Item();
	}
}
public static bool IsBlankItem(Item item) {
	return item == null || item.type == 0 || item.name == null || item.name == "" || item.name == "Unloaded Item" || item.stack <= 0;
}
public static Item CloneItem(Item item) {
	Item ret = new Item();
	if (IsBlankItem(item)) return ret;
	
	ret.SetDefaults(item.type);
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

public bool PreDrawPlayerEquipment(SpriteBatch sb) {
	if (!Main.playerInventory || Config.mainInstance.showNPCs) return true;
	Player player = Main.player[Main.myPlayer];
	gui.SetLocation(new Vector2(player.position.X/16,player.position.Y/16));
	
	string sEmpty = "";
	gui.Draw(ref sEmpty);
	return true;
}
public void PostDraw(SpriteBatch sb) {
	if (!Main.playerInventory || Config.mainInstance.showNPCs) return;
	Player player = Main.player[Main.myPlayer];
	
	for (int i = 0; i < ModGeneric.extraSlots; i++) {
		float scale = .85f;
		Vector2 pos = gui.slotLocation[i];
		if (Main.mouseX >= pos.X && Main.mouseX <= pos.X+Main.inventoryBackTexture.Width*scale && Main.mouseY >= pos.Y && Main.mouseY <= pos.Y+Main.inventoryBackTexture.Height*scale) {
			player.mouseInterface = true;
			ItemMouseText(accessories[player.whoAmi][i]);
		}
	}
}

public static void ItemMouseText(Item item) {
	if (item == null || item.type == 0) return;
	string tip = item.name;
	Main.toolTip = CloneItem(item);
	
	if (item.stack > 1) tip += " ("+item.stack+")";
	Config.mainInstance.MouseText(tip,item.rare,0);
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
	
	cat = "Shockah's mods->Accessory Slots+";
	s = "SHK_AS+_OVERKILL"; AddAchievement(s,cat,"Overkill!","Have an accessory equipped in each accessory slot.",null,20,Main.itemTexture[Config.itemDefs.byName["Anklet of the Wind"].type],false);
}

public static int ExternalGetNumAccessorySlots() {
	return ModGeneric.extraSlots;
}
public static Item[] ExternalGetAccessorySlots() {
	return ExternalGetAccessorySlotsFor(Main.myPlayer);
}
public static Item[] ExternalGetAccessorySlotsFor(int player) {
	if (player < 0) player = Main.myPlayer;
	return accessories[player];
}
public static void ExternalSetAccessorySlot(int slot, Item item) {
	ExternalSetAccessorySlotFor(Main.myPlayer,slot,item);
}
public static void ExternalSetAccessorySlotFor(int player, int slot, Item item) {
	if (player < 0) player = Main.myPlayer;
	accessories[player][slot] = item;
}