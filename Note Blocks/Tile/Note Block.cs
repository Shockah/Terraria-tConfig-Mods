public static Microsoft.Xna.Framework.Audio.SoundEffectInstance[] soundInstance = new Microsoft.Xna.Framework.Audio.SoundEffectInstance[64];
public static int soundInstanceN = 0;

private Tile tile;
private Vector2 tilev;

private readonly byte sheetWidth = 5;
private int noteType, noteLevel;

public void Initialize(int x, int y) {
	tile = Main.tile[x,y];
	tilev = new Vector2(x,y);
	noteType = tile.frameNumber;
	noteLevel = 0;
	UpdateTileFrame();
}
public void Save(BinaryWriter bw) {
	bw.Write((byte)noteLevel);
}
public void Load(BinaryReader br, int version) {
	noteLevel = (int)br.ReadByte();
}

public void UseTile(Player player, int x, int y) {
	Play(x,y);
	ModWorld.GuiNoteBlock.Create(x,y);
}
public void hitWire(int x, int y) {
	Play(x,y);
}
public void UpdateTileFrame() {
	tile.frameNumber = (byte)noteType;
}
public void Play(int x, int y) {
	float level = 1f/12f*((noteLevel%12));
	PlaySound(x*16,y*16,soundHandler.soundID[GetSoundName(noteLevel)],level);
	
	float hue = 1f/3f;
	hue -= noteLevel/24f;
	if (hue < 0f) hue += 1f;
	ModWorld.AddNote(new Vector2(x*16+8,y*16+8),ModWorld.HSVtoRGB(hue,1f,1f));
}
public int ExternalGetNoteBlockLevel() {
	return noteLevel;
}
public void ExternalSetNoteBlockLevel(int value) {
	noteLevel = value;
}
public void ExternalNoteBlockPlay() {
	Play((int)tilev.X,(int)tilev.Y);
}

public string GetItemName() {
	switch (noteType) {
		case 0: return "Note Bass"; break;
		case 1: return "Note Guitar"; break;
		case 2: return "Note Percussion"; break;
		case 3: return "Note Piano"; break;
		case 4: return "Note Flute"; break;
		default: return null;
	}
}
public string GetSoundName(int level) {
	switch (noteType) {
		case 0: return "noteBass"+(level < 12 ? "1" : "2"); break;
		case 1: return "noteGuitar"+(level < 12 ? "1" : "2"); break;
		case 2: return "notePercussion"+(level < 12 ? "1" : "2"); break;
		case 3: return "notePiano"+(level < 12 ? "1" : "2"); break;
		case 4: return "noteFlute"+(level < 12 ? "1" : "2"); break;
		default: return null;
	}
}

public void KillTile(int x, int y, Player player) {
	Item.NewItem(x*16,y*16,16,16,GetItemName());
}

public void UpdateFrame(int x, int y) {
	tile.frameX = (short)((tile.frameNumber%sheetWidth)/16);
	tile.frameY = (short)((tile.frameNumber/sheetWidth)/16);
	if (Main.netMode == 2) NetMessage.SendTileSquare(-1,(int)tilev.X,(int)tilev.Y,1);
}

public static void PlaySound(int x = -1, int y = -1, int Style = 1, float pitch = 0f) {
	if (Main.dedServ || Main.soundVolume == 0f) return;
	bool flag = x == -1 || y == -1;
	int num = Style;
	float num2 = 1f, num3 = 0f;
	if (!flag) {
		if (WorldGen.gen) return;
		if (Main.netMode == 2) return;
		
		Rectangle value = new Rectangle((int)(Main.screenPosition.X - (float)(Main.screenWidth * 2)), (int)(Main.screenPosition.Y - (float)(Main.screenHeight * 2)), Main.screenWidth * 5, Main.screenHeight * 5);
		Rectangle rectangle = new Rectangle(x, y, 1, 1);
		Vector2 vector = new Vector2(Main.screenPosition.X + (float)Main.screenWidth * 0.5f, Main.screenPosition.Y + (float)Main.screenHeight * 0.5f);
		if (rectangle.Intersects(value)) flag = true;
		if (flag) {
			num3 = ((float)x - vector.X) / ((float)Main.screenWidth * 0.5f);
			float num4 = System.Math.Abs((float)x - vector.X);
			float num5 = System.Math.Abs((float)y - vector.Y);
			float num6 = (float)System.Math.Sqrt((double)(num4 * num4 + num5 * num5));
			num2 = 1f - num6 / ((float)Main.screenWidth * 1.5f);
		}
		if (num3 < -1f) num3 = -1f;
		if (num3 > 1f) num3 = 1f;
		if (num2 > 1f) num2 = 1f;
		if (num2 > 0f && flag) {
			try {
				if (soundInstance[soundInstanceN] != null) {
					soundInstance[soundInstanceN].Stop();
				}
				soundInstance[soundInstanceN] = Main.soundItem[num].CreateInstance();
				soundInstance[soundInstanceN].Volume = num2;
				soundInstance[soundInstanceN].Pan = num3;
				soundInstance[soundInstanceN].Pitch = pitch;
				soundInstance[soundInstanceN].Play();
				
				if (++soundInstanceN >= soundInstance.Length) soundInstanceN = 0;
			} catch (Exception) {}
		}
	}
}