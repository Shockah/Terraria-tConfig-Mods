public abstract class Quest {
	public static Quest getRandomQuest(Player player) {
		Quest q = null;
		while (q == null) {
			switch (Main.rand.Next(5)) {
				case 0: case 1: q = QuestMonster.getRandomQuest(player,false); break;
				case 2: q = QuestMonster.getRandomQuest(player,true); break;
				case 3: case 4: q = QuestGather.getRandomQuest(player); break;
			}
		}
		return q;
	}
	
	public readonly Reward reward;
	public readonly int progressTotal;
	public int progress = 0;
	
	protected Quest(int progressTotal, Reward reward) {
		this.progressTotal = progressTotal;
		this.reward = reward;
	}
	
	public abstract string questText();
	public virtual void save(BinaryWriter bw) {
		bw.Write((byte)questType());
		bw.Write(progressTotal);
		bw.Write(reward.itemName);
		bw.Write(reward.amount);
	}
	
	public abstract int questType();
	public abstract string questTypeName();
	public void endQuest() {
		Player player = Main.player[Main.myPlayer];
		int id = Item.NewItem(Convert.ToInt32(player.position.X),Convert.ToInt32(player.position.Y),0,0,reward.itemName,reward.amount,false,0);
		Main.item[id].noGrabDelay = 1;
		if (Main.netMode == 1) NetMessage.SendData(21,-1,-1,"",id,0f,0f,0f,0);
		ModPlayer.questsFinished++;
		
		if (ModPlayer.AcAchieve != null) {
			if (ModPlayer.questsFinished >= 1) ModPlayer.AcAchieve("SHK_QUEST_1",null);
			ModPlayer.AcSetProgress("SHK_QUEST_50",ModPlayer.questsFinished,null);
		}
	}
}