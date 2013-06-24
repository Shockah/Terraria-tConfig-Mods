[Serializable] public abstract class EffectFireflyTargets : EffectFirefly {
	protected Vector2 vel = new Vector2();
	protected Target target = null;
	protected bool inWall = false;
	protected int timesMissed = 0;
	
	public EffectFireflyTargets(Random rand) : base(rand) {}
	public EffectFireflyTargets(int id, Random rand) : base(id,rand) {}
	
	protected override void OnCreate() {
		base.OnCreate();
		inWall = IsTileSolid((int)(pos.X/16f),(int)(pos.Y/16f));
	}
	
	protected override void OnUpdate(List<Player> players) {
		if (IsActive()) OnUpdateMove(players);
		base.OnUpdate(players);
	}
	protected virtual void OnUpdateMove(List<Player> players) {
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
			if (diff.Length() < speed*16f) {
				if (Vector2.Distance(pos+diff,target.GetPosition()) > 48) {
					timesMissed++;
					if (timesMissed >= 3) killMe = true;
					timesMissed++;
				}
				ChangeTarget(players);
			}
		}
		
		if (target == null || !target.IsActive() || Vector2.Distance(pos,target.GetPosition()) <= 16 || Vector2.Distance(pos,target.GetPosition()) > 1920) ChangeTarget(players);
		if (target != null && Vector2.Distance(pos,target.GetPosition()) > 1920) target = null;
		if (killMe && target != null && Vector2.Distance(pos,target.GetPosition()) < speed*32) {
			killMe = false;
			posList.Clear();
		}
		if (!HandleLife(players)) return;
		
		float speedMod = 1f;
		Tile tile = Main.tile[(int)(pos.X/16),(int)(pos.Y/16)];
		if (tile.liquid > 0 && pos.Y > pos.Y/16+1-1f/tile.liquid) speedMod *= .5f;
		
		if (target == null) {
			pos += vel*speedMod;
		} else {
			Vector2 move = Vector(speed*speedMod/32f,Direction(pos,target.GetPosition()));
			vel += move;
			if (vel.Length() > speed*speedMod) vel *= speed*speedMod/vel.Length();
			
			Vector2 toMove = new Vector2(vel.X,vel.Y);
			Vector2 lastTile = new Vector2((int)(pos.X/16f),(int)(pos.Y/16f));
			Vector2 moveTo, newTile;
			while (toMove.X != 0 || toMove.Y != 0) {
				if (toMove.X != 0) {
					float xx = Math.Abs(toMove.X) <= 1 ? toMove.X : Math.Sign(toMove.X);
					toMove.X -= xx;
					
					moveTo = new Vector2(pos.X+xx,pos.Y);
					newTile = new Vector2((int)(moveTo.X/16f),(int)(moveTo.Y/16f));
					if (lastTile.X != newTile.X) {
						lastTile = newTile;
						if (IsTileSolid((int)(moveTo.X/16f),(int)(moveTo.Y/16f))) {
							xx *= -1;
							vel.X *= -1;
							toMove.X *= -1;
						}
					}
					pos.X += xx;
				}
				if (toMove.Y != 0) {
					float yy = Math.Abs(toMove.Y) <= 1 ? toMove.Y : Math.Sign(toMove.Y);
					toMove.Y -= yy;
					
					moveTo = new Vector2(pos.X,pos.Y+yy);
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
	public override bool IsTileSolid(int x, int y) {
		bool b = base.IsTileSolid(x,y);
		if (inWall) {
			if (!b) inWall = false;
			return false;
		} else return b;
	}
	protected override bool ShouldDie(List<Player> players) {
		return base.ShouldDie(players) || target == null;
	}
	protected virtual void ChangeTarget(List<Player> players) {
		posList.Clear();
		if (timesMissed > 0) timesMissed--;
		List<Target> targets = GetAllTargets(players);
		List<Target> close = new List<Target>();
		foreach (Target target in targets) {
			if (Vector2.Distance(pos,target.GetPosition()) < 1920/3 && (this.target == null || Vector2.Distance(pos,target.GetPosition()) >= 192)) close.Add(target);
		}
		if (close.Count == 0) {
			this.target = null;
			return;
		}
		this.target = close[rand.Next(close.Count)];
	}
	protected abstract List<Target> GetAllTargets(List<Player> players);
	
	protected abstract class Target {
		public abstract bool IsActive();
		public abstract Vector2 GetPosition();
	}
	protected class TargetPos : Target {
		protected readonly Vector2 pos;
		public TargetPos(Vector2 pos) {this.pos = pos;}
		public override bool IsActive() {return true;}
		public override Vector2 GetPosition() {return pos;}
	}
	protected class TargetTile : Target {
		protected readonly Vector2 pos;
		public TargetTile(Vector2 pos) {this.pos = pos;}
		public override bool IsActive() {return Main.tile[(int)pos.X,(int)pos.Y].active;}
		public override Vector2 GetPosition() {return new Vector2(pos.X*16+8,pos.Y*16+8);}
	}
	protected class TargetTileBig : Target {
		protected readonly Vector2 pos, size;
		public TargetTileBig(Vector2 pos, int w, int h) {this.pos = pos; size = new Vector2(w,h);}
		public override bool IsActive() {return Main.tile[(int)pos.X,(int)pos.Y].active;}
		public override Vector2 GetPosition() {return new Vector2((pos.X+size.X/2f)*16,(pos.Y+size.Y/2f)*16);}
	}
	protected class TargetPlayer : Target {
		protected readonly Player player;
		public TargetPlayer(Player player) {this.player = player;}
		public override bool IsActive() {return player.active;}
		public override Vector2 GetPosition() {return player.Center;}
	}
}