#INCLUDE "Pair.cs"
#INCLUDE "WeightedRandom.cs"

public static void checkXMas() {
	Main.xMas = true;
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

public void ExternalFCMSetNPCCategoryTexture(Dictionary<string,Texture2D> customTex, Dictionary<string,int> customCount) {
	customTex["Awesome Christmas"] = Main.npcTexture[Config.npcDefs.byName["Frostmaw"].type];
	customCount["Awesome Christmas"] = 1;
}