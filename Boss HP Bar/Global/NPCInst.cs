public class NPCInst {
	public List<NPC> parts = new List<NPC>();
	public bool separateLife = false;
	private Vector2 centerPos;
	private int life, lifeMax;
	private string name;
	
	private bool recalc = false;
	
	public void AddPart(NPC npc) {
		parts.Add(npc);
		recalc = true;
	}
	
	public Vector2 GetCenterPos() {
		Recalculate();
		return new Vector2(centerPos.X,centerPos.Y);
	}
	public int GetLife() {
		Recalculate();
		return life;
	}
	public int GetLifeMax() {
		Recalculate();
		return lifeMax;
	}
	public string GetName() {
		Recalculate();
		return name;
	}
	
	private void Recalculate() {
		if (!recalc) return;
		
		int partN = 0;
		double xx = 0, yy = 0;
		
		life = separateLife ? 0 : parts[0].life;
		lifeMax = separateLife ? 0 : parts[0].lifeMax;
		
		NPC shortest = null;
		int shortestL = -1;
		bool duplicate = false;
		foreach (NPC npc in parts) {
			partN++;
			xx += npc.position.X+npc.width/2f;
			yy += npc.position.Y+npc.height/2f;
			if (separateLife) {
				life += npc.life;
				lifeMax += npc.lifeMax;
			}
			
			string useName = npc.displayName == null || npc.displayName == "" ? npc.name : npc.displayName;
			int l = useName.Split(' ').Length-1;
			if (shortest != null && l == shortestL) duplicate = true;
			if (shortest == null || l < shortestL) {
				shortest = npc;
				shortestL = l;
				duplicate = false;
			}
		}
		
		if (duplicate) {
			name = "";
			
			bool nope = false;
			string useName1 = parts[0].displayName == null || parts[0].displayName == "" ? parts[0].name : parts[0].displayName, useName2;
			for (int i = 1; i < parts.Count; i++) {
				useName2 = parts[i].displayName == null || parts[i].displayName == "" ? parts[i].name : parts[i].displayName;
				if (useName1 != useName2) {
					nope = true;
					break;
				}
			}
			
			if (nope) {
				List<NPC> npcShort = new List<NPC>();
				foreach (NPC npc in parts) if ((npc.displayName == null || npc.displayName == "" ? npc.name : npc.displayName).Split(' ').Length-1 == shortestL) npcShort.Add(npc);
				
				string[] spl = (npcShort[0].displayName == null || npcShort[0].displayName == "" ? npcShort[0].name : npcShort[0].displayName).Split(' ');
				for (int i = 0; i < shortestL; i++) {
					useName1 = spl[i];
					for (int j = 1; j < npcShort.Count; j++) {
						useName2 = (npcShort[j].displayName == null || npcShort[j].displayName == "" ? npcShort[j].name : npcShort[j].displayName).Split(' ')[i];
						if (useName1 != useName2) goto L;
					}
					
					if (name.Length > 0) name += " ";
					name += spl[i];
				}
				L: {}
			} else name = useName1;
		} else name = shortest.displayName == null || shortest.displayName == "" ? shortest.name : shortest.displayName;
		
		centerPos = new Vector2((float)(xx/partN),(float)(yy/partN));
		recalc = false;
	}
}