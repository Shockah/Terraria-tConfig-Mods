public class BuffDef {
	public readonly int buffID;
	public Dictionary<string,bool> categories = new Dictionary<string,bool>();
	
	public BuffDef(int buffID) {
		this.buffID = buffID;
		
		categories["Buff"] = !Main.debuff[buffID];
		categories["Debuff"] = Main.debuff[buffID];
		categories["No timer"] = Main.buffDontDisplayTime[buffID];
	}
}