#INCLUDE "GuiSpitter.cs"

public static bool[] ghosts = new bool[Main.player.Length];
public static bool wasGhost = false;
public static InterfaceObj lastGui = null;

public bool changeDiff = false, usedStone = false, makeMeAGhost = false;
public Item lastMouseItem = null;

public bool chargeSnowball = false;
public int lastSelectedItem = -1;
public int powerSnowball = 0;
public float missAngle = 0;

public static bool HasItem(Player player, Item item) {
	if (player == null) return false;
	if (item == null || item.name == null || item.name == "" || item.stack <= 0) return true;
	
	int count = item.stack;
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] != null && player.inventory[i].name != null && player.inventory[i].name == item.name) count -= player.inventory[i].stack;
		if (count <= 0) return true;
	}
	return false;
}
public static bool EatItem(Player player, Item item) {
	if (player == null) return false;
	if (item == null || item.name == null || item.name == "" || item.stack <= 0) return true;
	
	if (HasItem(player,item)) {
		int count = item.stack;
		for (int i = 0; i <= 39; i++) {
			if (player.inventory[i] != null && player.inventory[i].name != null && player.inventory[i].name == item.name) {
				int diff = Math.Min(count,player.inventory[i].stack);
				count -= diff;
				player.inventory[i].stack -= diff;
				if (player.inventory[i].stack == 0) player.inventory[i].SetDefaults(0);
			}
			if (count == 0) return true;
		}
	}
	return false;
}
public static bool HasFreeStack(Player player) {
	if (player == null) return false;
	
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) return true;
	}
	return false;
}
public static bool HasPlaceFor(Player player, Item item) {
	if (player == null) return false;
	if (item == null || item.name == null || item.name == "" || item.stack <= 0) return true;
	
	int place = 0;
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) place += item.maxStack;
		else if (player.inventory[i].name == item.name) place += player.inventory[i].maxStack-player.inventory[i].stack;
	}
	
	return place >= item.stack;
}
public static void GiveItem(Player player, Item item) {
	if (player == null) return;
	if (item == null || item.name == null || item.name == "" || item.stack <= 0) return;
	
	int give = item.stack;
	
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i].name == item.name) {
			int diff = Math.Min(player.inventory[i].maxStack-player.inventory[i].stack,give);
			player.inventory[i].stack += diff;
			give -= diff;
		}
		if (give == 0) break;
	}
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) {
			int diff = Math.Min(item.maxStack,give);
			player.inventory[i].SetDefaults(item.name);
			player.inventory[i].stack = diff;
			give -= diff;
		}
		if (give == 0) break;
	}
	
	Main.PlaySound(7,-1,-1,1);
}

public void Initialize() {
	lastMouseItem = null;
	wasGhost = false;
	changeDiff = false;
	usedStone = false;
	makeMeAGhost = false;
	lastGui = null;
	powerSnowball = 0;
	missAngle = 0f;
	lastSelectedItem = -1;
	chargeSnowball = false;
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
	ModWorld.ExternalInitAchievementsDelegates(AddAchievement,ConfigNetMode,ConfigDifficulty,ConfigHardmode,ConfigProgress,GetAchieved,Achieve,AchievePlayer,AchieveAllPlayers,GetProgress,SetProgress,Progress,ProgressPlayer,ProgressAllPlayers,GetAchievementInfo);
}

public void Save(BinaryWriter bw) {
	/*if (Main.creatingChar) {
		bw.Write(false);
	} else {*/
        if(ModWorld.JustQuitted)
        {
		    Player player = Main.player[Main.myPlayer];
		    bw.Write(player.ghost);
            ModWorld.JustQuitted = false;
        }
        else bw.Write(false);
	//}
}
public void Load(BinaryReader br, int version) {
	makeMeAGhost = br.ReadBoolean();
}

