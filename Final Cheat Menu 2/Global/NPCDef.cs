public class NPCDef {
	public readonly NPC npc;
	public Dictionary<string,bool> categories = new Dictionary<string,bool>();
	
	public NPCDef(NPC npc) {
		this.npc = npc;
		
		categories["Town"] = npc.townNPC;
		categories["Friendly"] = npc.friendly || (npc.damage <= 0 && npc.defDamage <= 0);
		categories["Hostile"] = !categories["Friendly"];
		categories["Boss"] = npc.boss;
	}
}