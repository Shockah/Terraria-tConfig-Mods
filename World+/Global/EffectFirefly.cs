[Serializable] public abstract class EffectFirefly : EffectPositionable {
	public readonly Random rand;
	protected float speed, size, alpha, sinSize, sinSizeSpeed, maxSinSize;
	protected List<Vector2> posList = new List<Vector2>();
	protected bool killMe = false, addLight = true;
	
	public EffectFirefly(Random rand) : base() {
		this.rand = rand;
		Init();
	}
	public EffectFirefly(int id, Random rand) : base(id) {
		this.rand = rand;
		Init();
	}
	private void Init() {
		speed = (float)(.5d+rand.NextDouble()*1.5d);
		size = (float)(12f+rand.NextDouble()*20d);
		sinSize = (float)(rand.NextDouble()*360d);
		sinSizeSpeed = (float)((15d+rand.NextDouble()*30d)*rand.Next(2) == 1 ? 1 : -1);
		alpha = 1f;
		maxSinSize = .25f;
	}
	
	public EffectFirefly Clone() {
		EffectFirefly ret = null;
		using (MemoryStream ms = new MemoryStream()) {
			BinaryWriter bw = new BinaryWriter(ms);
			NetworkHelper.EasySerialize(bw,rand);
			Save(bw);
			
			ms.Position = 0;
			BinaryReader br = new BinaryReader(ms);
			ret = ModWorld.fireflies.CreateFirefly(GetType(),(Random)NetworkHelper.EasyDeserialize(br));
			ret.Load(br);
		}
		return ret;
	}
	
	public virtual void Save(BinaryWriter bw) {
		bw.Write(speed);
		bw.Write(size);
		bw.Write(sinSize);
		bw.Write(sinSizeSpeed);
		bw.Write(alpha);
		bw.Write(maxSinSize);
	}
	public virtual void Load(BinaryReader br) {
		speed = br.ReadSingle();
		size = br.ReadSingle();
		sinSize = br.ReadSingle();
		sinSizeSpeed = br.ReadSingle();
		alpha = br.ReadSingle();
		maxSinSize = br.ReadSingle();
	}
	
	protected int FindTileBelow(int x, int y) {
		int ys = y;
		while (true) {
			if (IsTileSolid(x,y)) {
				if (y == ys) {
					int dif = 0;
					while (true) {
						dif++;
						if (!IsTileSolid(x,y-dif)) return y-dif+1;
						if (IsTileSolid(x,y+dif)) return y+dif;
					}
				}
				return y;
			}
			y++;
			if (y >= Main.tile.GetLength(1)) return -1;
		}
	}
	public virtual bool IsTileSolid(int x, int y) {
		if (x < 0 || y < 0 || x > Main.tile.GetLength(0) || y > Main.tile.GetLength(1)) return false;
		Tile tile = Main.tile[x,y];
		return tile != null && ((tile.active && Main.tileSolid[tile.type]) || (tile.liquid >= 16 && tile.lava));
	}
	
	protected override void OnCreate() {
		alpha = 0f;
	}
	public virtual void Spawn() {
		if (Main.netMode == 2) {
			using (MemoryStream ms = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(ms)) {
				bw.Write((byte)fireflies.mapFireflyIdByType[GetType()]);
				NetworkHelper.EasySerialize(bw,rand);
				NetworkHelper.Write(bw,pos);
				NetworkHelper.Send(NetworkHelper.FLYSPAWN,ms);
			}
		}
	}
	
	protected override void OnUpdate(List<Player> players) {
		sinSize += sinSizeSpeed;
		if (sinSize >= 360) sinSize -= 360;
		if (sinSize < 0) sinSize += 360;
	}
	protected virtual bool HandleLife(List<Player> players) {
		if (ShouldDie(players)) {
			alpha -= .05f;
			if (alpha <= 0f) Destroy();
			return false;
		} else {
			alpha += .05f;
			if (alpha > 1f) alpha = 1f;
		}
		return true;
	}
	protected virtual bool ShouldDie(List<Player> players) {
		Player close = null;
		double closeDist = -1;
		foreach (Player player in players) {
			double d = Vector2.Distance(pos,player.position);
			if (close == null || d < closeDist) {
				close = player;
				closeDist = d;
			}
		}
		
		return closeDist > 1920 || killMe;
	}
	
	public virtual Vector2 GetDrawPos() {
		return pos;
	}
	
	public virtual void DrawItem(SpriteBatch sb, float x, float y, float scale) {
		Vector2 oPos = pos;
		float oSize = size;
		
		pos = new Vector2(x+52/2*scale,y+52/2*scale)+Main.screenPosition;
		size *= scale;
		addLight = false;
		Draw(sb);
		
		pos = oPos;
		size = oSize;
		addLight = true;
	}
	public virtual void DrawTile(SpriteBatch sb, Vector2 drawPos) {
		Vector2 oPos = pos;
		pos = drawPos;
		Draw(sb);
		pos = oPos;
	}
	
	public virtual void OnCatch(Player player) {}
	public virtual void AffixName(ref string name) {}
	public virtual void RefreshItemValue(ref int value, ref int rare) {
		value = (int)(value*size/16f);
	}
}