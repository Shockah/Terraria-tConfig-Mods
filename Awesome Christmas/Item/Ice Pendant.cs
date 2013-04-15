public const float spread = 2.5f;

public int fuel = 0;

public void Save(BinaryWriter bw) {
	bw.Write((short)fuel);
}
public void Load(BinaryReader br, int version) {
	fuel = br.ReadInt16();
}

public bool CanRefuel(Player player) {
	Item item = new Item();
	item.SetDefaults("Snow Block");
	return ModPlayer.HasItem(player,item);
}
public void Refuel(Player player) {
	Item item = new Item();
	item.SetDefaults("Snow Block");
	if (ModPlayer.EatItem(player,item)) fuel += 3;
}

public void Effects(Player player) {
	if (Main.netMode == 2) return;
	
	int y = (int)Math.Round((player.position.Y+player.velocity.Y+player.height)/16f);
	float xCenter = (player.position.X+player.velocity.X+player.width/2f)/16f;
	float xStart = xCenter-spread, xEnd = xCenter+spread;
	
	if (fuel > 0 || CanRefuel(player)) {
		for (int x = (int)Math.Floor(xStart); x <= (int)Math.Floor(xEnd); x++) {
			if (Main.tile[x,y] == null) Main.tile[x,y] = new Tile();
			if (!Main.tile[x,y].active && Main.tile[x,y].liquid >= 16) {
				Projectile proj = Main.projectile[Projectile.NewProjectile(x*16f+8f,y*16f+8f,0f,0f,"Ice Block",0,0,player.whoAmi)];
				proj.ai[0] = (float)x;
				proj.ai[1] = (float)y;
				proj.timeLeft = 60*6;
				fuel--;
				if (Main.netMode == 1) NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_ICEACC,-1,-1,(byte)player.whoAmi,x,y);
			}
		}
		while (fuel < 0) {
			int oldFuel = fuel;
			Refuel(player);
			if (oldFuel == fuel) break;
		}
	}
}