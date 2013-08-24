[Serializable] public class EffectFireflyRainbow : EffectFireflyHover {
	public static int GetMaxCount(List<Player> players) {
		int ret = 0, cur = 6, nextTier = 1, i = 0;
		foreach (Player player in players) {
			if (!DoesFulfillSpawnConditions(player)) continue;
			ret += cur;
			if (++i >= nextTier) {
				if (cur > 1) cur--;
				nextTier++;
				i = 0;
			}
		}
		return ret;
	}
	protected static bool DoesFulfillSpawnConditions(Player player) {
		return player.zone["Hallow"];
	}
	
	protected float hue, hueSpeed;
	protected List<float> rainbowHue = new List<float>();
	protected List<float> rainbowSize = new List<float>();
	protected List<Vector2> rainbowPos = new List<Vector2>();
	protected Vector2 moved = new Vector2();
	
	public EffectFireflyRainbow(Random rand) : base(rand) {Init();}
	public EffectFireflyRainbow(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		speed = (float)Math.Sqrt(speed/1.5f+2f);
		hue = (float)(rand.NextDouble()*360d);
		hueSpeed = rand.Next(Settings.GetInt("rarity")) == 0 ? (float)((1d+rand.NextDouble()*9d)*rand.Next(2) == 1 ? 1 : -1) : 0;
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
		bw.Write(hue);
		bw.Write(hueSpeed);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
		hue = br.ReadSingle();
		hueSpeed = br.ReadSingle();
	}
	
	protected override void OnCreate() {
		base.OnCreate();
		int y = FindTileBelow((int)(pos.X/16),(int)(pos.Y/16));
		if (y != -1) pos.Y = y*16f-(height*16f);
	}
	protected override void OnDestroy() {
		int r,g,b;
		HsvToRgb(hue,1f,1f,out r,out g,out b);
		Color c = new Color(r,g,b,255);
		
		int dusts = (int)(speed*30);
		for (int i = 0; i < dusts; i++) {
			int id = Dust.NewDust(GetDrawPos(),0,0,43,0f,0f,100,c,0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	
	protected override void OnUpdate(List<Player> players) {
		if (hueSpeed != 0 && moved.Length() >= speed*size/16f) {
			moved.X = 0;
			moved.Y = 0;
			if (rainbowPos.Count == 19) {
				rainbowPos.RemoveAt(0);
				rainbowHue.RemoveAt(0);
				rainbowSize.RemoveAt(0);
			}
			rainbowPos.Add(GetDrawPos());
			rainbowHue.Add(hue);
			rainbowSize.Add((float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180))));
		}
		
		Vector2 opos = new Vector2(pos.X,pos.Y);
		if (IsActive()) OnUpdateMove(players,3);
		moved += pos-opos;
		base.OnUpdate(players);
		
		hue += hueSpeed;
		if (hue >= 360) hue -= 360;
		if (hue < 0) hue += 360;
		
		if (IsActive() && rand.Next((int)(50-speed*10)) == 0) {
			int r,g,b;
			HsvToRgb(hue,1f,1f,out r,out g,out b);
			Color c = new Color(r,g,b,(byte)(alpha*255));
			
			int id = Dust.NewDust(GetDrawPos(),0,0,43,0f,0f,100,c,0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	public override Vector2 GetDrawPos() {
		float hover = IsActive() ? 16f : 2f;
		Vector2 pos2 = new Vector2(pos.X,pos.Y);
		pos2.Y += (float)(Math.Sin(sin*(Math.PI/180))*hover);
		return pos2;
	}
	protected override bool ShouldDie(List<Player> players) {
		return base.ShouldDie(players) || Main.dayTime;
	}
	
	protected override void OnDraw(SpriteBatch sb) {
		Vector2 pos2 = GetDrawPos();
		
		if (hueSpeed != 0 && IsActive()) {
			float aa = 1f-rainbowPos.Count*.05f;
			for (int i = 0; i < rainbowPos.Count; i++) {
				OnDrawOne(sb,rainbowPos[i],rainbowSize[i],rainbowHue[i],aa);
				aa += .05f;
			}
		}
		OnDrawOne(sb,pos2,(float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180))),hue,1f);
	}
	protected virtual void OnDrawOne(SpriteBatch sb, Vector2 pos, float size, float hue, float alpha) {
		int r,g,b;
		HsvToRgb(hue,1f,1f,out r,out g,out b);
		Color c = new Color(r,g,b,(byte)(this.alpha*alpha*255));
		
		float sizeAmp = hueSpeed != 0 && !IsActive() ? 1.5f : 1f;
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),new Color(c.R,c.G,c.B,(byte)(c.A/2f)),0f,GetCenterFuzzy(),GetScaleFuzzy(size*sizeAmp),SpriteEffects.None,0f);
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),c,0f,GetCenterFuzzy(),GetScaleFuzzy(size*sizeAmp/2f),SpriteEffects.None,0f);
		float lightAmp = hueSpeed != 0 && !IsActive() ? 2f : 1f;
		if (addLight) Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),c.R/255f*size/32f*c.A/255f*lightAmp,c.G/255f*size/32f*c.A/255f*lightAmp,c.B/255f*size/32f*c.A/255f*lightAmp);
	}
	
	public override void OnCatch(Player player) {
		if (Main.netMode == 2 || player.whoAmi != Main.myPlayer) return;
		if (ModWorld.AcAchieve != null) {
			ModWorld.AcAchieve("SHK_WORLDP_FLY_HOLY",null);
			if (hueSpeed != 0) ModWorld.AcAchieve("SHK_WORLDP_FLY_HOLY2",null);
		}
	}
	protected virtual string GetColorName() {
		if (hueSpeed != 0) return "Rainbow";
		
		if (hue < 20) return "Red";
		if (hue < 50) return "Orange";
		if (hue < 70) return "Yellow";
		if (hue < 100) return "Lime";
		if (hue < 150) return "Green";
		if (hue < 190) return "Cyan";
		if (hue < 220) return "Blue";
		if (hue < 255) return "Deep Blue";
		if (hue < 280) return "Violet";
		if (hue < 330) return "Fuchsia";
		return "Red";
	}
	public override void AffixName(ref string name) {
		name = GetColorName()+" "+name;
	}
	public override void RefreshItemValue(ref int value, ref int rare) {
		rare += 2;
		value *= 20;
		
		if (hueSpeed != 0) {
			rare += 2;
			value *= 10;
		}
		
		base.RefreshItemValue(ref value,ref rare);
	}
}