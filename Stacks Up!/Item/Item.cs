public void Initialize() {
	if (item.maxStack > 1) item.maxStack *= ModGeneric.maxStackMultiplier;
}

public void Save(BinaryWriter bw) {
	bw.Write((byte)item.stack/256);
}

public void Load(BinaryReader br, int v) {
	item.stack += br.ReadByte()*256;
}