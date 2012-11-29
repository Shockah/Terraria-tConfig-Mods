public class QuestMonster:Quest {
	private class QuestGenNPC {
		public readonly NPC npc;
		public readonly int amount;
		
		public QuestGenNPC(NPC npc, int amount) {
			this.npc = npc;
			this.amount = amount;
		}
	}
	
	public static Dictionary<string,int> presetValues = new Dictionary<string,int>();
	
	static QuestMonster() {
		int divby = 50;
		presetValues["Eater of Worlds"] = 15006/divby;
		presetValues["Eye of Cthulhu"] = 30000/divby;
		presetValues["Skeletron"] = 50000/divby;
		presetValues["Wall of Flesh"] = 80000/divby;
		presetValues["The Destroyer"] = 120000/divby;
		presetValues["Skeletron Prime"] = 120000/divby;
		presetValues["Retinazer"] = 50000/divby;
		presetValues["Spazmatism"] = 50000/divby;
	}
	
	public static QuestMonster getRandomQuest(Player player, bool boss) {
		int playerValue = ModGeneric.getPlayerValue(player);
		List<QuestGenNPC> list = new List<QuestGenNPC>();
		
		foreach (KeyValuePair<string,NPC> pair in Config.npcDefs.byName) {
			NPC npc = pair.Value;
			if (npc.townNPC || npc.friendly || npc.dontTakeDamage) continue;
			if (getNPCValue(npc) < 1) continue;
			
			if (boss != isBoss(npc)) continue;
			
			int tAmount = getBestAmount(playerValue,npc);
			if (tAmount > 0) list.Add(new QuestGenNPC(npc,tAmount));
		}
		
		if (list.Count != 0) {
			int average = 0;
			for (int i = 0; i < list.Count; i++) average += (int)list[i].npc.value*list[i].amount;
			average = (int)(1d*average/list.Count);
			
			List<QuestGenNPC> list2 = new List<QuestGenNPC>();
			for (int i = 0; i < list.Count; i++) {
				if (list2.Count == 0) list2.Add(list[i]);
				else {
					int val1 = (int)Math.Abs(list[i].npc.value*list[i].amount-average), val2 = (int)Math.Abs(list2[0].npc.value*list2[0].amount-average);
					if (val1 == val2) list2.Add(list[i]);
					else if (val1 < val2) {
						list2.Clear();
						list2.Add(list[i]);
					}
				}
			}
			
			QuestGenNPC qgnpc = list2[list2.Count == 1 ? 0 : Main.rand.Next(list2.Count)];
			return new QuestMonster(qgnpc.amount,Reward.getRandomReward(getQuestValue(qgnpc)),qgnpc.npc);
		}
		return null;
	}
	private static bool isBoss(NPC npc) {
		if (npc.boss) return true;
		if (npc.displayName != null && npc.displayName != "") {
			if (presetValues.ContainsKey(npc.name)) return true;
			try {
				NPC npc2 = Config.npcDefs.byName[npc.displayName];
				if (npc2.boss) return true;
				if (presetValues.ContainsKey(npc2.name)) return true;
			} catch (Exception) {}
		}
		return false;
	}
	private static int getNPCValue(NPC npc) {
		return (int)(presetValues.ContainsKey(npc.name) ? presetValues[npc.name] : (npc.displayName != null && npc.displayName != "" && presetValues.ContainsKey(npc.displayName) ? presetValues[npc.displayName] : npc.value))/2;
	}
	private static int getBestAmount(int playerValue, NPC npc) {
		return (int)(1d*playerValue/getNPCValue(npc));
	}
	private static int getQuestValue(QuestGenNPC qgnpc) {
		return (int)(getNPCValue(qgnpc.npc)*(isBoss(qgnpc.npc) ? 5 : 1)*qgnpc.amount*(20d+Main.rand.NextDouble()*10d));
	}
	
	public readonly NPC kill;
	
	public QuestMonster(int toKill, Reward reward, NPC kill) : base(toKill,reward) {
		this.kill = kill;
	}
	
	public override string questText() {
		string npcName = (kill.displayName != null && kill.displayName != "") ? kill.displayName : kill.name;
		return npcName+": "+progress+"/"+progressTotal;
	}
	public override int questType() {
		return 0;
	}
	public override string questTypeName() {
		return "Kill";
	}
	public override void save(BinaryWriter bw) {
		base.save(bw);
		bw.Write(kill.name);
		bw.Write(progress);
	}
}