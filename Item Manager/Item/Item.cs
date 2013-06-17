//Modified code, original code by Yoraiz0r
public void UpdateItem(int itemId, ref bool update, ref int movementType, ref int lavaImmunity) {
	Item item = Main.item[itemId];
	if (item.maxStack == 1) return;
	
	List<Vector2> ease = new List<Vector2>();
	for (int i = 0; i < Main.item.Length-1; i++) {
		Item item2 = Main.item[i];
		if (itemId != i && item2.active && item.type == item2.type && !item2.beingGrabbed) {
			if (item.stack+item2.stack > item.maxStack) continue;
			
			Vector2 dist = item2.Center-item.Center;
			if (dist.Length() <= 10f) {
				Vector2 moveBy = dist*(1f*item.stack/(item.stack+item2.stack));
				item.position += moveBy;
				
				item.stack += item2.stack;
				item2.active = false;
				
				if (Main.netMode == 2) {
					NetMessage.SendData(21,-1,-1,"",itemId,0f,0f,0f,0);
					NetMessage.SendData(21,-1,-1,"",i,0f,0f,0f,0);
				}
			} else if (dist.Length() <= 40f) ease.Add(dist);
		}
	}
	
	Vector2 over = new Vector2();
	foreach (Vector2 v in ease) over += v/(ease.Count*30f);
	if (over.Length() > 0) item.velocity = Vector2.SmoothStep(over,item.velocity,1f);
}