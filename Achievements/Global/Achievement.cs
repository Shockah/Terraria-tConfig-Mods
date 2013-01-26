public class Achievement {
	public static Func<object,object,string> defaultProgressParser = delegate(object o1, object o2){
		if (o1 is double || o1 is int) {
			int val1, val2;
			if (o1 is double) {
				val1 = (int)Math.Floor((double)o1);
				val2 = (int)Math.Ceiling((double)o2);
			} else if (o1 is int) {
				val1 = (int)Math.Floor((decimal)(int)o1);
				val2 = (int)Math.Ceiling((decimal)(int)o2);
			} else return null;
			
			string s1 = "", s2 = "";
			while (val1 > 0) {
				int v = val1 % 1000;
				val1 -= v;
				val1 /= 1000;
				s1 = val1 != 0 ? ","+String.Format("{0:000}",v)+s1 : ""+v+s1;
			}
			if (s1 == "") s1 = "0";
			while (val2 > 0) {
				int v = val2 % 1000;
				val2 -= v;
				val2 /= 1000;
				s2 = val2 != 0 ? ","+String.Format("{0:000}",v)+s2 : ""+v+s2;
			}
			if (s2 == "") s2 = "0";
			
			return s1+" / "+s2;
		}
		return null;
	};
	
	public readonly string apiName, category, title, description, parent;
	public readonly int value;
	public Texture2D tex;
	public readonly bool hidden;
	public int netMode = -1, difficulty = 7, hardMode = 3;
	public object progress = null, progressMax = null;
	public Func<object,object,string> progressParser = defaultProgressParser;
	
	public bool achieved = false;
	public List<Achievement> sub = new List<Achievement>();
	
	public Achievement(string apiName, string category, string title, string description, string parent, int value = 10, Texture2D tex = null, bool hidden = false) {
		this.apiName = apiName;
		this.category = category;
		this.title = title;
		this.description = description;
		this.parent = parent;
		this.value = value;
		this.tex = tex;
		this.hidden = hidden;
	}
	
	public void ReadProgress(BinaryReader br) {
		int type = (int)br.ReadByte();
		switch (type) {
			case 1: {
				progress = br.ReadDouble();
				double d = br.ReadDouble();
				if (progressMax == null) progressMax = d;
			} break;
			case 2: {
				progress = br.ReadInt32();
				int i = br.ReadInt32();
				if (progressMax == null) progressMax = i;
			} break;
			default: break;
		}
	}
	public void Write(BinaryWriter bw) {
		bw.Write(apiName);
		bw.Write(achieved);
		
		if (progress == null) bw.Write((byte)0);
		else if (progress is Double) {
			bw.Write((byte)1);
			bw.Write((double)progress);
			bw.Write((double)progressMax);
		} else if (progress is Int32) {
			bw.Write((byte)2);
			bw.Write((int)progress);
			bw.Write((int)progressMax);
		}
	}
	
	public float GetProgress() {
		if (progress == null) return 0f;
		else if (progress is double) return (float)((double)(progress)/(double)(progressMax));
		else if (progress is int) return (float)((double)(int)(progress)/(double)(int)(progressMax));
		return 0f;
	}
	public string GetProgressText() {
		if (progress == null) return null;
		return progressParser(progress,progressMax);
	}
	
	public bool CheckNetMode() {
		if (netMode < 0) return true;
		return netMode == Main.netMode;
	}
	public bool CheckDifficulty() {
		switch (Main.player[Main.myPlayer].difficulty) {
			case 0: return difficulty == 1 || difficulty == 3 || difficulty == 5 || difficulty == 7; break;
			case 1: return difficulty == 2 || difficulty == 3 || difficulty == 6 || difficulty == 7; break;
			case 2: return difficulty == 4 || difficulty == 5 || difficulty == 6 || difficulty == 7; break;
			default: return false; break;
		}
	}
	public bool CheckHardMode() {
		if (hardMode == 1) return !Main.hardMode;
		if (hardMode == 2) return Main.hardMode;
		return true;
	}
	
	public Achievement Compare(List<Achievement> list, Achievement ac2) {
		if (ac2 == null) return this;
		if (achieved ^ ac2.achieved) return achieved ? this : ac2;
		if (value < ac2.value) return this;
		if (value > ac2.value) return ac2;
		return String.Compare(ac2.title,title) < 0 ? ac2 : this;
	}
	
	public void Draw(SpriteBatch sb, int xx, ref int yy, int ww) {
		Color white = new Color(255,255,255,191);
		Color limish = new Color(136,255,0,191);
		Color orange = new Color(255,127,0,191);
		
		float progress = GetProgress();
		if (achieved) progress = 1f;
		Color c = achieved ? limish : white;
		
		if (progress <= 0f || progress >= 1f) {
			sb.Draw(ModWorld.Notifier.frame1,new Rectangle(xx+1,yy,ww-2,56),c);
			sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx,yy,1,56),c);
			sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx+ww-1,yy,1,56),c);
		} else {
			int progressW = (int)((ww-2)*progress);
			sb.Draw(ModWorld.Notifier.frame1,new Rectangle(xx+1,yy,progressW,56),orange);
			sb.Draw(ModWorld.Notifier.frame1,new Rectangle(xx+1+progressW,yy,ww-2-progressW,56),white);
			sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx,yy,1,56),orange);
			sb.Draw(ModWorld.Notifier.frame2,new Rectangle(xx+ww-1,yy,1,56),white);
		}
		
		xx += 4;
		if (tex != null) {
			float scale = 1f;
			if (tex.Width > 48) scale = 48f/tex.Width;
			if (tex.Height*scale > 48f) scale = 48f/tex.Height;
			sb.Draw(tex,new Rectangle((int)(xx+(48-tex.Width*scale)/2f),(int)(yy+4+(48-tex.Height*scale)/2f),(int)(tex.Width*scale),(int)(tex.Height*scale)),Color.White);
			xx += 52;
		}
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,title,new Vector2(xx,yy+8),Color.White,Color.Black);
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,description,new Vector2(xx,yy+28),Color.White,Color.Black,default(Vector2),.75f);
		string progressText = GetProgressText();
		if (!achieved && progressText != null) ModWorld.DrawStringShadowed(sb,Main.fontMouseText,progressText,new Vector2((int)(xx+Main.fontMouseText.MeasureString(description).X*.75f+20),yy+18),Color.White,Color.Black,default(Vector2),.75f);
		xx -= 4; if (tex != null) xx -= 52;
		ModWorld.DrawStringShadowed(sb,Main.fontMouseText,""+value,new Vector2(xx+ww-12-Main.fontMouseText.MeasureString(""+value).X,yy+16),Color.White,Color.Black);
		
		yy += 56;
		
		for (int i = 0; i < sub.Count; i++) sub[i].Draw(sb,xx+24,ref yy,ww-48);
	}
}