private int tx, ty;

public void Initialize(int x, int y) {
	tx = x;
	ty = y;
}

public void Save(BinaryWriter bw) {
	ModWorld.fireflies.SaveOne(bw,ModWorld.fireflies.GetFireflyAt(tx,ty));
}
public void Load(BinaryReader br, int version) {
	ModWorld.fireflies.SetAt(tx,ty,ModWorld.fireflies.LoadOne(br));
}

public void KillTile(int x, int y, Player player) {
	if (Main.netMode != 1) {
		Item item = new Item();
		item.SetDefaults("Firefly in a Bottle");
		item.RunMethod("ExternalSetFirefly",ModWorld.fireflies.GetFireflyAt(tx,ty));
		Item.NewItem((int)(tx*16+8),(int)(ty*16+8),0,0,item);
	}
	ModWorld.fireflies.SetAt(tx,ty,null);
}