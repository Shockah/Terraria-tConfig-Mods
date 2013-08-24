[Serializable] public class EffectFireflyMoon : EffectFireflyTargets {
	public static int GetMaxCount(List<Player> players) {
		int ret = 0, cur = 4, nextTier = 1, i = 0;
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
		return player.zone["Jungle"] && player.zone["RockLayer"];
	}
	
	protected Color color;
	protected bool hasRing;
	
	public EffectFireflyMoon(Random rand) : base(rand) {Init();}
	public EffectFireflyMoon(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		size *= 1.5f;
		speed *= 1.5f;
		sinSizeSpeed *= 2.5f;
		maxSinSize = 1f/3f;
		
		int r,g,b;
		HsvToRgb((float)(120f+rand.NextDouble()*60f),1f,1f,out r,out g,out b);
		color = new Color(r,g,b);
		
		hasRing = rand.Next(Settings.GetInt("rarity")) == 0;
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
		NetworkHelper.Write(bw,color);
		bw.Write(hasRing);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
		color = NetworkHelper.ReadColor(br);
		hasRing = br.ReadBoolean();
	}
	
	protected override void OnDestroy() {
		int dusts = (int)(speed*30);
		for (int i = 0; i < dusts; i++) {
			int id = Dust.NewDust(pos,0,0,43,0f,0f,100,color,0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	
	protected override void OnUpdate(List<Player> players) {
		base.OnUpdate(players);
		
		if (IsActive() && rand.Next((int)(50-speed*10)) == 0) {
			int id = Dust.NewDust(pos,0,0,43,0f,0f,100,new Color(color.R,color.G,color.B,(byte)(alpha*255)),0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	protected override List<Target> GetAllTargets(List<Player> players) {
		List<Target> targets = new List<Target>();
		for (int i = 0; i < ModWorld.listJungleTargets.Count; i++) {
			Vector2 v = ModWorld.listJungleTargets[i];
			int x = (int)v.X, y = (int)v.Y;
			if (Main.tile[x,y] == null) continue;
			if (Main.netMode != -1 && !(Main.tile[x,y].active && Main.tile[x,y].type == 61 && Main.tile[x,y].frameX == 144)) {
				ModWorld.listJungleTargets.RemoveAt(i--);
				
				if (Main.netMode == 2) {
					using (MemoryStream ms = new MemoryStream())
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						bw.Write(false);
						NetworkHelper.Write(bw,v);
						NetworkHelper.Send(NetworkHelper.JUNGLETARGET,ms);
					}
				}
			} else targets.Add(new TargetTile(new Vector2(x,y)));
		}
		foreach (Player player in players) targets.Add(new TargetPlayer(player));
		return targets;
	}
	
	protected override void OnDraw(SpriteBatch sb) {
		Color c = new Color(color.R,color.G,color.B,(byte)(alpha*255));
		float size = (float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180)));
		
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),new Color(c.R,c.G,c.B,(byte)(c.A/2f)),0f,GetCenterFuzzy(),GetScaleFuzzy(size),SpriteEffects.None,0f);
		sb.Draw(ptFuzzy,pos-Main.screenPosition,GetRectFuzzy(),c,0f,GetCenterFuzzy(),GetScaleFuzzy(size/2f),SpriteEffects.None,0f);
		if (hasRing) sb.Draw(ptRing,pos-Main.screenPosition,GetRectRing(),c,0f,GetCenterRing(),GetScaleRing(size*.8f),SpriteEffects.None,0f);
		if (addLight) Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),c.R/255f*size/32f*c.A/255f,c.G/255f*size/32f*c.A/255f,c.B/255f*size/32f*c.A/255f);
	}
	
	public override void OnCatch(Player player) {
		if (Main.netMode == 2 || player.whoAmi != Main.myPlayer) return;
		if (ModWorld.AcAchieve != null) {
			ModWorld.AcAchieve("SHK_WORLDP_FLY_MOON",null);
			if (hasRing) ModWorld.AcAchieve("SHK_WORLDP_FLY_MOON2",null);
		}
	}
	public override void AffixName(ref string name) {
		name = (hasRing ? "Lively ": "")+"Moon "+name;
	}
	public override void RefreshItemValue(ref int value, ref int rare) {
		value *= 2;
		
		if (hasRing) {
			rare += 2;
			value *= 10;
		}
		
		base.RefreshItemValue(ref value,ref rare);
	}
}