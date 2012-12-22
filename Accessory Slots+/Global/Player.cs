public static Func<string,Texture2D,bool> AcAchieve;

public static void Initialize() {
	for (int i = 0; i < ModGeneric.extraSlots; i++) ModWorld.slots.itemSlots[i] = new Item();
}
public static void CreatePlayer(Player p) {
	Initialize();
}
public static void Save(BinaryWriter bw) {
	for (int i = 0; i < ModGeneric.extraSlots; i++) ItemSave(bw,ModWorld.slots.itemSlots[i]);
}
public static void Load(BinaryReader br, int version) {
	for (int i = 0; i < ModGeneric.extraSlots; i++) ModWorld.slots.itemSlots[i] = ItemLoad(br);
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

public void CheckAc(Player player) {
	if (player == null) return;
	
	for (int i = 3; i <= 7; i++) {
		Item acc = player.armor[i];
		if (acc == null || acc.name == "" || acc.stack <= 0) return;
	}
	for (int i = 0; i < ModGeneric.extraSlots; i++) {
		Item acc = ModWorld.slots.itemSlots[i];
		if (acc == null || acc.name == "" || acc.stack <= 0) return;
	}
	if (AcAchieve != null) AcAchieve("SHK_AS+_OVERKILL",null);
}
public void UpdatePlayer(Player player) {
	if (player == null) return;
	if (player.name == "") return;
	if (player.whoAmi == Main.myPlayer) CheckAc(player);
	
	for (int i = 0; i < ModGeneric.extraSlots; i++) {
		Item acc = player.whoAmi == Main.myPlayer ? ModWorld.slots.itemSlots[i] : ModWorld.accessories[player.whoAmi][i];
		if (acc.type == 15) {if (player.accWatch < 1) player.accWatch = 1;}
		else if (acc.type == 16) {if (player.accWatch < 2) player.accWatch = 2;}
		else if (acc.type == 17) {if (player.accWatch < 3) player.accWatch = 3;}
		else if (acc.type == 18) {if (player.accDepthMeter < 1) player.accDepthMeter = 1;}
		else if (acc.type == 53) player.doubleJump = true;
		else if (acc.type == 54) player.baseMaxSpeed = 6f;
		else if (acc.type == 128) player.rocketBoots = 1;
		else if (acc.type == 156) player.noKnockback = true;
		else if (acc.type == 158) player.noFallDmg = true;
		else if (acc.type == 159) player.jumpBoost = true;
		else if (acc.type == 187) player.accFlipper = true;
		else if (acc.type == 211) player.meleeSpeed += 0.12f;
		else if (acc.type == 223) player.manaCost -= 0.06f;
		else if (acc.type == 285) player.moveSpeed += 0.05f;
		else if (acc.type == 212) player.moveSpeed += 0.1f;
		else if (acc.type == 267) player.killGuide = true;
		else if (acc.type == 193) player.fireWalk = true;
		else if (acc.type == 485) player.wolfAcc = true;
		else if (acc.type == 486) player.rulerAcc = true;
		else if (acc.type == 393) player.accCompass = 1;
		else if (acc.type == 394) {
			player.accFlipper = true;
			player.accDivingHelm = true;
		} else if (acc.type == 395) {
			player.accWatch = 3;
			player.accDepthMeter = 1;
			player.accCompass = 1;
		} else if (acc.type == 396) {
			player.noFallDmg = true;
			player.fireWalk = true;
		} else if (acc.type == 397) {
			player.noKnockback = true;
			player.fireWalk = true;
		} else if (acc.type == 399) {
			player.jumpBoost = true;
			player.doubleJump = true;
		} else if (acc.type == 405) {
			player.baseMaxSpeed = 6f;
			player.rocketBoots = 2;
		} else if (acc.type == 407) player.blockRange = 1;
		else if (acc.type == 489) player.magicDamage += 0.15f;
		else if (acc.type == 490) player.meleeDamage += 0.15f;
		else if (acc.type == 491) player.rangedDamage += 0.15f;
		else if (acc.type == 492) player.wings = 1;
		else if (acc.type == 493) player.wings = 2;
		else if (acc.type == 497) player.accMerman = true;
		else if (acc.type == 535) player.pStone = true;
		else if (acc.type == 536) player.kbGlove = true;
		else if (acc.type == 532) player.starCloak = true;
		else if (acc.type == 554) player.longInvince = true;
		else if (acc.type == 555) {
			player.manaFlower = true;
			player.manaCost -= 0.08f;
		} else if (Main.myPlayer == player.whoAmi) {
			if (acc.type == 576 && Main.rand.Next(18000) == 0 && Main.curMusic > 0) {
				int musId = 0;
				if (Main.curMusic == 1) musId = 0;
				if (Main.curMusic == 2) musId = 1;
				if (Main.curMusic == 3) musId = 2;
				if (Main.curMusic == 4) musId = 4;
				if (Main.curMusic == 5) musId = 5;
				if (Main.curMusic == 7) musId = 6;
				if (Main.curMusic == 8) musId = 7;
				if (Main.curMusic == 9) musId = 9;
				if (Main.curMusic == 10) musId = 8;
				if (Main.curMusic == 11) musId = 11;
				if (Main.curMusic == 12) musId = 10;
				if (Main.curMusic == 13) musId = 12;
				acc.SetDefaults(musId+562,false,true);
			}
			if (acc.type >= 562 && acc.type <= 574) Main.musicBox2 = acc.type-562;
		}
		
		player.statDefense += acc.defense;
		player.lifeRegen += acc.lifeRegen;
		if (acc.prefix == 62) player.statDefense++;
		else if (acc.prefix == 63) player.statDefense += 2;
		else if (acc.prefix == 64) player.statDefense += 3;
		else if (acc.prefix == 65) player.statDefense += 4;
		else if (acc.prefix == 66) player.statManaMax2 += 20;
		else if (acc.prefix == 67) {
			player.meleeCrit++;
			player.rangedCrit++;
			player.magicCrit++;
		} else if (acc.prefix == 68) {
			player.meleeCrit += 2;
			player.rangedCrit += 2;
			player.magicCrit += 2;
		} else if (acc.prefix == 69) {
			player.meleeDamage += 0.01f;
			player.rangedDamage += 0.01f;
			player.magicDamage += 0.01f;
		} else if (acc.prefix == 70) {
			player.meleeDamage += 0.02f;
			player.rangedDamage += 0.02f;
			player.magicDamage += 0.02f;
		} else if (acc.prefix == 71) {
			player.meleeDamage += 0.03f;
			player.rangedDamage += 0.03f;
			player.magicDamage += 0.03f;
		} else if (acc.prefix == 72) {
			player.meleeDamage += 0.04f;
			player.rangedDamage += 0.04f;
			player.magicDamage += 0.04f;
		} else if (acc.prefix == 73)player.moveSpeed += 0.01f;
		else if (acc.prefix == 74) player.moveSpeed += 0.02f;
		else if (acc.prefix == 75) player.moveSpeed += 0.03f;
		else if (acc.prefix == 76) player.moveSpeed += 0.04f;
		else if (acc.prefix == 77) player.meleeSpeed += 0.01f;
		else if (acc.prefix == 78) player.meleeSpeed += 0.02f;
		else if (acc.prefix == 79) player.meleeSpeed += 0.03f;
		else if (acc.prefix == 80) player.meleeSpeed += 0.04f;
		if (!acc.RunMethod("Effects",new object[]{player})) {
			if (acc.type == 238) player.magicDamage += 0.15f;
			else if (acc.type == 123 || acc.type == 124 || acc.type == 125) player.magicDamage += 0.05f;
			else if (acc.type == 151 || acc.type == 152 || acc.type == 153) player.rangedDamage += 0.05f;
			else if (acc.type == 111 || acc.type == 228 || acc.type == 229 || acc.type == 230) player.statManaMax2 += 20;
			else if (acc.type == 228 || acc.type == 229 || acc.type == 230) player.magicCrit += 3;
			else if (acc.type == 100 || acc.type == 101 || acc.type == 102) player.meleeSpeed += 0.07f;
			else if (acc.type == 371) {
				player.magicCrit += 9;
				player.statManaMax2 += 40;
			} else if (acc.type == 372) {
				player.moveSpeed += 0.07f;
				player.meleeSpeed += 0.12f;
			} else if (acc.type == 373) {
				player.rangedDamage += 0.1f;
				player.rangedCrit += 6;
			} else if (acc.type == 374) {
				player.magicCrit += 3;
				player.meleeCrit += 3;
				player.rangedCrit += 3;
			} else if (acc.type == 375) player.moveSpeed += 0.1f;
			else if (acc.type == 376) {
				player.magicDamage += 0.15f;
				player.statManaMax2 += 60;
			} else if (acc.type == 377) {
				player.meleeCrit += 5;
				player.meleeDamage += 0.1f;
			} else if (acc.type == 378) {
				player.rangedDamage += 0.12f;
				player.rangedCrit += 7;
			} else if (acc.type == 379) {
				player.rangedDamage += 0.05f;
				player.meleeDamage += 0.05f;
				player.magicDamage += 0.05f;
			} else if (acc.type == 380) {
				player.magicCrit += 3;
				player.meleeCrit += 3;
				player.rangedCrit += 3;
			} else if (acc.type == 268) player.accDivingHelm = true;
			else if (acc.type == 400) {
				player.magicDamage += 0.11f;
				player.magicCrit += 11;
				player.statManaMax2 += 80;
			} else if (acc.type == 401) {
				player.meleeCrit += 7;
				player.meleeDamage += 0.14f;
			} else if (acc.type == 402) {
				player.rangedDamage += 0.14f;
				player.rangedCrit += 8;
			} else if (acc.type == 403) {
				player.rangedDamage += 0.06f;
				player.meleeDamage += 0.06f;
				player.magicDamage += 0.06f;
			} else if (acc.type == 404) {
				player.magicCrit += 4;
				player.meleeCrit += 4;
				player.rangedCrit += 4;
				player.moveSpeed += 0.05f;
			} else if (acc.type == 558) {
				player.magicDamage += 0.12f;
				player.magicCrit += 12;
				player.statManaMax2 += 100;
			} else if (acc.type == 559) {
				player.meleeCrit += 10;
				player.meleeDamage += 0.1f;
				player.meleeSpeed += 0.1f;
			} else if (acc.type == 553) {
				player.rangedDamage += 0.15f;
				player.rangedCrit += 8;
			} else if (acc.type == 551) {
				player.magicCrit += 7;
				player.meleeCrit += 7;
				player.rangedCrit += 7;
			} else if (acc.type == 552) {
				player.rangedDamage += 0.07f;
				player.meleeDamage += 0.07f;
				player.magicDamage += 0.07f;
				player.moveSpeed += 0.08f;
			}
		}
	}
}

public void PostKill(Player player, double dmg, int hitDirection, bool pvp, string deathText) {
	Item[] items = Main.netMode != 2 && player.whoAmi == Main.myPlayer ? ModWorld.slots.itemSlots : ModWorld.accessories[player.whoAmi];
	if (player.difficulty > 0) {
		for (int i = 0; i < ModGeneric.extraSlots; i++) {
			if (items[i].stack > 0) {
				items[i].RunMethod("OnUnequip",player,-i-1);
				int id = Item.NewItem((int)player.position.X,(int)player.position.Y,player.width,player.height,items[i],false);
				Main.item[id].velocity.Y = Main.rand.Next(-20,1)*0.2f;
				Main.item[id].velocity.X = Main.rand.Next(-20,21)*0.2f;
				Main.item[id].noGrabDelay = 100;
			}
			items[i] = new Item();
		}
		if (Main.netMode != 2) ModWorld.SendItemData(Main.myPlayer,ModWorld.ExternalGetAccessorySlots(),-1,Main.myPlayer);
	}
}