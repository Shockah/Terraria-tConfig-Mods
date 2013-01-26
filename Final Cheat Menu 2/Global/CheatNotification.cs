public class CheatNotification {
	public static List<CheatNotification> list = new List<CheatNotification>();
	
	public static void Init() {
		list.Clear();
	}
	public static void Add(CheatNotification cn) {
		if (Main.netMode != 1) return;
		for (int i = 0; i < list.Count; i++) if (list[i].type == cn.type && list[i].delay > 0) {
			list.RemoveAt(i);
			break;
		}
		list.Add(cn);
	}
	public static void Update() {
		for (int i = 0; i < list.Count; i++) {
			CheatNotification cn = list[i];
			if (--cn.delay <= 0) {
				NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_CHEAT_NOTIFICATION,-1,-1,(byte)Main.myPlayer,cn.message);
				list.RemoveAt(i--);
			}
		}
	}
	
	public readonly string type, message;
	public int delay;
	
	public CheatNotification(string type, string message, int delay = 0) {
		this.type = type;
		this.message = message;
		this.delay = delay;
	}
}