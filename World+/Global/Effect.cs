[Serializable] public class Effect : IEquatable<Effect> {
	public static int nextId = 0;
	public readonly static Texture2D ptFuzzy, ptSquare, ptStar, ptRing;
	public readonly static BlendState
		bsSubtract = new BlendState{
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.ReverseSubtract,
			AlphaSourceBlend = Blend.SourceAlpha,
			AlphaDestinationBlend = Blend.One,
			AlphaBlendFunction = BlendFunction.ReverseSubtract
		};
	
	static Effect() {
		if (Main.dedServ) return;
		ptFuzzy = Main.goreTexture[Config.goreID["ParticleFuzzy"]];
		ptSquare = Main.goreTexture[Config.goreID["ParticleSquare"]];
		ptStar = Main.goreTexture[Config.goreID["ParticleStar"]];
		ptRing = Main.goreTexture[Config.goreID["ParticleRing"]];
	}
	
	public static Rectangle? GetTexRectangle(Texture2D tex) {return new Rectangle?(new Rectangle(0,0,tex.Width,tex.Height));}
	public static Vector2 GetTexCenter(Texture2D tex) {return new Vector2(tex.Width/2f,tex.Height/2f);}
	public static float GetTexScale(Texture2D tex, float px) {return 1f/tex.Width*px;}
	
	public static Rectangle? GetRectFuzzy() {return GetTexRectangle(ptFuzzy);}
	public static Vector2 GetCenterFuzzy() {return GetTexCenter(ptFuzzy);}
	public static float GetScaleFuzzy(float px) {return GetTexScale(ptFuzzy,px);}
	
	public static Rectangle? GetRectSquare() {return GetTexRectangle(ptSquare);}
	public static Vector2 GetCenterSquare() {return GetTexCenter(ptSquare);}
	public static float GetScaleSquare(float px) {return GetTexScale(ptSquare,px);}
	
	public static Rectangle? GetRectStar() {return GetTexRectangle(ptStar);}
	public static Vector2 GetCenterStar() {return GetTexCenter(ptStar);}
	public static float GetScaleStar(float px) {return GetTexScale(ptStar,px);}
	
	public static Rectangle? GetRectRing() {return GetTexRectangle(ptRing);}
	public static Vector2 GetCenterRing() {return GetTexCenter(ptRing);}
	public static float GetScaleRing(float px) {return GetTexScale(ptRing,px);}
	
	public static float LdirX(double dist, double angle) {return (float)(-Math.Cos((angle+180)*Math.PI/180f)*dist);}
	public static float LdirY(double dist, double angle) {return (float)(Math.Sin((angle+180)*Math.PI/180f)*dist);}
	public static float Direction(Vector2 v1, Vector2 v2) {return (float)(Math.Atan2(v1.Y-v2.Y,v2.X-v1.X)*(180f/Math.PI));}
	public static Vector2 Vector(float dist, float angle) {return new Vector2(LdirX(dist,angle),LdirY(dist,angle));}
	
	public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b) {
		double H = h;
		while (H < 0) { H += 360; };
		while (H >= 360) { H -= 360; };
		double R, G, B;
		
		if (V <= 0) { R = G = B = 0; }
		else if (S <= 0) { R = G = B = V; }
		else {
			double hf = H / 60.0;
			int i = (int)Math.Floor(hf);
			double f = hf - i;
			double pv = V * (1 - S);
			double qv = V * (1 - S * f);
			double tv = V * (1 - S * (1 - f));
			switch (i) {
				case 0:
					R = V;
					G = tv;
					B = pv;
				break;
				case 1:
					R = qv;
					G = V;
					B = pv;
				break;
				case 2:
					R = pv;
					G = V;
					B = tv;
				break;
				case 3:
					R = pv;
					G = qv;
					B = V;
				break;
				case 4:
					R = tv;
					G = pv;
					B = V;
				break;
				case 5:
					R = V;
					G = pv;
					B = qv;
				break;
				case 6:
					R = V;
					G = tv;
					B = pv;
				break;
				case -1:
					R = V;
					G = pv;
					B = qv;
				break;
				default:
					R = G = B = V;
				break;
			}
		}
		r = Clamp((int)(R*255.0));
		g = Clamp((int)(G*255.0));
		b = Clamp((int)(B*255.0));
	}
	private static int Clamp(int i) {
		if (i < 0) return 0;
		if (i > 255) return 255;
		return i;
	}
	public static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v) {
		float max = Math.Max(Math.Max(r,g),b);
		float min = Math.Min(Math.Min(r,g),b);
		float C = max-min;
		
		v = max;
		s = C == 0 || v == 0 ? 0 : 1f*C/v;
		h = 0;
		
		if (max == r) h = ((g-b)*1f/C)%6;
		else if (max == g) h = ((b-r)*1f/C)+2;
		else if (max == b) h = ((r-g)*1f/C)+4;
		h *= 60;
	}
	
	public Effect(double depth) {
		_depth = depth;
	}
	
	public readonly int id;
	private double _depth;
	public double depth {
		get {return _depth;}
		set {
			ModWorld.effectsRemove.Add(this);
			ModWorld.effectsAdd.Add(this);
			depth = value;
		}
	}
	private bool active = false;
	
	public Effect() : this(nextId++) {}
	public Effect(int id) {
		this.id = id;
	}
	
	public bool Equals(Effect other) {
		return other.id == id;
	}
	
	public void Create() {
		ModWorld.effectsAdd.Add(this);
		OnCreate();
		active = true;
	}
	protected virtual void OnCreate() {}
	public void Destroy() {
		active = false;
		OnDestroy();
		ModWorld.effectsRemove.Add(this);
	}
	protected virtual void OnDestroy() {}
	
	public bool IsActive() {
		return active;
	}
	
	public void Update(List<Player> players) {
		OnUpdate(players);
	}
	protected virtual void OnUpdate(List<Player> players) {}
	
	public void Draw(SpriteBatch sb) {
		OnDraw(sb);
	}
	protected virtual void OnDraw(SpriteBatch sb) {}
}