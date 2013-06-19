[Serializable] public class EffectFireflyFlame : EffectFireflyHover {
	public const int
		TYPE_SIN = 0,
		TYPE_SIN2 = 1;
	
	public static int GetMaxCount(List<Player> players) {
		int ret = 0, cur = 5, nextTier = 1, i = 0;
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
		return player.zone["Hell"];
	}
	
	protected int type;
	protected Color color1, color2;
	protected float rot1, rotSpeed1, rot2, rotSpeed2;
	
	public EffectFireflyFlame(Random rand) : base(rand) {Init();}
	public EffectFireflyFlame(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		speed = (float)(Math.Sqrt(speed/1.5f+2f)*1.5f);
		size *= 2.5f;
		sinSizeSpeed *= 4f;
		maxSinSize = 1f/3f;
		
		type = rand.Next(2);
		switch (type) {
			case TYPE_SIN: {
				sinSpeed *= 4f;
			} break;
			default: break;
		}
		
		int col = rand.Next(50);
		if (col == 0) {
			color1 = new Color(0,255,0);
			color2 = new Color(127,255,0);
		} else {
			color1 = new Color(255,0,0);
			color2 = new Color(255,127,0);
		}
		
		rot1 = (float)(rand.NextDouble()*360d);
		rotSpeed1 = (float)((1d+rand.NextDouble()*2d)*rand.Next(2) == 1 ? 1 : -1);
		rot2 = (float)(rand.NextDouble()*360d);
		rotSpeed2 = (float)((2d+rand.NextDouble()*4d)*Math.Sign(-rotSpeed1));
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
		bw.Write((byte)type);
		NetworkHelper.Write(bw,color1);
		NetworkHelper.Write(bw,color2);
		bw.Write(rot1);
		bw.Write(rotSpeed1);
		bw.Write(rot2);
		bw.Write(rotSpeed2);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
		type = br.ReadByte();
		color1 = NetworkHelper.ReadColor(br);
		color2 = NetworkHelper.ReadColor(br);
		rot1 = br.ReadSingle();
		rotSpeed1 = br.ReadSingle();
		rot2 = br.ReadSingle();
		rotSpeed2 = br.ReadSingle();
	}
	
	protected override void OnCreate() {
		base.OnCreate();
		int y = FindTileBelow((int)(pos.X/16),(int)(pos.Y/16));
		if (y != -1) pos.Y = y*16f-(height*16f);
	}
	protected override void OnDestroy() {
		Color c = Color.Lerp(color1,color2,.5f);
		
		int dusts = (int)(speed*30);
		for (int i = 0; i < dusts; i++) {
			int id = Dust.NewDust(GetDrawPos(),0,0,43,0f,0f,100,c,0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	
	protected override void OnUpdate(List<Player> players) {
		if (IsActive()) OnUpdateMove(players,10);
		base.OnUpdate(players);
		
		rot1 += rotSpeed1;
		if (rot1 >= 360) rot1 -= 360;
		if (rot1 < 0) rot1 += 360;
		
		rot2 += rotSpeed2;
		if (rot2 >= 360) rot2 -= 360;
		if (rot2 < 0) rot2 += 360;
		
		if (IsActive() && rand.Next((int)(50-speed*10)) == 0) {
			int id = Dust.NewDust(GetDrawPos(),0,0,43,0f,0f,100,new Color(255,63,0,(byte)(alpha*255)),0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	public override Vector2 GetDrawPos() {
		float hover = IsActive() ? 16f : 2f;
		Vector2 pos2 = new Vector2(this.pos.X,this.pos.Y);
		switch (type) {
			case TYPE_SIN: {
				pos2.Y += (float)(Math.Sin(sin*Math.PI/180)*hover);
			} break;
			case TYPE_SIN2: {
				pos2.Y += (float)(Math.Sin((Math.Sin(sin*Math.PI/180)*180+180)*Math.PI/180)*hover);
			} break;
			default: break;
		}
		return pos2;
	}
	
	protected override void OnDraw(SpriteBatch sb) {
		Vector2 pos2 = GetDrawPos();
		Color c1 = new Color(color1.R,color1.G,color1.B,(byte)(alpha*255));
		Color c2 = new Color(color2.R,color2.G,color2.B,(byte)(alpha*255));
		float size = (float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180)));
		
		sb.Draw(ptStar,pos2-Main.screenPosition,GetRectFuzzy(),new Color(c1.R,c1.G,c1.B,(byte)(c1.A/2f)),(float)(rot1*Math.PI/180f),GetCenterFuzzy(),GetScaleFuzzy(size),SpriteEffects.None,0f);
		sb.Draw(ptStar,pos2-Main.screenPosition,GetRectFuzzy(),c2,(float)(rot2*Math.PI/180f),GetCenterFuzzy(),GetScaleFuzzy(size/2f),SpriteEffects.None,0f);
		if (addLight) {
			Lighting.addLight((int)Math.Round(pos2.X/16f),(int)Math.Round(pos2.Y/16f),c1.R/255f*size/2f/32f*c1.A/255f,c1.G/255f*size/2f/32f*c1.A/255f,c1.B/255f*size/2f/32f*c1.A/255f);
			Lighting.addLight((int)Math.Round(pos2.X/16f),(int)Math.Round(pos2.Y/16f),c2.R/255f*size/2f/64f*c2.A/255f,c2.G/255f*size/2f/64f*c2.A/255f,c2.B/255f*size/2f/64f*c2.A/255f);
		}
	}
	
	public override void OnCatch(Player player) {
		if (Main.netMode == 2 || player.whoAmi != Main.myPlayer) return;
		if (ModWorld.AcAchieve != null) {
			ModWorld.AcAchieve("SHK_WORLDP_FLY_FLAME",null);
			if (color1.G == 255) ModWorld.AcAchieve("SHK_WORLDP_FLY_FLAME2",null);
		}
	}
	public override void AffixName(ref string name) {
		name = (color1.G == 255 ? "Cursed " : "")+"Flame "+name;
	}
	public override void RefreshItemValue(ref int value, ref int rare) {
		rare += 1;
		value *= 5;
		
		if (color1.G == 255) {
			rare += 2;
			value *= 10;
		}
		
		base.RefreshItemValue(ref value,ref rare);
	}
}