public class NetworkHelper {
	public const int
		SYNCCONNECT = 1,
		FLYSPAWN = 2,
		FLYSPAWNDETAILED = 3,
		FLYCATCH = 4,
		FLYPLACE = 5,
		JUNGLETARGET = 6,
		CORRUPTTARGET = 7;
	
	public static int modId;
	private static System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
	
	public static void Send(int message, MemoryStream ms) {Send(-1,message,ms);}
	public static void Send(Player player, int message, MemoryStream ms) {Send(player == null ? player.whoAmi : -1,message,ms);}
	public static void Send(int playerId, int message, MemoryStream ms) {SendIgnore(playerId,-1,message,ms);}
	public static void SendIgnore(int ignoreId, int message, MemoryStream ms) {SendIgnore(-1,ignoreId,message,ms);}
	public static void SendIgnore(Player ignore, int message, MemoryStream ms) {SendIgnore(-1,ignore == null ? ignore.whoAmi : -1,message,ms);}
	public static void SendIgnore(Player player, int ignoreId, int message, MemoryStream ms) {SendIgnore(player == null ? player.whoAmi : -1,ignoreId,message,ms);}
	public static void SendIgnore(Player player, Player ignore, int message, MemoryStream ms) {SendIgnore(player == null ? player.whoAmi : -1,ignore == null ? ignore.whoAmi : -1,message,ms);}
	public static void SendIgnore(int playerId, int ignoreId, int message, MemoryStream ms) {
		byte[] data = ms.ToArray();
		object[] toSend = new object[data.Length];
		for (int i = 0; i < data.Length; i++) toSend[i] = data[i];
		NetMessage.SendModData(modId,message,playerId,ignoreId,toSend);
	}
	
	public static MemoryStream Serialize(Object o) {
		MemoryStream ms = new MemoryStream();
		formatter.Serialize(ms,o);
		return ms;
	}
	public static void Serialize(BinaryWriter bw, Object o) {
		formatter.Serialize(bw.BaseStream,o);
	}
	public static void EasySerialize(BinaryWriter bw, Object o) {
		MemoryStream ms = Serialize(o);
		bw.Write((int)ms.Length);
		bw.Write(ms.ToArray());
		ms.Close();
	}
	public static Object Deserialize(BinaryReader br) {
		return formatter.Deserialize(br.BaseStream);
	}
	public static Object EasyDeserialize(BinaryReader br) {
		MemoryStream ms = new MemoryStream(br.ReadBytes(br.ReadInt32()));
		ms.Position = 0;
		Object ret = formatter.Deserialize(ms);
		ms.Close();
		return ret;
	}
	
	public static void Write(BinaryWriter bw, Vector2 v) {
		bw.Write(v.X);
		bw.Write(v.Y);
	}
	public static Vector2 ReadVector2(BinaryReader br) {
		return new Vector2(br.ReadSingle(),br.ReadSingle());
	}
	
	public static void Write(BinaryWriter bw, Color c) {
		bw.Write(c.R);
		bw.Write(c.G);
		bw.Write(c.B);
		bw.Write(c.A);
	}
	public static Color ReadColor(BinaryReader br) {
		return new Color(br.ReadByte(),br.ReadByte(),br.ReadByte(),br.ReadByte());
	}
}

public void PlayerConnected(int playerId) {
	using (MemoryStream ms = new MemoryStream())
	using (BinaryWriter bw = new BinaryWriter(ms)) {
		bw.Write(listJungleTargets.Count);
		foreach (Vector2 v in listJungleTargets) NetworkHelper.Write(bw,v);
		
		bw.Write(listCorruptTargets.Count);
		foreach (Vector2 v in listCorruptTargets) NetworkHelper.Write(bw,v);
		
		NetworkHelper.EasySerialize(bw,effects);
		fireflies.Save(bw);
		
		NetworkHelper.Send(NetworkHelper.SYNCCONNECT,ms);
	}
}

