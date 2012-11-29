#INCLUDE "Note.cs"

public static int noteCount = 0;
public static Note[] notes = new Note[64];

public static void AddNote(Vector2 pos, Color color) {
	notes[noteCount++] = new Note(pos,color);
	if (noteCount >= notes.Length) noteCount = 0;
}

public static void UpdateWorld() {
	for (int i = 0; i < notes.Length; i++) {
		if (notes[i] == null) continue;
		if (notes[i].alpha <= 0f) continue;
		notes[i].Update();
	}
}
public static void PreDrawInterface(SpriteBatch sb) {
	for (int i = 0; i < notes.Length; i++) {
		if (notes[i] == null) continue;
		if (notes[i].alpha <= 0f) continue;
		notes[i].Draw(sb);
	}
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