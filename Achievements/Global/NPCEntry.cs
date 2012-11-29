public class NPCEntry {
	public readonly NPC npc;
	public List<int> hitBy = new List<int>();
	
	public NPCEntry(NPC npc) {
		this.npc = npc;
	}
}