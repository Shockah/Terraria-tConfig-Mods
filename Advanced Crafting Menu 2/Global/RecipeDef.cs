public class RecipeDef {
	public Recipe recipe;
	public Dictionary<string,bool> categories = new Dictionary<string,bool>();
	
	public RecipeDef(Recipe recipe) {
		this.recipe = recipe;
		
		categories["Accessory"] = recipe.createItem.accessory;
		categories["Ammo"] = recipe.createItem.ammo > 0;
		categories["Consumable"] = recipe.createItem.potion || (recipe.createItem.consumable && recipe.createItem.buffType > 0) || recipe.createItem.healMana > 0 || recipe.createItem.healLife > 0;
		categories["Head"] = recipe.createItem.headSlot > 0;
		categories["Legs"] = recipe.createItem.legSlot > 0;
		categories["Magic"] = recipe.createItem.magic || recipe.createItem.mana > 0;
		categories["Material"] = recipe.createItem.material;
		categories["Melee"] = recipe.createItem.melee;
		categories["Ranged"] = recipe.createItem.ranged && recipe.createItem.ammo <= 0;
		categories["Tile"] = recipe.createItem.createTile >= 0;
		categories["Torso"] = recipe.createItem.bodySlot > 0;
		categories["Vanity"] = recipe.createItem.vanity;
		categories["Wall"] = recipe.createItem.createWall > 0;
		
		categories["Other"] = false;
		foreach (KeyValuePair<string,bool> pair in categories) if (pair.Key != "Material" && pair.Value) return;
		categories["Other"] = true;
	}
}