public class ItemDef {
	public readonly Item item;
	public Dictionary<string,bool> categories = new Dictionary<string,bool>();
	
	public ItemDef(Item item) {
		this.item = item;
		
		categories["Accessory"] = item.accessory;
		categories["Ammo"] = item.ammo > 0;
		categories["Consumable"] = item.potion || (item.consumable && item.buffType > 0) || item.healMana > 0 || item.healLife > 0;
		categories["Head"] = item.headSlot > 0;
		categories["Legs"] = item.legSlot > 0;
		categories["Magic"] = item.magic || item.mana > 0;
		categories["Material"] = item.material;
		categories["Melee"] = item.melee;
		categories["Ranged"] = item.ranged && item.ammo <= 0;
		categories["Tile"] = item.createTile >= 0;
		categories["Torso"] = item.bodySlot > 0;
		categories["Vanity"] = item.vanity;
		categories["Wall"] = item.createWall > 0;
		
		categories["Other"] = false;
		foreach (KeyValuePair<string,bool> pair in categories) if (pair.Key != "Material" && pair.Value) return;
		categories["Other"] = true;
	}
}