#INCLUDE "Note.cs"
#INCLUDE "GuiNoteBlock.cs"
#INCLUDE "Pair.cs"

public static int modId;
public const int
	MSG_PLAY = 1,
	MSG_SETLEVEL = 2,
	MSG_GUI_REQUEST = 3;

public static int
	typeBass = Config.tileDefs.ID["Note Bass"],
	typeFlute = Config.tileDefs.ID["Note Flute"],
	typeGuitar = Config.tileDefs.ID["Note Guitar"],
	typePercussion = Config.tileDefs.ID["Note Percussion"],
	typePiano = Config.tileDefs.ID["Note Piano"];

public static int noteCount = 0;
public static Note[] notes = new Note[64];

public static List<Pair<Vector2,int>> noteLevels = new List<Pair<Vector2,int>>();

public void Initialize(int modId) {
	ModWorld.modId = modId;
	noteLevels.Clear();
}

public void Save(BinaryWriter bw) {
	for (int i = 0; i < noteLevels.Count; i++) {
		Pair<Vector2,int> pair = noteLevels[i];
		bw.Write(true);
		bw.Write((int)pair.A.X);
		bw.Write((int)pair.A.Y);
		bw.Write((byte)pair.B);
	}
	bw.Write(false);
}
public void Load(BinaryReader br, int version) {
	while (br.ReadBoolean()) {
		Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
		noteLevels.Add(new Pair<Vector2,int>(v,(int)br.ReadByte()));
	}
}

public static void SetNoteLevel(Vector2 v, int level) {
	for (int i = 0; i < noteLevels.Count; i++) {
		Pair<Vector2,int> pair = noteLevels[i];
		if (v.X == pair.A.X && v.Y == pair.A.Y) {
			noteLevels.RemoveAt(i);
			if (level >= 0) noteLevels.Add(new Pair<Vector2,int>(v,level));
			return;
		}
	}
	
	noteLevels.Add(new Pair<Vector2,int>(v,level));
}
public static int GetNoteLevel(Vector2 v) {
	foreach (Pair<Vector2,int> pair in noteLevels) {
		if (v.X == pair.A.X && v.Y == pair.A.Y) return pair.B;
	}
	noteLevels.Add(new Pair<Vector2,int>(v,0));
	return 0;
}

public void NetReceive(int messageType, BinaryReader br) {
	if (Main.netMode == 1) {
		switch (messageType) {
			case MSG_PLAY: {
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				int x = (int)v.X, y = (int)v.Y;
				int noteLevel = br.ReadByte();
				Codable.RunTileMethod(false,v,Main.tile[x,y].type,"ExternalNoteBlockPlayLevel",noteLevel);
				if (Config.tileInterface != null && Config.tileInterface.code is GuiNoteBlock && x == (int)Config.tileInterface.sourceLocation.X && y == (int)Config.tileInterface.sourceLocation.Y) GuiNoteBlock.Create(x,y,noteLevel);
			} break;
			case MSG_GUI_REQUEST: {
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				int x = (int)v.X, y = (int)v.Y;
				int noteLevel = br.ReadByte();
				
				GuiNoteBlock.Create(x,y,noteLevel);
			} break;
			default: break;
		}
	} else if (Main.netMode == 2) {
		switch (messageType) {
			case MSG_SETLEVEL: {
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				int x = (int)v.X, y = (int)v.Y;
				int noteLevel = br.ReadByte();
				SetNoteLevel(v,noteLevel);
			} break;
			case MSG_GUI_REQUEST: {
				int pID = br.ReadByte();
				Vector2 v = new Vector2(br.ReadInt32(),br.ReadInt32());
				int x = (int)v.X, y = (int)v.Y;
				
				int noteLevel = GetNoteLevel(v);
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_GUI_REQUEST,pID,-1,x,y,(byte)noteLevel);
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_PLAY,-1,pID,x,y,(byte)noteLevel);
			} break;
			default: break;
		}
	}
}

public static void AddNote(Vector2 pos, Color color) {
	notes[noteCount++] = new Note(pos,color);
	if (noteCount >= notes.Length) noteCount = 0;
}

public void PostDrawBackground(SpriteBatch sb) {
	for (int i = 0; i < notes.Length; i++) {
		if (notes[i] == null) continue;
		if (notes[i].alpha <= 0f) continue;
		notes[i].Update();
	}
}
public void PreDrawInterface(SpriteBatch sb) {
	for (int i = 0; i < notes.Length; i++) {
		if (notes[i] == null) continue;
		if (notes[i].alpha <= 0f) continue;
		notes[i].Draw(sb);
	}
}

public bool PreDrawAvailableRecipes(SpriteBatch sb) {
	Dictionary<int,int> A = new Dictionary<int,int>();
	A.Add(0,0);
	Codable.RunGlobalMethod("ModWorld","AnySinisterMenus",A);
	return A[0] <= 0;
}
public static void AnySinisterMenus(Dictionary<int,int> A) {
	if (Config.tileInterface != null && Config.tileInterface.code is GuiNoteBlock) A[0]++;
}

public static Color HSVtoRGB(float h, float s, float v) {
	int i;
	float f,p,q,t;
	
	if (s == 0) return new Color(v,v,v);
	
	h *= 5f;
	i = (int)Math.Floor(h);
	f = h-i;
	p = v*(1-s);
	q = v*(1-s*f);
	t = v*(1-s*(1-f));
	
	switch( i ) {
		case 0: return new Color(v,t,p); break;
		case 1: return new Color(q,v,p); break;
		case 2: return new Color(p,v,t); break;
		case 3: return new Color(p,q,v); break;
		case 4: return new Color(t,p,v); break;
		default: return new Color(v,p,q); break;
	}
}