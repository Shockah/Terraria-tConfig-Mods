[Serializable] public class EffectFireflyStandard : EffectFireflyHover {
	public static int GetMaxCount(List<Player> players) {
		int ret = 0, cur = 3, nextTier = 1, i = 0;
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
		return player.zone["Overworld"] && !player.zone["Hallow"] && !player.zone["Corruption"] && !player.zone["Jungle"] && !player.zone["Snow"];
	}
	
	protected Vector2 moved = new Vector2();
	
	public EffectFireflyStandard(Random rand) : base(rand) {Init();}
	public EffectFireflyStandard(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		speed = (float)Math.Sqrt(speed/1.5f+2f);
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
	}
	
	protected override void OnCreate() {
		base.OnCreate();
		int y = FindTileBelow((int)(pos.X/16),(int)(pos.Y/16));
		if (y != -1) pos.Y = y*16f-(height*16f);
	}
	protected override void OnDestroy() {
		Color c = Color.GreenYellow;
		
		int dusts = (int)(speed*30);
		for (int i = 0; i < dusts; i++) {
			int id = Dust.NewDust(GetDrawPos(),0,0,43,0f,0f,100,c,0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	
	protected override void OnUpdate(List<Player> players) {
		Vector2 opos = new Vector2(pos.X,pos.Y);
		if (IsActive()) OnUpdateMove(players,3);
		moved += pos-opos;
		base.OnUpdate(players);
		
		if (IsActive() && rand.Next((int)(50-speed*10)) == 0) {
			Color c = Color.GreenYellow;
			
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
		OnDrawOne(sb,pos2,(float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180))),1f);
	}
	protected virtual void OnDrawOne(SpriteBatch sb, Vector2 pos, float size, float alpha) {
		Color c = Color.GreenYellow;
		
		float sizeAmp = !IsActive() ? 1.5f : 1f;
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),new Color(c.R,c.G,c.B,(byte)(c.A/2f)),0f,GetCenterFuzzy(),GetScaleFuzzy(size*sizeAmp),SpriteEffects.None,0f);
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),c,0f,GetCenterFuzzy(),GetScaleFuzzy(size*sizeAmp/2f),SpriteEffects.None,0f);
		float lightAmp = !IsActive() ? 2f : 1f;
		if (addLight) Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),c.R/255f*size/32f*c.A/255f*lightAmp,c.G/255f*size/32f*c.A/255f*lightAmp,c.B/255f*size/32f*c.A/255f*lightAmp);
	}
	
	public override void OnCatch(Player player) {
		if (Main.netMode == 2 || player.whoAmi != Main.myPlayer) return;
		if (ModWorld.AcAchieve != null) {
			ModWorld.AcAchieve("SHK_WORLDP_FLY_STANDARD",null);
		}
	}
	/*public override void AffixName(ref string name) {
		name = GetColorName()+" "+name;
	}*/
}