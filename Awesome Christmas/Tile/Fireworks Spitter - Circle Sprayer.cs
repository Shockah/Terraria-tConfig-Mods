int ignite;
int sx,sy;
public void Initialize(int x,int y)
{
    sx = x;
    sy = y;
}
public void Update()
{
    if(Main.tile[sx,sy].frameX > 0 && ignite == 0)
    {
        ignite = 60;
    }
    if(ignite > 0) 
    {
        ignite --;

        if(ignite == 0)
        {
            bool b = LaunchFirework(GetData());
            //Main.NewText("taunt "+Main.time);
            Main.tile[sx,sy].frameX = 0;
			Main.tile[sx,sy].frameNumber = 0;
			
			if (b) {
				Main.tile[sx,sy+1].frameX = 0;
				Main.tile[sx,sy+1].frameNumber = 0;

                NetMessage.SendTileSquare(-1, sx, sy, 1);
                NetMessage.SendTileSquare(-1, sx, sy+1, 1);
			}
        }
    }
}
public int GetData() {
	List<Item> items = ModWorld.GetCustomItems(new Vector2(sx,sy+1));
	while (items.Count < 5) items.Add(new Item());
	byte[] result = new byte[4];
	
	Color? c1;
	Random rnd = new Random();
	c1 = ModWorld.Firework.GetColor(items[0],items[1],items[2],items[3],items[4]);
	
	if (!c1.HasValue) {
		int r,g,b;
		ModWorld.HsvToRgb((float)(rnd.NextDouble()*360d),(float)(.5f+rnd.NextDouble()*.5f),1f,out r,out g,out b);
		c1 = new Color?(new Color(r,g,b));
	}
	
	float h,s,v;
	
	ModWorld.RgbToHsv(c1.Value.R,c1.Value.G,c1.Value.B,out h,out s,out v);
	result[0] = (byte)(h/360f*255f);
	result[1] = (byte)(s*255f);
	
	result[2] = (byte)rnd.Next(256);
	result[3] = (byte)rnd.Next(256);
	
	return BitConverter.ToInt32(result,0);
}
public bool LaunchFirework(int t)
{
    if (sy > 0 && Main.tile[sx,sy-1].active && Main.tile[sx,sy-1].type == Main.tile[sx,sy].type) return false;
	Vector2 Pos = new Vector2(sx*16 +8, sy*16);
    Vector2 Velo = new Vector2(BitConverter.ToSingle(BitConverter.GetBytes(t),0),-8f);
    Projectile.NewProjectile(Pos.X,Pos.Y,Velo.X,Velo.Y,"Holiday Firework - Circle Sprayer",30,2,Main.myPlayer);
	return true;
}
public void UseTile(Player p,int x,int y)
{
	int mx = (int)((Main.screenPosition.X+Main.mouseX)/16f);
	int my = (int)((Main.screenPosition.Y+Main.mouseY)/16f);
	
	if (my > 0 && Main.tile[mx,my-1].active && Main.tile[mx,my-1].type == Main.tile[mx,my].type) {
		Vector2 v = new Vector2(mx,my);
		if (Main.netMode == 0) {
			ModPlayer.GuiSpitter.Create(v,ModWorld.GetCustomItems(v),5);
		} else if (Main.netMode == 1) {
			NetMessage.SendModData(ModWorld.modId,ModWorld.MSG_GUI_REQUEST,-1,-1,(byte)p.whoAmi,mx,my);
		}
	} else {
		if(Main.tile[x,y].frameY > 0) y--;
		bool Out = Main.tile[x,y].frameNumber==1;
		if(!Out) 
		{
			Main.tile[x,y].frameX+=18;
			Main.tile[x,y+1].frameX+=18;
		}
		else 
		{
            return;
		} 
		Main.tile[x,y].frameNumber = (byte)(Out?0:1);
		Main.tile[x,y+1].frameNumber = (byte)(Out?0:1);
		if (Main.netMode == 1)
		{
			NetMessage.SendTileSquare(-1, x, y, 1);
			NetMessage.SendTileSquare(-1, x, y+1, 1);
		}
	}
}
public void hitWire(int x,int y)
{
    if(Main.tile[x,y].frameY > 0) y--;
    bool Out = Main.tile[x,y].frameNumber==1;
    if(!Out) 
    {
        Main.tile[x,y].frameX+=18;
        Main.tile[x,y+1].frameX+=18;
    }
    else 
    {
            return;
    } 
    Main.tile[x,y].frameNumber = (byte)(Out?0:1);
    Main.tile[x,y+1].frameNumber = (byte)(Out?0:1);
    NetMessage.SendTileSquare(-1, x, y, 1);
    NetMessage.SendTileSquare(-1, x, y+1, 1);
    WorldGen.noWire(x,y);
    WorldGen.noWire(x,y+1);
}
public void AddLight(int x,int y,ref float r,ref float g,ref float b)
{
    bool Light = Main.tile[x,y].frameNumber==1;
    if(Light)
    {
        if(Main.rand.Next(2) == 0) Dust.NewDust(new Vector2(sx*16,sy*16),16,2,6);
    }
}