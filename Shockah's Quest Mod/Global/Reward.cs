public class Reward {
	public static Reward getRandomReward(int value) {
		List<string> list = new List<string>();
		string[] arr;
		
		arr = new string[]{"Copper","Iron","Silver","Gold","Demonite","Meteorite","Hellstone"};
		foreach (string s in arr) list.Add(s+" Bar");
		arr = new string[]{"Amethyst","Topaz","Sapphire","Emerald","Ruby","Diamond"};
		foreach (string s in arr) list.Add(s);
		if (Main.hardMode) {
			arr = new string[]{"Cobalt","Mythril","Adamantite"};
			foreach (string s in arr) list.Add(s+" Bar");
		}
		
		while (true) {
			if (Main.rand.Next(3) >= 1) {
				string name = list[Main.rand.Next(list.Count)];
				Item item = Config.itemDefs.byName[name];
				if (item.value > value) continue;
				Reward r = new Reward(name,(int)(value/item.value));
				if (r.amount <= Config.itemDefs.byName[r.itemName].maxStack) return r;
			} else {
				string type = "Copper Coin";
				if (value >= 100) {value /= 100; type = "Silver Coin";}
				if (value >= 100) {value /= 100; type = "Gold Coin";}
				if (value >= 100) {value /= 100; type = "Platinum Coin";}
				return new Reward(type,value);
			}
		}
	}
	
	public readonly string itemName;
	public readonly int amount;
	
	public Reward(string itemName, int amount) {
		this.itemName = itemName;
		this.amount = amount;
	}
}