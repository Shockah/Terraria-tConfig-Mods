private ModWorld.EffectFirefly firefly;

public void Save(BinaryWriter bw) {
	if (firefly == null) {
		bw.Write(false);
		return;
	}
	
	bw.Write(true);
	ModWorld.fireflies.SaveOne(bw,firefly);
}
public void Load(BinaryReader br, int version) {
	firefly = null;
	if (!br.ReadBoolean()) return;
	
	ExternalSetFirefly(ModWorld.fireflies.LoadOne(br));
}

public bool InvRightClicked(Player player, int pId, int slot) {
	firefly.Clone().Create(player.Center);
	player.inventory[slot].stack = 0;
	Main.PlaySound(7,-1,-1,1);
	
	if (Main.netMode != 1) return false;
	using (MemoryStream ms = new MemoryStream())
	using (BinaryWriter bw = new BinaryWriter(ms)) {
		bw.Write((byte)Main.myPlayer);
		ModWorld.fireflies.SaveOne(bw,firefly);
		ModWorld.NetworkHelper.Write(bw,player.Center);
		ModWorld.NetworkHelper.Send(ModWorld.NetworkHelper.FLYSPAWNDETAILED,ms);
	}
	
	return false;
}
public void PostPlaceThing(int x, int y, Player player) {
	if (Main.netMode == 2) return;
	ModWorld.fireflies.SetAt(x,y,firefly);
	
	if (Main.netMode != 1) return;
	using (MemoryStream ms = new MemoryStream())
	using (BinaryWriter bw = new BinaryWriter(ms)) {
		bw.Write((byte)Main.myPlayer);
		bw.Write(x);
		bw.Write(y);
		bw.Write(true);
		ModWorld.fireflies.SaveOne(bw,firefly);
		ModWorld.NetworkHelper.Send(ModWorld.NetworkHelper.FLYPLACE,ms);
	}
}

public void AffixName(ref string name, bool afterPrefix) {
	if (!afterPrefix) return;
	if (firefly == null) return;
	firefly.AffixName(ref name);
}

public void ExternalSetFirefly(ModWorld.EffectFirefly firefly) {
	this.firefly = firefly;
	
	Item item2 = new Item();
	item2.SetDefaults("Firefly in a Bottle");
	item.value = item2.value;
	item.rare = item2.rare;
	
	firefly.RefreshItemValue(ref item.value, ref item.rare);
}
public ModWorld.EffectFirefly ExternalGetFirefly() {
	return firefly;
}