public void NetReceive(int messageType, BinaryReader br) {
	switch (Main.netMode) {
		case 1: {
			switch (messageType) {
				case NetworkHelper.SYNCCONNECT: {
					int count;
					
					count = br.ReadInt32();
					while (count-- > 0) listJungleTargets.Add(NetworkHelper.ReadVector2(br));
					
					count = br.ReadInt32();
					while (count-- > 0) listCorruptTargets.Add(NetworkHelper.ReadVector2(br));
					
					effects = (List<Effect>)NetworkHelper.EasyDeserialize(br);
					fireflies.Load(br);
				} break;
				case NetworkHelper.FLYSPAWN: {
					fireflies.CreateFirefly(br.ReadByte(),(Random)NetworkHelper.EasyDeserialize(br)).Create(NetworkHelper.ReadVector2(br));
				} break;
				case NetworkHelper.FLYSPAWNDETAILED: {
					fireflies.LoadOne(br).Create(NetworkHelper.ReadVector2(br));
				} break;
				case NetworkHelper.FLYCATCH: {
					int id = br.ReadInt32();
					for (int i = 0; i < effects.Count; i++) {
						Effect e = effects[i];
						if (e.id == id) {
							e.Destroy();
							break;
						}
					}
				} break;
				case NetworkHelper.FLYPLACE: {
					fireflies.SetAt(br.ReadInt32(),br.ReadInt32(),br.ReadBoolean() ? fireflies.LoadOne(br) : null);
				} break;
				case NetworkHelper.JUNGLETARGET: {
					if (br.ReadBoolean()) listJungleTargets.Add(NetworkHelper.ReadVector2(br));
					else listJungleTargets.Remove(NetworkHelper.ReadVector2(br));
				} break;
				case NetworkHelper.CORRUPTTARGET: {
					if (br.ReadBoolean()) listCorruptTargets.Add(NetworkHelper.ReadVector2(br));
					else listCorruptTargets.Remove(NetworkHelper.ReadVector2(br));
				} break;
			}
		} break;
		case 2: {
			switch (messageType) {
				case NetworkHelper.FLYSPAWNDETAILED: {
					int pId = br.ReadByte();
					
					EffectFirefly firefly = fireflies.LoadOne(br);
					Vector2 pos = NetworkHelper.ReadVector2(br);
					firefly.Create(pos);
					
					using (MemoryStream ms = new MemoryStream())
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						fireflies.SaveOne(bw,firefly);
						NetworkHelper.Write(bw,pos);
						NetworkHelper.SendIgnore(pId,NetworkHelper.FLYSPAWNDETAILED,ms);
					}
				} break;
				case NetworkHelper.FLYCATCH: {
					int pId = br.ReadByte();
					int id = br.ReadInt32();
					for (int i = 0; i < effects.Count; i++) {
						Effect e = effects[i];
						if (e.id == id) {
							e.Destroy();
							break;
						}
					}
					
					using (MemoryStream ms = new MemoryStream())
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						bw.Write(id);
						NetworkHelper.SendIgnore(pId,NetworkHelper.FLYCATCH,ms);
					}
				} break;
				case NetworkHelper.FLYPLACE: {
					int pId = br.ReadByte();
					
					int x = br.ReadInt32();
					int y = br.ReadInt32();
					EffectFirefly firefly = br.ReadBoolean() ? fireflies.LoadOne(br) : null;
					fireflies.SetAt(x,y,firefly);
					
					using (MemoryStream ms = new MemoryStream())
					using (BinaryWriter bw = new BinaryWriter(ms)) {
						bw.Write(x);
						bw.Write(y);
						bw.Write(firefly != null);
						if (firefly != null) fireflies.SaveOne(bw,firefly);
						NetworkHelper.SendIgnore(pId,NetworkHelper.FLYCATCH,ms);
					}
				} break;
			}
		} break;
	}
}