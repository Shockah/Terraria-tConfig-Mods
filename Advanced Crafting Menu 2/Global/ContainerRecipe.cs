public class ContainerRecipe : Container {
	public static bool IsRecipeTheSame(Recipe r1, Recipe r2) {
		if (r1 == null ^ r2 == null) return false;
		if (r1 == null && r2 == null) return true;
		
		if (r1.needWater != r2.needWater) return false;
		if (r1.needLava != r2.needLava) return false;
		if (!r1.createItem.IsTheSameAs(r2.createItem)) return false;
		
		for (int i = 0; i < r1.requiredTile.Length; i++) {
			if (r1.requiredTile[i] != r2.requiredTile[i]) return false;
		}
		for (int i = 0; i < r1.requiredItem.Length; i++) {
			if (!r1.requiredItem[i].IsTheSameAs(r2.requiredItem[i])) return false;
			if (r1.requiredItem[i].stack != r2.requiredItem[i].stack) return false;
		}
		
		return true;
	}
	
	public class SlotRecipe : Slot {
		public Recipe recipe;
		public readonly bool canPick;
		
		public SlotRecipe(Container container, Vector2 pos, bool canPick = true) : base(container,pos) {
			this.canPick = canPick;
		}
		public SlotRecipe(Container container, Vector2 pos, Vector2 size, bool canPick = true) : base(container,pos,size) {
			this.canPick = canPick;
		}
		
		public override void HandleHover(SpriteBatch sb, int slot) {
			base.HandleHover(sb,slot);
			if (IsBlankItem(item)) return;
			
			ContainerRecipe cr = (ContainerRecipe)container;
			List<Slot> slots = cr.GetMaterialSlots();
			for (int i = 0; i < recipe.requiredItem.Length; i++) {
				slots[i].item = recipe.requiredItem[i].CloneItem();
				slots[i].pos.X = pos.X+size.X+6;
			}
		}
		
		public override void HandleLeftMouse(SpriteBatch sb, int slot) {
			if (!canPick) return;
			if (recipe == null || IsBlankItem(recipe.createItem)) return;
			if (!IsBlankItem(Main.mouseItem) && !recipe.createItem.IsTheSameAs(Main.mouseItem)) return;
			if (!Main.mouseLeftRelease) return;
			
			if (IsBlankItem(Main.mouseItem)) {
				Main.mouseItem = (Item)recipe.createItem.Clone();
				Main.mouseItem.stack = 0;
			}
			if (Main.mouseItem.stack+recipe.createItem.stack > Main.mouseItem.maxStack) return;
			
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift)) {
				ContainerRecipe cr = (ContainerRecipe)container;
				while (true) {
					Main.mouseItem.stack += recipe.createItem.stack;
					if (Main.mouseItem.maxStack == 1) Main.mouseItem.Prefix(-1);
					recipe.Create(Main.mouseItem);
					Recipe.FindRecipes();
					
					if (Main.mouseItem.stack+recipe.createItem.stack > Main.mouseItem.maxStack) break;
					
					bool canContinue = false;
					for (int i = 0; i < cr.filtered.Count; i++) if (IsRecipeTheSame(recipe,cr.filtered[i].recipe)) {
						canContinue = true;
						break;
					}
					
					if (!canContinue) break;
				}
				
				Main.mouseItem = Main.player[Main.myPlayer].GetItem(Main.myPlayer,Main.mouseItem);
			} else {
				Main.mouseItem.stack += recipe.createItem.stack;
				if (Main.mouseItem.maxStack == 1) Main.mouseItem.Prefix(-1);
				recipe.Create(Main.mouseItem);
				Recipe.FindRecipes();
			}
		}
		public override void HandleRightMouse(SpriteBatch sb, int slot) {
			if (!canPick) return;
			if (recipe == null || IsBlankItem(recipe.createItem)) return;
			if (!IsBlankItem(Main.mouseItem) && !recipe.createItem.IsTheSameAs(Main.mouseItem)) return;
			
			if (Main.stackSplit <= 1 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0 || Main.mouseItem.stack < Main.mouseItem.maxStack)) {
				if (Main.mouseItem.type == 0) {
					Main.mouseItem = (Item)item.Clone();
					Main.mouseItem.stack = 0;
				}
				
				Main.mouseItem.stack += item.stack;
				if (Main.mouseItem.maxStack == 1) Main.mouseItem.Prefix(-1);
				recipe.Create(Main.mouseItem);
				Recipe.FindRecipes();
				
				if (Main.stackSplit == 0) Main.stackSplit = 15;
				else Main.stackSplit = Main.stackDelay;
			}
		}
	}
	
	public class SlotRecipeMaterial : Slot {
		public SlotRecipeMaterial(Container container, Vector2 pos) : this(container,pos,new Vector2(32,32)) {}
		public SlotRecipeMaterial(Container container, Vector2 pos, Vector2 size) : base(container,pos,size) {}
		
		public override void HandleLeftMouse(SpriteBatch sb, int slot) {}
		public override void HandleRightMouse(SpriteBatch sb, int slot) {}
		
		public override void HandleDraw(SpriteBatch sb, int slot) {
			if (!IsBlankItem(item)) base.HandleDraw(sb,slot);
		}
	}
	
	public class SlotInput : Slot {
		public SlotInput(Container container, Vector2 pos) : base(container,pos) {}
		public SlotInput(Container container, Vector2 pos, Vector2 size) : base(container,pos,size) {}
		
		public override void HandleLeftMouse(SpriteBatch sb, int slot) {
			base.HandleLeftMouse(sb,slot);
			Main.guideItem = item;
			Recipe.FindRecipes();
			Main.guideItem.type = 0;
		}
		public override void HandleRightMouse(SpriteBatch sb, int slot) {
			base.HandleLeftMouse(sb,slot);
			Main.guideItem = item;
			Recipe.FindRecipes();
			Main.guideItem.type = 0;
		}
	}
	
	public readonly List<RecipeDef> filtered;
	public readonly Slot slotInput;
	
	public ContainerRecipe(Vector2 pos, int rows, int cols, List<RecipeDef> filtered) {
		this.filtered = filtered;
		bool b = Main.craftGuide || Main.ForceGuideMenu;
		for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++) AddSlot(new SlotRecipe(this,new Vector2((int)(pos.X+(x*44*2)),(int)(pos.Y+(b ? 50 : 0)+(y*44)))));
		for (int i = 0; i < Recipe.maxRequirements; i++) AddSlot(new SlotRecipeMaterial(this,new Vector2((int)(pos.X+44+6),(int)(pos.Y+(b ? 50 : 0)+6+i*32))));
		slotInput = b ? AddSlot(new SlotInput(this,new Vector2((int)pos.X,(int)pos.Y))) : null;
	}
	
	public virtual List<Slot> GetMaterialSlots() {
		List<Slot> slots = new List<Slot>();
		foreach (Slot slot in this.slots) if (slot is SlotRecipeMaterial) slots.Add(slot);
		return slots;
	}
}