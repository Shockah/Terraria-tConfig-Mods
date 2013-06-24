[Serializable] public abstract class EffectFireflyHover : EffectFirefly {
	protected int dir, height;
	protected float sin, sinSpeed;
	protected Vector2 vel = new Vector2();
	
	public EffectFireflyHover(Random rand) : base(rand) {Init();}
	public EffectFireflyHover(int id, Random rand) : base(id,rand) {Init();}
	private void Init() {
		dir = rand.Next(2) == 1 ? 1 : -1;
		sin = (float)(rand.NextDouble()*360d);
		sinSpeed = (float)((1d+rand.NextDouble()*9d)*rand.Next(2) == 1 ? 1 : -1);
		height = 3+rand.Next(2);
	}
	
	public override void Save(BinaryWriter bw) {
		base.Save(bw);
		bw.Write(dir == 1);
		bw.Write((byte)height);
		bw.Write(sin);
		bw.Write(sinSpeed);
	}
	public override void Load(BinaryReader br) {
		base.Load(br);
		dir = br.ReadBoolean() ? 1 : -1;
		height = br.ReadByte();
		sin = br.ReadSingle();
		sinSpeed = br.ReadSingle();
	}
	
	protected override void OnCreate() {
		base.OnCreate();
		int y = FindTileBelow((int)(pos.X/16),(int)(pos.Y/16));
		if (y != -1) pos.Y = y*16f-(height*16f);
	}
	
	protected override void OnUpdate(List<Player> players) {
		base.OnUpdate(players);
		
		sin += sinSpeed;
		if (sin >= 360) sin -= 360;
		if (sin < 0) sin += 360;
	}
	protected virtual void OnUpdateMove(List<Player> players, int tiles) {
		int x = (int)(pos.X/16), y = (int)(pos.Y/16);
		if (IsTileSolid(x,y)) {
			int y2;
			
			y2 = (int)(pos.Y-1/16);
			if (y2 != y && !IsTileSolid(x,y2)) {
				pos.Y -= 1;
				y = y2;
			} else {
				y2 = (int)(pos.Y+1/16);
				if (y2 != y && !IsTileSolid(x,y2)) {
					pos.Y += 1;
					y = y2;
				} else killMe = true;
			}
		}
		
		float speedMod = 1f;
		Tile tile = Main.tile[(int)(pos.X/16),(int)(pos.Y/16)];
		if (tile == null) return;
		if (tile.liquid > 0 && pos.Y > pos.Y/16+1-1f/tile.liquid) speedMod *= .5f;
		
		int iSpeed = (int)(200*speed);
		if (posList.Count == iSpeed) posList.RemoveAt(0);
		posList.Add(new Vector2(pos.X,pos.Y));
		if (posList.Count == iSpeed) {
			Vector2 diff = new Vector2();
			foreach (Vector2 oldPos in posList) {
				diff.X += pos.X-oldPos.X;
				diff.Y += pos.Y-oldPos.Y;
			}
			diff /= 1f*iSpeed;
			if (diff.Length() < speed*speedMod*8f) killMe = true;
		}
		
		int[] yys = new int[tiles+1];
		for (int xx = 0; xx <= tiles; xx++) yys[xx] = FindTileBelow(x+xx*dir,y);
		
		int yMin = -1;
		foreach (int yy in yys) if (yy != -1) if (yMin == -1 || yy < yMin) yMin = yy;
		if (yMin != -1) while (y-yMin > tiles/2f) yMin++;
		Vector2 target = yMin == -1 ? new Vector2(-1,-1) : new Vector2(pos.X+speed*dir,yMin*16+8-height*16f);
		
		if (!HandleLife(players)) return;
		
		if (target.X < 0 || target.Y < 0) {
			pos += vel*speedMod;
		} else {
			Vector2 move = Vector(speed*speedMod/32f,Direction(pos,target));
			vel += move;
			if (vel.Length() > speed*speedMod) vel *= speed*speedMod/vel.Length();
			
			Vector2 toMove = new Vector2(vel.X,vel.Y);
			Vector2 lastTile = new Vector2((int)(pos.X/16f),(int)(pos.Y/16f));
			Vector2 moveTo, newTile, pos2;
			while (toMove.X != 0 || toMove.Y != 0) {
				if (toMove.X != 0) {
					float xx = Math.Abs(toMove.X) <= 1 ? toMove.X : Math.Sign(toMove.X);
					toMove.X -= xx;
					
					pos2 = GetDrawPos();
					moveTo = new Vector2(pos2.X+xx,pos2.Y);
					newTile = new Vector2((int)(moveTo.X/16f),(int)(moveTo.Y/16f));
					if (lastTile.X != newTile.X) {
						lastTile = newTile;
						if (IsTileSolid((int)(moveTo.X/16f),(int)(moveTo.Y/16f))) {
							xx *= -1;
							vel.X *= -1;
							toMove.X *= -1;
							dir *= -1;
							
							int c = posList.Count/2;
							while (c-- > 0) posList.RemoveAt(0);
						}
					}
					pos.X += xx;
				}
				if (toMove.Y != 0) {
					float yy = Math.Abs(toMove.Y) <= 1 ? toMove.Y : Math.Sign(toMove.Y);
					toMove.Y -= yy;
					
					pos2 = GetDrawPos();
					moveTo = new Vector2(pos2.X,pos2.Y+yy);
					newTile = new Vector2((int)(moveTo.X/16f),(int)(moveTo.Y/16f));
					if (lastTile.Y != newTile.Y) {
						lastTile = newTile;
						if (IsTileSolid((int)(moveTo.X/16f),(int)(moveTo.Y/16f))) {
							yy *= -1;
							vel.Y *= -1;
							toMove.Y *= -1;
						}
					}
					pos.Y += yy;
				}
			}
		}
	}
}