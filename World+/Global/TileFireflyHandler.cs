public class TileFireflyHandler {
	public Dictionary<Type,int> mapFireflyIdByType;
	public Dictionary<int,Type> mapFireflyTypeById;
	protected List<TileFirefly> list = new List<TileFirefly>();
	
	public TileFireflyHandler() {
		mapFireflyIdByType = new Dictionary<Type,int>();
		mapFireflyTypeById = new Dictionary<int,Type>();
		
		int i = 1;
		RegisterFireflyType(typeof(EffectFireflyRainbow),i++);
		RegisterFireflyType(typeof(EffectFireflyFlame),i++);
		RegisterFireflyType(typeof(EffectFireflyMoon),i++);
		RegisterFireflyType(typeof(EffectFireflyCorrupt),i++);
	}
	
	private void RegisterFireflyType(Type type, int id) {
		mapFireflyIdByType.Add(type,id);
		mapFireflyTypeById.Add(id,type);
	}
	public EffectFirefly CreateFirefly(Type type, Random rand) {
		return (EffectFirefly)type.GetConstructor(new[]{typeof(Random)}).Invoke(new object[]{rand});
	}
	public EffectFirefly CreateFirefly(int id, Random rand) {
		return (EffectFirefly)mapFireflyTypeById[id].GetConstructor(new[]{typeof(Random)}).Invoke(new object[]{rand});
	}
	
	public void SetAt(int x, int y, EffectFirefly firefly) {
		if (firefly == null) {
			for (int i = 0; i < list.Count; i++) if (list[i].x == x && list[i].y == y) {
				list.RemoveAt(i);
				return;
			}
		} else {
			foreach (TileFirefly tf in list) if (tf.x == x && tf.y == y) {
				tf.firefly = firefly;
				return;
			}
			list.Add(new TileFirefly(x,y,firefly));
		}
	}
	public TileFirefly GetAt(int x, int y) {
		foreach (TileFirefly tf in list) if (tf.x == x && tf.y == y) return tf;
		return null;
	}
	public EffectFirefly GetFireflyAt(int x, int y) {
		TileFirefly tf = GetAt(x,y);
		if (tf == null) return null;
		return tf.firefly;
	}
	public void Clear() {
		list.Clear();
	}
	public void UpdateAll() {
		List<Player> players = new List<Player>();
		foreach (Player player in Main.player) if (player.active && !player.ghost && player.statLife > 0) players.Add(player);
		foreach (TileFirefly tf in list) tf.firefly.Update(players);
	}
	public void DrawAll(SpriteBatch sb, float offX, float offY) {
		foreach (TileFirefly tf in list) tf.firefly.DrawTile(sb,new Vector2(tf.x*16f+offX,tf.y*16f+offY));
	}
	
	public void Save(BinaryWriter bw) {
		bw.Write(list.Count);
		foreach (TileFirefly tf in list) {
			bw.Write(tf.x);
			bw.Write(tf.y);
			SaveOne(bw,tf.firefly);
		}
	}
	public void SaveOne(BinaryWriter bw, EffectFirefly firefly) {
		bw.Write((byte)mapFireflyIdByType[firefly.GetType()]);
		NetworkHelper.EasySerialize(bw,firefly.rand);
		firefly.Save(bw);
	}
	public void Load(BinaryReader br) {
		int count = br.ReadInt32();
		while (count-- > 0) {
			int x = br.ReadInt32();
			int y = br.ReadInt32();
			EffectFirefly firefly = LoadOne(br);
			list.Add(new TileFirefly(x,y,firefly));
		}
	}
	public EffectFirefly LoadOne(BinaryReader br) {
		EffectFirefly firefly = CreateFirefly(br.ReadByte(),(Random)NetworkHelper.EasyDeserialize(br));
		firefly.Load(br);
		return firefly;
	}
	
	public class TileFirefly {
		public readonly int x, y;
		public EffectFirefly firefly;
		
		public TileFirefly(int x, int y, EffectFirefly firefly) {
			this.x = x;
			this.y = y;
			this.firefly = firefly;
		}
	}
}