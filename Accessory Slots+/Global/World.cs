#INCLUDE "GuiAccessorySlots.cs"

public const int
	MSG_ITEM = 1,
	MSG_REQUEST = 2;

public static InterfaceObj slots = new InterfaceObj(new GuiAccessorySlots(),ModGeneric.extraSlots+1,0);
public static int modId;

public static Item[][] accessories;

static ModWorld() {
	int x = Main.screenWidth-139, y = 364;
	for (int i = 0; i < ModGeneric.extraSlots; i++) slots.AddItemSlot(x-i/3*48,y+(i%3)*48);
	slots.AddItemSlot(-100,-100);
	
	accessories = new Item[Main.player.Length][];
	for (int i = 0; i < accessories.Length; i++) {
		accessories[i] = new Item[ModGeneric.extraSlots];
		for (int j = 0; j < accessories[i].Length; j++) accessories[i][j] = new Item();
	}
}

public static void Initialize(int modId) {
	ModWorld.modId = modId;
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

public static void PlayerConnected(int playerID) {
	for (int i = 0; i < accessories[playerID].Length; i++) accessories[playerID][i] = new Item();
	
	for (int i = 0; i < Main.player.Length; i++) {
		if (playerID == i) continue;
		Player player = Main.player[i];
		if (player == null) continue;
		
		SendItemData(i,accessories[i],playerID,-1);
	}
	
	NetMessage.SendModData(modId,MSG_REQUEST,-1,Main.myPlayer);
}
public static void NetReceive(int messageType, BinaryReader br) {
	switch (messageType) {
		case MSG_ITEM: {
			int playerID = (int)br.ReadByte();
			for (int i = 0; i < ModGeneric.extraSlots; i++) accessories[playerID][i] = ModPlayer.ItemLoad(br);
			if (Main.netMode == 2) SendItemData(playerID,accessories[playerID],-1,playerID);
		} break;
		case MSG_REQUEST: {
			SendItemData(Main.myPlayer,ExternalGetAccessorySlots(),-1,-1);
		} break;
	}
}

public static void SendItemData(int playerID, Item[] items, int remoteClient, int ignoreClient) {
	MemoryStream ms = new MemoryStream();
	BinaryWriter bw = new BinaryWriter(ms);
	
	for (int i = 0; i < items.Length; i++) ModPlayer.ItemSave(bw,items[i]);
	byte[] data = ms.ToArray();
	object[] toSend = new object[data.Length+1];
	toSend[0] = (byte)playerID;
	for (int i = 0; i < data.Length; i++) toSend[i+1] = data[i];
	NetMessage.SendModData(modId,MSG_ITEM,remoteClient,ignoreClient,toSend);
}

public static bool PreDrawPlayerEquipment(SpriteBatch spriteBatch) {
	if (!Main.playerInventory || Config.mainInstance.showNPCs) return true;
	Player player = Main.player[Main.myPlayer];
	slots.SetLocation(new Vector2(player.position.X/16,player.position.Y/16));
	
	UpdateSlotsPositions();
	string sEmpty = string.Empty;
	slots.Draw(ref sEmpty);
	
	return true;
}
public static void PostDraw(SpriteBatch spriteBatch) {
	if (!Main.playerInventory || Config.mainInstance.showNPCs) return;
	Player player = Main.player[Main.myPlayer];
	
	for (int i = 0; i < ModGeneric.extraSlots; i++) {
		float scale = .85f;
		Vector2 pos = slots.slotLocation[i];
		if (Main.mouseX >= pos.X && Main.mouseX <= pos.X+Main.inventoryBackTexture.Width*scale && Main.mouseY >= pos.Y && Main.mouseY <= pos.Y+Main.inventoryBackTexture.Height*scale) {
			player.mouseInterface = true;
			ItemMouseText(ModWorld.slots.itemSlots[i]);
		}
	}
}
public static void ItemMouseText(Item item) {
	if (item == null || item.type == 0) return;
	string tip = item.name;
	Main.toolTip = (Item)item.Clone();
	
	if (item.stack > 1) tip += " ("+item.stack+")";
	Config.mainInstance.MouseText(tip,item.rare,0);
}

public static void UpdateSlotsPositions() {
	int x = Main.screenWidth-139;
	for (int i = 0; i < ModGeneric.extraSlots; i++) slots.slotLocation[i].X = x-i/3*48;
}

public static Item[] ExternalGetAccessorySlots() {
	Item[] ret = new Item[ModGeneric.extraSlots];
	for (int i = 0; i < ModGeneric.extraSlots; i++) ret[i] = (Item)slots.itemSlots[i].Clone();
	return ret;
}