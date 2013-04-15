public bool CanUse(Player player, int pID) {
	return ModPlayer.HasFreeStack(player);
}

public void UseItem(Player player, int playerID) {
	if (Main.rand.Next(2) == 0) {
		ModPlayer.GiveItem(player,GetTierReward(GetTier()));
	} else {
		ModGeneric.WeightedRandom<Item> wr = new ModGeneric.WeightedRandom<Item>();
		Item item;
		
		item = new Item();
		item.SetDefaults("Candy Cane Block");
		item.stack = wr.rnd.Next(20,50+1);
		wr.Add(item,7);
		
		item = new Item();
		item.SetDefaults("Green Candy Cane Block");
		item.stack = wr.rnd.Next(20,50+1);
		wr.Add(item,7);
		
		item = new Item();
		item.SetDefaults("Blue Warm Rug");
		item.stack = wr.rnd.Next(10,20+1);
		wr.Add(item,2);

		item = new Item();
		item.SetDefaults("Red Warm Rug");
		item.stack = wr.rnd.Next(10,20+1);
		wr.Add(item,2);

		item = new Item();
		item.SetDefaults("Green Warm Rug");
		item.stack = wr.rnd.Next(10,20+1);
		wr.Add(item,2);

		item = new Item();
		item.SetDefaults("Brown Warm Rug");
		item.stack = wr.rnd.Next(10,20+1);
		wr.Add(item,2);

		item = new Item();
		item.SetDefaults("White Warm Rug");
		item.stack = wr.rnd.Next(10,20+1);
		wr.Add(item,2);

		item = new Item();
		item.SetDefaults("Red Christmas Stocking");
		item.stack = wr.rnd.Next(2,5+1);
		wr.Add(item,1);

		item = new Item();
		item.SetDefaults("Wreath");
		item.stack = wr.rnd.Next(2,5+1);
		wr.Add(item,1);
		
		if (Main.hardMode) {
			item = new Item();
			item.SetDefaults("Snow Globe");
			wr.Add(item,2);
		}
		
		ModPlayer.GiveItem(player,wr.Get());
	}
}

