public class QuestGather:Quest {
	private class QuestGenItem {
		public readonly Item item;
		public readonly int amount;
		
		public QuestGenItem(Item item, int amount) {
			this.item = item;
			this.amount = amount;
		}
	}
	
	new public static QuestGather getRandomQuest(Player player) {
		int playerValue = ModGeneric.getPlayerValue(player);
		List<QuestGenItem> list = new List<QuestGenItem>();
		
		foreach (KeyValuePair<string,Item> pair in Config.itemDefs.byName) {
			Item item = pair.Value;
			if (item.name == "Unloaded Item") continue;
			if (item.value < 1) continue;
			if (item.maxStack <= 1) continue;
			if (item.damage > 0) continue;
			if (item.createTile != -1) continue;
			if (item.createWall != -1) continue;
			if (item.name.EndsWith(" Coin")) continue;
			
			int tAmount = getBestAmount(playerValue,item);
			if (tAmount > 0) list.Add(new QuestGenItem(item,tAmount));
		}
		
		if (list.Count != 0) {
			int average = 0;
			for (int i = 0; i < list.Count; i++) average += (int)list[i].item.value*list[i].amount;
			average = (int)(1d*average/list.Count);
			
			List<QuestGenItem> list2 = new List<QuestGenItem>();
			for (int i = 0; i < list.Count; i++) {
				if (list2.Count == 0) list2.Add(list[i]);
				else {
					int val1 = (int)Math.Abs(list[i].item.value*list[i].amount-average), val2 = (int)Math.Abs(list2[0].item.value*list2[0].amount-average);
					if (val1 == val2) list2.Add(list[i]);
					else if (val1 < val2) {
						list2.Clear();
						list2.Add(list[i]);
					}
				}
			}
			
			QuestGenItem qgitem = list2[list2.Count == 1 ? 0 : Main.rand.Next(list2.Count)];
			while (true) {
				Reward r = Reward.getRandomReward(getQuestValue(qgitem));
				if (r.itemName != qgitem.item.name) return new QuestGather(qgitem.amount,r,qgitem.item);
			}
		}
		return null;
	}
	private static int getBestAmount(int playerValue, Item item) {
		return (int)(10d*playerValue/item.value);
	}
	private static int getQuestValue(QuestGenItem qgitem) {
		return (int)(qgitem.item.value*qgitem.amount*(4d+Main.rand.NextDouble()*2d));
	}
	
	public readonly Item gather;
	
	public QuestGather(int toGather, Reward reward, Item gather) : base(toGather,reward) {
		this.gather = gather;
	}
	
	public override string questText() {
		return gather.name+": "+progress+"/"+progressTotal;
	}
	public override int questType() {
		return 1;
	}
	public override string questTypeName() {
		return "Gather";
	}
	public override void save(BinaryWriter bw) {
		base.save(bw);
		bw.Write(gather.name);
		bw.Write(progress);
	}
}