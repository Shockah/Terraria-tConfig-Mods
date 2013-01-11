public static int[] MUSIC_BOXES = new int[]{-1,0,1,2,4,5,-1,6,7,9,8,11,10,12};

public void Initialize() {
	if (!Main.dedServ) ModWorld.Init();
}

public void Save(BinaryWriter bw) {
	if (Main.creatingChar) {
		for (int i = 0; i < ModGeneric.extraSlots; i++) ModWorld.ItemSave(bw,new Item());
	} else {
		for (int i = 0; i < ModGeneric.extraSlots; i++) ModWorld.ItemSave(bw,ModWorld.accessories[Main.myPlayer][i]);
	}
}
public void Load(BinaryReader br, int version) {
	for (int i = 0; i < ModGeneric.extraSlots; i++) ModWorld.accessories[Main.myPlayer][i] = ModWorld.ItemLoad(br);
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

public bool CheckAchievement(Player player) {
	if (ModWorld.AcAchieve == null) return false;
	
	for (int i = 3; i <= 7; i++) if (ModWorld.IsBlankItem(player.armor[i])) return false;
	for (int i = 0; i < ModGeneric.extraSlots; i++) if (ModWorld.IsBlankItem(ModWorld.accessories[player.whoAmi][i])) return false;
	ModWorld.AcAchieve("SHK_AS+_OVERKILL",null);
	return true;
}

public void UpdatePlayer(Player player) {
	if (player == null || !player.active || player.name == "") return;
	if (player.whoAmi == Main.myPlayer) CheckAchievement(player);
	
	for (int i = 0; i < ModGeneric.extraSlots; i++) {
		Item acc = ModWorld.accessories[player.whoAmi][i];
		if (ModWorld.IsBlankItem(acc)) continue;
		
		player.statDefense += acc.defense;
		player.lifeRegen += acc.lifeRegen;
		if (acc.prefix > 0 && acc.prefix < Terraria.Prefix.prefixes.Count) Terraria.Prefix.prefixes[acc.prefix].Apply(player);
		acc.Effects(player);
		
		switch (acc.type) {
			case 238: player.magicDamage += .15f; break; //Wizard Hat
			case 111: player.statManaMax2 += 20; break; //Band of Starpower
			case 268: player.accDivingHelm = true; break; //Diving Helmet
			case 15: if (player.accWatch < 1) player.accWatch = 1; break; //Copper Watch
			case 16: if (player.accWatch < 2) player.accWatch = 2; break; //Silver Watch
			case 17: if (player.accWatch < 3) player.accWatch = 3; break; //Gold Watch
			case 18: if (player.accDepthMeter < 1) player.accDepthMeter = 1; break; //Depth Meter
			case 53: player.doubleJump = true; break; //Cloud in a Bottle
			case 54: if (player.baseMaxSpeed < 6f) player.baseMaxSpeed = 6f; break; //Hermes Boots
			case 128: if (player.rocketBoots == 0) player.rocketBoots = 1; break; //Rocket Boots
			case 156: player.noKnockback = true; break; //Cobalt Shield
			case 158: player.noFallDmg = true; break; //Lucky Horseshoe
			case 159: player.jumpBoost = true; break; //Shiny Red Balloon
			case 187: player.accFlipper = true; break; //Flipper
			case 211: player.meleeSpeed += .12f; break; //Feral Claws
			case 223: player.manaCost -= .06f; break; //Nature's Gift
			case 285: player.moveSpeed += .05f; break; //Aglet
			case 212: player.moveSpeed += .1f; break; //Anklet of the Wind
			case 267: player.killGuide = true; break; //Guide Voodoo Doll
			case 193: player.fireWalk = true; break; //Obsidian Skull
			case 485: player.wolfAcc = true; break; //Moon Charm
			case 486: player.rulerAcc = true; break; //Ruler
			case 393: player.accCompass = 1; break; //Compass
			case 394: { //Diving Gear
				player.accFlipper = true;
				player.accDivingHelm = true;
			} break;
			case 395: { //GPS
				player.accWatch = 3;
				player.accDepthMeter = 1;
				player.accCompass = 1;
			} break;
			case 396: { //Obsidian Horseshoe
				player.noFallDmg = true;
				player.fireWalk = true;
			} break;
			case 397: { //Obsidian Shield
				player.noKnockback = true;
				player.fireWalk = true;
			} break;
			case 399: { //Cloud in a Balloon
				player.jumpBoost = true;
				player.doubleJump = true;
			} break;
			case 405: { //Spectre Boots
				if (player.baseMaxSpeed < 6f) player.baseMaxSpeed = 6f;
				if (player.rocketBoots == 0) player.rocketBoots = 2;
			} break;
			case 407: if (player.blockRange < 1) player.blockRange = 1; break; //Toolbelt
			case 489: player.magicDamage += .15f; break; //Sorcerer Emblem
			case 490: player.meleeDamage += .15f; break; //Warrior Emblem
			case 491: player.rangedDamage += .15f; break; //Ranger Emblem
			case 492: if (player.wings == 0) player.wings = 1; break; //Demon Wings
			case 493: if (player.wings == 0) player.wings = 2; break; //Angel Wings
			case 497: player.accMerman = true; break; //Neptune's Shell
			case 535: player.pStone = true; break; //Philosopher's Stone
			case 536: player.kbGlove = true; break; //Titan Glove
			case 532: player.starCloak = true; break; //Star Cloak
			case 554: player.longInvince = true; break; //Cross Necklace
			case 555: { //Mana Flower
				player.manaFlower = true;
				player.manaCost -= .08f;
			} break;
			default: {
				if (Main.myPlayer == player.whoAmi) {
					if (acc.type == 576 && Main.rand.Next(18000) == 0 && Main.curMusic > 0) {
						int mus = Main.curMusic <= MUSIC_BOXES.Length-1 ? MUSIC_BOXES[Main.curMusic] : -1;
						ModWorld.accessories[player.whoAmi][i].SetDefaults(562+mus,false);
						
						MemoryStream ms = new MemoryStream();
						BinaryWriter bw = new BinaryWriter(ms);
						
						bw.Write((byte)player.whoAmi);
						ModWorld.ItemSave(bw,ModWorld.accessories[player.whoAmi][i]);
						
						byte[] data = ms.ToArray();
						object[] toSend = new object[data.Length];
						for (int j = 0; j < data.Length; j++) toSend[j] = data[j];
						NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_ITEM,-1,-1,toSend);
					}
					if (acc.type >= 562 && acc.type <= 574) Main.musicBox2 = acc.type-562;
				}
			} break;
		}
	}
}

public void PostKill(Player player, double dmg, int hitDirection, bool pvp, string deathText) {
	Item[] items = ModWorld.accessories[player.whoAmi];
	if (player.difficulty > 0) {
		for (int i = 0; i < items.Length; i++) {
			if (!ModWorld.IsBlankItem(items[i])) {
				items[i].RunMethod("OnUnequip",player,-i-1);
				if (Main.netMode != 1) {
					int id = Item.NewItem((int)player.position.X,(int)player.position.Y,player.width,player.height,items[i],false);
					Main.item[id].velocity.Y = Main.rand.Next(-20,1)*0.2f;
					Main.item[id].velocity.X = Main.rand.Next(-20,21)*0.2f;
					Main.item[id].noGrabDelay = 100;
				}
			}
			items[i] = new Item();
		}
	}
}