public static int energy, energyMax;

public static void Initialize() {
	energy = energyMax = 0;
}
public void CreatePlayer(Player p) {
	Initialize();
}
public void Save(BinaryWriter bw) {
	bw.Write((ushort)energy);
	bw.Write((ushort)energyMax);
}
public void Load(BinaryReader br, int version) {
	Initialize();
	try {
		energy = (int)br.ReadUInt16();
		energyMax = (int)br.ReadUInt16();
	} catch (Exception) {}
}