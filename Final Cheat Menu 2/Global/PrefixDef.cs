public class PrefixDef {
	public static Item GetShowcaseItem(Prefix prefix) {
		foreach (KeyValuePair<string,Item> pair in Config.itemDefs.byName) {
			if (prefix.Check(pair.Value)) return pair.Value;
		}
		return null;
	}
	
	public readonly int prefixID;
	public readonly Prefix prefix;
	public readonly Item showcaseItem;
	public Dictionary<string,bool> categories = new Dictionary<string,bool>();
	
	public PrefixDef(int prefixID, Prefix prefix) {
		this.prefixID = prefixID;
		this.prefix = prefix;
		
		Item item = new Item();
		item.SetDefaults("Unloaded Item");
		item.value = 100000;
		prefix.Apply(item);
		
		categories["Positive"] = item.value >= 100000;
		categories["Negative"] = item.value < 100000;
		
		showcaseItem = GetShowcaseItem(prefix);
	}
}