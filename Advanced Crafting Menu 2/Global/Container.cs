public class Container {
	public static bool IsBlankItem(Item item) {
		return item == null || item.type == 0 || item.name == null || item.name == "" || item.name == "Unloaded Item" || item.stack <= 0;
	}
	
	public class Slot {
		public readonly Container container;
		public Vector2 pos, size;
		public Item item = new Item();
		
		public Slot(Container container, Vector2 pos) : this(container,pos,new Vector2(44,44)) {}
		public Slot(Container container, Vector2 pos, Vector2 size) {
			this.container = container;
			this.pos = pos;
			this.size = size;
		}
		
		public virtual void HandleHover(SpriteBatch sb, int slot) {
			Main.player[Main.myPlayer].mouseInterface = true;
			
			if (item == null || item.type == 0) return;
			string tip = item.name;
			Main.toolTip = (Item)item.ShallowClone();
			Main.buffString = "";
			
			if (item.stack > 1) tip += " ("+item.stack+")";
			Config.mainInstance.MouseText(tip,item.rare,0);
		}
		
		public virtual void HandleLeftMouse(SpriteBatch sb, int slot) {
			if (!Main.mouseLeftRelease) return;
			
			bool blankMouse = IsBlankItem(Main.mouseItem);
			bool blankSlot = IsBlankItem(item);
			
			if (blankMouse && blankSlot) return;
			else if (blankMouse && !blankSlot) {
				Main.mouseItem = item.CloneItem();
				item = new Item();
			} else if (!blankMouse && blankSlot) {
				item = Main.mouseItem.CloneItem();
				Main.mouseItem = new Item();
			} else {
				if (item.IsTheSameAs(Main.mouseItem) && item.maxStack > 1) {
					int diff = Math.Min(item.maxStack-item.stack,Main.mouseItem.stack);
					item.stack += diff;
					Main.mouseItem.stack -= diff;
					if (Main.mouseItem.stack == 0) Main.mouseItem = new Item();
				} else {
					Item tmp = Main.mouseItem;
					Main.mouseItem = item.CloneItem();
					item = tmp.CloneItem();
				}
			}
		}
		public virtual void HandleRightMouse(SpriteBatch sb, int slot) {
			if (Main.mouseRightRelease && item.maxStack == 1 && (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1 || item.accessory)) {
				item = Main.armorSwap(item);
			} else {
				if (Main.stackSplit <= 1 && item.maxStack > 1 && (Main.mouseItem.IsTheSameAs(item) || Main.mouseItem.type == 0 || Main.mouseItem.stack < Main.mouseItem.maxStack)) {
					if (Main.mouseItem.type == 0) {
						Main.mouseItem = item.CloneItem();
						Main.mouseItem.stack = 0;
					}
					
					Main.mouseItem.stack++;
					item.stack--;
					if (item.stack <= 0) item = new Item();
					Recipe.FindRecipes();
					
					if (Main.stackSplit == 0) Main.stackSplit = 15;
					else Main.stackSplit = Main.stackDelay;
				}
			}
		}
		
		public virtual void HandleDraw(SpriteBatch sb, int slot) {
			HandleDrawBack(sb,slot);
			HandleDrawContents(sb,slot);
		}
		public virtual void HandleDrawBack(SpriteBatch sb, int slot) {
			sb.Draw(Main.inventoryBack5Texture,new Rectangle((int)pos.X,(int)pos.Y,(int)size.X,(int)size.Y),new Rectangle?(new Rectangle(0,0,Main.inventoryBackTexture.Width,Main.inventoryBackTexture.Height)),Color.White);
		}
		public virtual void HandleDrawContents(SpriteBatch sb, int slot) {
			ItemSlotRender.DrawItemInSlot(sb,item,pos,size.X < size.Y ? size.X/Main.inventoryBack5Texture.Width : size.Y/Main.inventoryBack5Texture.Height);
		}
	}
	
	public List<Slot> slots = new List<Slot>();
	
	public Slot AddSlot(Vector2 pos, Vector2 size) {
		return AddSlot(new Slot(this,pos,size));
	}
	public Slot AddSlot(Slot slot) {
		slots.Add(slot);
		return slot;
	}
	
	public virtual void HandlePreDrawInterface(SpriteBatch sb) {
		HandleDraw(sb);
		if (Main.mouseLeft) HandleLeftMouse(sb);
		if (Main.mouseRight) HandleRightMouse(sb);
	}
	public virtual void HandlePostDraw(SpriteBatch sb) {
		HandleHover(sb);
	}
	
	public virtual void HandleHover(SpriteBatch sb) {
		for (int i = 0; i < slots.Count; i++) {
			Slot slot = slots[i];
			if (Main.mouseX >= slot.pos.X && Main.mouseY >= slot.pos.Y && Main.mouseX < slot.pos.X+slot.size.X && Main.mouseY < slot.pos.Y+slot.size.Y) slot.HandleHover(sb,i);
		}
	}
	
	public virtual void HandleLeftMouse(SpriteBatch sb) {
		for (int i = 0; i < slots.Count; i++) {
			Slot slot = slots[i];
			if (Main.mouseX >= slot.pos.X && Main.mouseY >= slot.pos.Y && Main.mouseX < slot.pos.X+slot.size.X && Main.mouseY < slot.pos.Y+slot.size.Y) slot.HandleLeftMouse(sb,i);
		}
	}
	public virtual void HandleRightMouse(SpriteBatch sb) {
		for (int i = 0; i < slots.Count; i++) {
			Slot slot = slots[i];
			if (Main.mouseX >= slot.pos.X && Main.mouseY >= slot.pos.Y && Main.mouseX < slot.pos.X+slot.size.X && Main.mouseY < slot.pos.Y+slot.size.Y) slot.HandleRightMouse(sb,i);
		}
	}
	
	public virtual void HandleDraw(SpriteBatch sb) {
		for (int i = 0; i < slots.Count; i++) slots[i].HandleDraw(sb,i);
	}
}