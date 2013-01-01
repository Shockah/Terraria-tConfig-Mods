public static Microsoft.Xna.Framework.Audio.SoundEffectInstance[] soundInstance = new Microsoft.Xna.Framework.Audio.SoundEffectInstance[64];
public static int soundInstanceN = 0;

private Vector2 tilev;

public void Initialize(int x, int y) {
	tilev = new Vector2(x,y);
}

public void UseTile(Player player, int x, int y) {
	Play(x,y);
	if (Main.netMode == 0) {
		ModWorld.GuiNoteBlock.Create(x,y,ModWorld.GetNoteLevel(tilev));
	} else {
		NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_GUI_REQUEST,-1,-1,(byte)Main.myPlayer,x,y);
	}
}
public void hitWire(int x, int y) {
	Play(x,y);
	NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_PLAY,-1,-1,x,y,(byte)ModWorld.GetNoteLevel(tilev));
}
public void Play(int x, int y) {
	Play(x,y,ModWorld.GetNoteLevel(tilev));
}
public void Play(int x, int y, int noteLevel) {
	if (Main.netMode == 2) return;
	
	float level = 1f/12f*((noteLevel%12));
	PlaySound(x*16,y*16,soundHandler.soundID[GetSoundName(noteLevel)],level);
	
	float hue = 1f/3f;
	hue -= noteLevel/24f;
	if (hue < 0f) hue += 1f;
	ModWorld.AddNote(new Vector2(x*16+8,y*16+8),ModWorld.HSVtoRGB(hue,1f,1f));
}
public int ExternalGetNoteBlockLevel() {
	return ModWorld.GetNoteLevel(tilev);
}
public void ExternalSetNoteBlockLevel(int value) {
	ModWorld.SetNoteLevel(tilev,value);
}
public void ExternalNoteBlockPlay() {
	Play((int)tilev.X,(int)tilev.Y);
}
public void ExternalNoteBlockPlayLevel(int noteLevel) {
	Play((int)tilev.X,(int)tilev.Y,noteLevel);
}

public string GetSoundName(int level) {
	int type = Main.tile[(int)tilev.X,(int)tilev.Y].type;
	
	if (type == ModWorld.typeBass) return "noteBass"+(level < 12 ? "1" : "2");
	if (type == ModWorld.typeFlute) return "noteFlute"+(level < 12 ? "1" : "2");
	if (type == ModWorld.typeGuitar) return "noteGuitar"+(level < 12 ? "1" : "2");
	if (type == ModWorld.typePercussion) return "notePercussion"+(level < 12 ? "1" : "2");
	if (type == ModWorld.typePiano) return "notePiano"+(level < 12 ? "1" : "2");
	return null;
}

public void KillTile(int x, int y, Player player) {
	ModWorld.SetNoteLevel(tilev,-1);
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