public static Item GetTierReward(int tier) {
	ModGeneric.WeightedRandom<Item> wr;
	Item item;
	int randchance = 4;
	if (tier == 9) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Music Box");
			item.stack = 1;
			wr.Add(item,0.3f);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 8) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Soul of Light");
			item.stack = wr.rnd.Next(4,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Soul of Night");
			item.stack = wr.rnd.Next(4,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Soul of Flight");
			item.stack = wr.rnd.Next(4,8+1);
			wr.Add(item,2);

			item = new Item();
			item.SetDefaults("Ice Rod");
			item.stack = 1;
			wr.Add(item,0.05f);

			item = new Item();
			item.SetDefaults("Magic Dagger");
			item.stack = 1;
			wr.Add(item,0.1f);

			item = new Item();
			item.SetDefaults("Music Box");
			item.stack = 1;
			wr.Add(item,0.3f);

			
			return wr.Get();
		} else tier--;
	}
	if (tier == 7) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Shotgun");
			item.stack = 1;
			wr.Add(item,0.2f);

			item = new Item();
			item.SetDefaults("Cobalt Bar");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Mythril Bar");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Adamantite Bar");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Pixie Dust");
			item.stack = wr.rnd.Next(5,20+1);
			wr.Add(item,4);

			item = new Item();
			item.SetDefaults("Holy Water");
			item.stack = wr.rnd.Next(5,20+1);
			wr.Add(item,2);

			item = new Item();
			item.SetDefaults("Crystal Bullet");
			item.stack = wr.rnd.Next(30,120+1);
			wr.Add(item,2);
			item = new Item();

			item.SetDefaults("Light Shard");
			wr.Add(item,1);
			
			item = new Item();
			item.SetDefaults("Dark Shard");
			wr.Add(item,1);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 6) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Fallen Star");
			item.stack = wr.rnd.Next(5,25+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Wire");
			item.stack = wr.rnd.Next(15,50+1);
			wr.Add(item,1);
			item = new Item();

			item.SetDefaults("Jester's Arrow");
			item.stack = wr.rnd.Next(50,150+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Explosives");
			item.stack = wr.rnd.Next(1,5+1);
			wr.Add(item,0.5f);

			item = new Item();
			item.SetDefaults("Dart Trap");
			item.stack = wr.rnd.Next(1,5+1);
			wr.Add(item,0.5f);

			item = new Item();
			item.SetDefaults("5 Second Timer");
			item.stack = wr.rnd.Next(1,5+1);
			wr.Add(item,0.2f);

			item = new Item();
			item.SetDefaults("Gravitation Potion");
			item.stack = wr.rnd.Next(1,5+1);
			wr.Add(item,1);
			
			item = new Item();
			item.SetDefaults("Hermes Boots");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Life Crystal");
			item.stack = 1;
			wr.Add(item,0.4);
			item = new Item();

			item.SetDefaults("Heart Statue");
			item.stack = 1;
			wr.Add(item,0.3);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 5) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Hellstone Bar");
			item.stack = wr.rnd.Next(4,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Fireblossom");
			item.stack = wr.rnd.Next(2,5+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Golden Key");
			item.stack = wr.rnd.Next(2,5+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Obsidian Skin Potion");
			item.stack = wr.rnd.Next(2,5+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Lucky Horseshoe");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Flamelash");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Dark Lance");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Magic Mirror");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Shiny Red Balloon");
			item.stack = 1;
			wr.Add(item,0.2);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 4) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Deathweed");
			item.stack = wr.rnd.Next(4,10+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Waterleaf");
			item.stack = wr.rnd.Next(4,10+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Meteorite Bar");
			item.stack = wr.rnd.Next(4,12+1);
			wr.Add(item,2);

			item = new Item();
			item.SetDefaults("Shine Potion");
			item.stack = wr.rnd.Next(2,5+1);
			wr.Add(item,2);

			item = new Item();
			item.SetDefaults("Lamp Post");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Heart Statue");
			item.stack = 1;
			wr.Add(item,1);
			
			item = new Item();
			item.SetDefaults("Spelunker Potion");
			item.stack = wr.rnd.Next(1,3+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Minishark");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Starfury");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Trident");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Wizard Hat");
			item.stack = 1;
			wr.Add(item,0.2);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 3) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Amethyst");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Topaz");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Sapphire");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Emerald");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Ruby");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Diamond");
			item.stack = wr.rnd.Next(2,8+1);
			wr.Add(item,1);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 2) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Demonite Bar");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,1);
			
			item = new Item();
			item.SetDefaults("Shadow Scale");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Flintlock Pistol");
			item.stack = 1;
			wr.Add(item,0.2);

			item = new Item();
			item.SetDefaults("Vine");
			item.stack = wr.rnd.Next(1,4+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Jungle Spores");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,1);
			
			return wr.Get();
		} else tier--;
	}
	if (tier == 1) {
		if (Main.rand.Next(randchance) == 0) {
			wr = new ModGeneric.WeightedRandom<Item>();
			
			item = new Item();
			item.SetDefaults("Copper Bar");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,4);
			
			item = new Item();
			item.SetDefaults("Iron Bar");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,3);
			
			item = new Item();
			item.SetDefaults("Silver Bar");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,2);
			
			item = new Item();
			item.SetDefaults("Gold Bar");
			item.stack = wr.rnd.Next(5,15+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Purification Powder");
			item.stack = wr.rnd.Next(15,50+1);
			wr.Add(item,1);

			item = new Item();
			item.SetDefaults("Grenade");
			item.stack = wr.rnd.Next(10,30+1);
			wr.Add(item,1);
			
			return wr.Get();
		} else tier--;
	}
	
	wr = new ModGeneric.WeightedRandom<Item>();
	
	item = new Item();
	item.SetDefaults("Wood");
	item.stack = wr.rnd.Next(20,50+1);
	wr.Add(item,2);
	
	item = new Item();
	item.SetDefaults("Gel");
	item.stack = wr.rnd.Next(10,30+1);
	wr.Add(item,2);
	
	item = new Item();
	item.SetDefaults("Shuriken");
	item.stack = wr.rnd.Next(20,50+1);
	wr.Add(item,1);
	
	item = new Item();
	item.SetDefaults("Clay Block");
	item.stack = wr.rnd.Next(10,30+1);
	wr.Add(item,1);

	item = new Item();
	item.SetDefaults("Hook");
	item.stack = 1;
	wr.Add(item,0.5);

	item = new Item();
	item.SetDefaults("Silk");
	item.stack = wr.rnd.Next(5,20+1);
	wr.Add(item,0.7);
	
	return wr.Get();
}

public static int GetTier() {
	int tier = 0;
	if (NPC.downedBoss1) tier++;
	if (NPC.downedBoss2) tier++;
	if (NPC.downedGoblins) tier++;
	if (NPC.savedGoblin) tier++;
	if (NPC.downedBoss3) tier++;
	if (NPC.savedMech) tier++;
	if (Main.hardMode) tier++;
	if (NPC.savedWizard) tier++;
	if (NPC.downedFrost) tier++;
	return tier;
}