[Serializable] public class EffectFireflyCorrupt : EffectFireflyTargets {
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
		return player.zone["Corruption"] && (player.zone["DirtLayer"] || player.zone["RockLayer"]);
	}
	
	protected Color color;
	protected float rot1, rotSpeed1, rot2, rotSpeed2;
	protected bool chaos;
	
	public EffectFireflyCorrupt(Random rand) : base(rand) {Init();}
	public EffectFireflyCorrupt(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		size *= 1.5f;
		speed *= 1.5f;
		sinSizeSpeed *= 2.5f;
		maxSinSize = 1f/3f;
		
		int r,g,b;
		HsvToRgb((float)(240f+rand.NextDouble()*45f),1f,1f,out r,out g,out b);
		color = new Color(r,g,b);
		
		rot1 = (float)(rand.NextDouble()*360d);
		rotSpeed1 = (float)((1d+rand.NextDouble()*2d)*rand.Next(2) == 1 ? 1 : -1);
		rot2 = (float)(rand.NextDouble()*360d);
		rotSpeed2 = (float)((3d+rand.NextDouble()*6d)*Math.Sign(-rotSpeed1));
		chaos = rand.Next(Settings.GetInt("rarity")) == 0;
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
		NetworkHelper.Write(bw,color);
		bw.Write(rot1);
		bw.Write(rotSpeed1);
		bw.Write(rot2);
		bw.Write(rotSpeed2);
		bw.Write(chaos);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
		color = NetworkHelper.ReadColor(br);
		rot1 = br.ReadSingle();
		rotSpeed1 = br.ReadSingle();
		rot2 = br.ReadSingle();
		rotSpeed2 = br.ReadSingle();
		chaos = br.ReadBoolean();
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
		
		rot1 += rotSpeed1;
		if (rot1 >= 360) rot1 -= 360;
		if (rot1 < 0) rot1 += 360;
		
		rot2 += rotSpeed2;
		if (rot2 >= 360) rot2 -= 360;
		if (rot2 < 0) rot2 += 360;
		
		if (IsActive() && rand.Next((int)(50-speed*10)) == 0) {
			int id = Dust.NewDust(pos,0,0,43,0f,0f,100,chaos ? new Color(0,0,0,(byte)(alpha*255)) : new Color(color.R,color.G,color.B,(byte)(alpha*255)),0.3f);
			Main.dust[id].fadeIn = 0.8f;
		}
	}
	protected override List<Target> GetAllTargets(List<Player> players) {
		List<Target> targets = new List<Target>();
		for (int i = 0; i < ModWorld.listCorruptTargets.Count; i++) {
			Vector2 v = ModWorld.listCorruptTargets[i];
			int x = (int)v.X, y = (int)v.Y;
			Tile tile = Main.tile[x,y];
			if (tile == null) continue;
			
			if (Main.netMode != -1) {
				if (tile.active && ((tile.type == 26 && tile.frameX == 0 && tile.frameY == 0) || (tile.type == 31 && tile.frameX == 0 && tile.frameY == 0))) {
					targets.Add(new TargetTile(new Vector2(x,y)));
				} else {
					ModWorld.listCorruptTargets.RemoveAt(i--);
					if (Main.netMode == 2) {
						using (MemoryStream ms = new MemoryStream())
						using (BinaryWriter bw = new BinaryWriter(ms)) {
							bw.Write(false);
							NetworkHelper.Write(bw,v);
							NetworkHelper.Send(NetworkHelper.CORRUPTTARGET,ms);
						}
					}
					continue;
				}
			} else targets.Add(new TargetTileBig(new Vector2(x,y),tile.type == 26 ? 3 : 2,2));
		}
		return targets;
	}
	
	protected override void OnDraw(SpriteBatch sb) {
		Color c = new Color(color.R,color.G,color.B,(byte)(alpha*255));
		float size = (float)(this.size+(maxSinSize*this.size*Math.Sin(sinSize*Math.PI/180)));
		if (chaos) {
			sb.End();
			sb.Begin(SpriteSortMode.Immediate,bsSubtract);
			
			c = new Color(255,255,255);
			if (addLight) size *= 3f;
			sb.Draw(ptSquare,pos-Main.screenPosition,GetRectSquare(),new Color(c.R,c.G,c.B,(byte)(c.A/2f)),(float)(rot1*Math.PI/180f),GetCenterSquare(),GetScaleSquare(size),SpriteEffects.None,0f);
			sb.Draw(ptSquare,pos-Main.screenPosition,GetRectSquare(),c,(float)(rot2*Math.PI/180f),GetCenterSquare(),GetScaleSquare(size/2f),SpriteEffects.None,0f);
			if (addLight) size /= 3f;
			
			sb.End();
			sb.Begin(SpriteSortMode.Immediate,BlendState.Additive);
			
			if (addLight) Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),c.R/255f*size/32f*c.A/255f/3f,c.G/255f*size/32f*c.A/255f/3f,c.B/255f*size/32f*c.A/255f/3f);
		} else {
			sb.Draw(ptSquare,pos-Main.screenPosition,GetRectSquare(),new Color(c.R,c.G,c.B,(byte)(c.A/2f)),(float)(rot1*Math.PI/180f),GetCenterSquare(),GetScaleSquare(size),SpriteEffects.None,0f);
			sb.Draw(ptSquare,pos-Main.screenPosition,GetRectSquare(),c,(float)(rot2*Math.PI/180f),GetCenterSquare(),GetScaleSquare(size/2f),SpriteEffects.None,0f);
			if (addLight) Lighting.addLight((int)Math.Round(pos.X/16f),(int)Math.Round(pos.Y/16f),c.R/255f*size/32f*c.A/255f,c.G/255f*size/32f*c.A/255f,c.B/255f*size/32f*c.A/255f);
		}
	}
	
	public override void OnCatch(Player player) {
		if (Main.netMode == 2 || player.whoAmi != Main.myPlayer) return;
		if (ModWorld.AcAchieve != null) {
			ModWorld.AcAchieve("SHK_WORLDP_FLY_CORRUPT",null);
			if (chaos) ModWorld.AcAchieve("SHK_WORLDP_FLY_CORRUPT2",null);
		}
	}
	public override void AffixName(ref string name) {
		name = (chaos ? "Chaos" : "Corrupt")+" "+name;
	}
	public override void RefreshItemValue(ref int value, ref int rare) {
		rare++;
		value *= 2;
		
		if (chaos) {
			rare++;
			value *= 10;
		}
		
		base.RefreshItemValue(ref value,ref rare);
	}
}