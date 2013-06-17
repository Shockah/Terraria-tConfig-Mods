public string playerName = null;
public Item gift = new Item();

public static void ItemSave(BinaryWriter bw, Item item) {
	if (item == null) item = new Item();
	if (item.type == 0 || item.stack <= 0) {
		bw.Write(false);
	} else {
		bw.Write(true);
		bw.Write(item.name);
		bw.Write((byte)item.stack);
		bw.Write((byte)item.prefix);
		Prefix.SavePrefix(bw,item);
		Codable.SaveCustomData(item,bw);
	}
}
public static Item ItemLoad(BinaryReader br) {
	try {
		Item item = new Item();
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

public void Save(BinaryWriter bw) {
	ItemSave(bw,gift);
	if (playerName != null) bw.Write(playerName);
}
public void Load(BinaryReader br, int version) {
	gift = ItemLoad(br);
	if (gift.type != 0 && gift.stack > 0) playerName = br.ReadString();
}

public void AffixName(ref string name, bool afterPrefix) {
	if (!afterPrefix) return;
	if (playerName != null) name += " from "+playerName;
}
public bool InvRightClicked(Player player, int pID, int slot) {
	player.inventory[slot] = gift;
	Main.PlaySound(7,-1,-1,1);
	return false;
}

public void ExternalSetPresentContents(string playerName, Item gift) {
	this.playerName = playerName;
	this.gift = gift;
}
public string ExternalGetPresentPlayer() {
	return playerName;
}
public Item ExternalGetPresentContents() {
	return gift;
}