public void UpdatePlayer(Player player) {
	if (player == null) return;
	
    TakeCareOfSuperSlipperyIce(player);
	if (player.grapCount > 0) {
		int tileIce = Config.tileDefs.ID["Icemaw"];
		for (int i = 0; i < player.grapCount; i++) {
			if (player.grappling[i] < 0) continue;
			
			Projectile proj = Main.projectile[player.grappling[i]];
			int xx = (int)((proj.position.X+proj.width/2f)/16f), yy = (int)((proj.position.Y+proj.height/2f)/16f);
			if (xx >= 0 && xx < Main.maxTilesX && yy >= 0 && yy < Main.maxTilesY && Main.tile[xx,yy].active && Main.tile[xx,yy].type == tileIce) {
				for (int j = i; j < player.grapCount-1; j++) player.grappling[j] = player.grappling[j+1];
				player.grappling[player.grapCount-1] = -1;
				player.grapCount--;
				proj.Kill();
			}
		}
	}
	
	if (player.whoAmi != Main.myPlayer) return;
	
	for (int i = 0; i <= 39; i++) {
		if (player.inventory[i] == null || player.inventory[i].name == null || player.inventory[i].name == "" || player.inventory[i].stack <= 0) continue;
		if (player.inventory[i].name == "Present") {
			if (player.inventory[i].RunMethod("ExternalGetPresentPlayer",player.name,Main.mouseItem)) {
				string playerName = (string)Codable.customMethodReturn;
				if (playerName == player.name) continue;
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_OPENEDPRESENT,-1,-1,playerName);
			}
		}
	}
	
	if (lastGui != null && (Config.tileInterface == null || !Object.ReferenceEquals(lastGui,Config.tileInterface))) {
		if (lastGui.code is GuiSpitter) {
			if (Main.netMode == 0) {
				List<Item> items = ModWorld.GetCustomItems(lastGui.sourceLocation);
				if (items == null) items = new List<Item>();
				items.Clear();
				for (int i = 0; i < lastGui.itemSlots.Length-1; i++) items.Add(lastGui.itemSlots[i]);
			} else if (Main.netMode == 1) {
				MemoryStream ms = new MemoryStream();
				BinaryWriter bw = new BinaryWriter(ms);
				
				bw.Write((int)lastGui.sourceLocation.X);
				bw.Write((int)lastGui.sourceLocation.Y);
				
				bw.Write((byte)(lastGui.itemSlots.Length-1));
				for (int i = 0; i < lastGui.itemSlots.Length-1; i++) ModGeneric.ItemSave(bw,lastGui.itemSlots[i]);
				
				byte[] data = ms.ToArray();
				object[] toSend = new object[data.Length];
				for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_GUI_CLOSE,-1,-1,toSend);
			}
		}
	}
	lastGui = Config.tileInterface;
	
	if (makeMeAGhost) {
		player.ghost = player.dead = true;
		makeMeAGhost = false;
	}
	
	if (changeDiff) {
		player.difficulty = 2;
		if (!usedStone) {
			player.dead = player.ghost = true;
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_GHOST,-1,-1,(byte)player.whoAmi);
		}
        else
        {
            for(int i = 0; i < 20; i++)
            {
                int d =Dust.NewDust(player.position+new Vector2(-4,4),player.width+8,player.height+8,66,0,-2f,100,new Color(180,180, Main.DiscoB),2.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity*=3f;
            }
        }
		changeDiff = false;
		usedStone = false;
		if (player.ghost) return;
	}
	
	Item item = new Item();
	item.SetDefaults("Divine Stone of Returning Soul");
	bool hasItem = HasItem(player,item);
	foreach (Player p in Main.player) {
		if (p == null || p.name == "" || p.whoAmi == Main.myPlayer) continue;
		p.ghost = player.ghost ? true : (hasItem ? ModPlayer.ghosts[p.whoAmi] : false);
	}
	
	if (Main.playerInventory) {
		if (lastMouseItem != null && lastMouseItem.stack > 0 && lastMouseItem.name != null && lastMouseItem.name == "Wrapping Paper") {
			if (Main.mouseItem != null && Main.mouseItem.stack > 0 && Main.mouseItem.name != null && Main.mouseItem.name != "") {
				for (int i = 0; i <= 39; i++) if (Object.ReferenceEquals(player.inventory[i],lastMouseItem)) {
					player.inventory[i] = new Item();
					player.inventory[i].SetDefaults("Present");
					player.inventory[i].RunMethod("ExternalSetPresentContents",player.name,Main.mouseItem);
					Main.mouseItem = new Item();
					break;
				}
			}
		}
		lastMouseItem = Main.mouseItem;
	} else {
		lastMouseItem = null;
	}
}

public bool PreKill(Player player, double dmg, int hitDirection, bool pvp, string deathText) {
	if (player == null || player.whoAmi != Main.myPlayer) return true;
	
	if (player.difficulty == 2) {
		changeDiff = true;
		player.difficulty = 1;
		
		Item item = new Item();
		item.SetDefaults("Divine Stone of Returning Soul");
		usedStone = EatItem(player,item);
	}
	return true;
}

public void TakeCareOfSuperSlipperyIce(Player P)
{
    int[] A = { Config.tileDefs.ID["Icemaw"] };
    if(!DetectTileCollision(P.position+new Vector2(0,-5f)+new Vector2(0,P.height/2f)+new Vector2(0,P.height/2f)*P.gravDir,P.width,10,0,A)) return;
    
    P.baseSlideFactor=0.01f;
}


#region Detect Tiles Method

public static bool DetectTileCollision(Vector2 Position, int Width, int Height,int Radius,int[] DetectTargets)
{
    int LowX = (int)(Position.X / 16f) - Radius;
    int HighX = (int)((Position.X + (float)Width) / 16f) + Radius;
    int LowY = (int)(Position.Y / 16f) - Radius;
    int HighY = (int)((Position.Y + (float)Height) / 16f) + Radius;
    if (LowX < 0)
    {
        LowX = 0;
    }
    if (HighX > Main.maxTilesX)
    {
        HighX = Main.maxTilesX;
    }
    if (LowY < 0)
    {
        LowY = 0;
    }
    if (HighY > Main.maxTilesY)
    {
        HighY = Main.maxTilesY;
    }
    for (int i = LowX; i <= HighX; i++)
    {
        for (int j = LowY; j <= HighY; j++)
        {
            if (Main.tile[i, j] != null && Main.tile[i, j].active && IsATargetTile(Main.tile[i,j].type,DetectTargets))
            {
                return true;
            }
        }
    }
    return false;
}

public static bool IsATargetTile(int x,int[] t)
{
    foreach(int y in t) if(x==y) return true;
    return false;
}

#endregion