float boost = 0;

public void UseItem(Player P,int i)
{

    Vector2 Low = P.position - new Vector2(boost,boost)*16 - new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Vector2 High = P.position + new Vector2(P.width,P.height) + new Vector2(boost,boost)*16 + new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Low/= 16f;
    High/= 16f;
    High.Y-=2;

    if(Player.tileTargetX > High.X || Player.tileTargetX < Low.X) return;
    if(Player.tileTargetY > High.Y || Player.tileTargetY < Low.Y) return;
	
	Item item = P.inventory[P.selectedItem];
	float spread = 1f;
	if (item.type == Config.itemDefs.byName["Molten Shovel"].type) spread = 1.5f;
	else if (item.type == Config.itemDefs.byName["Hallowed Shovel"].type) spread = 3f;
	int maxSteps = (int)(spread*3f);

    if (Main.tile[Player.tileTargetX, Player.tileTargetY].active) 
        DigOutTile(P,Player.tileTargetX,Player.tileTargetY,spread,maxSteps);
    else DigOutWall(P,Player.tileTargetX,Player.tileTargetY,spread,maxSteps);
    P.itemTime = (int)((float)P.inventory[P.selectedItem].useTime * P.pickSpeed);

}

#region crappy visuals

public void HoldStyle(Player P)
{
    Vector2 Low = P.position - new Vector2(boost,boost)*16 - new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Vector2 High = P.position + new Vector2(P.width,P.height) + new Vector2(boost,boost)*16 + new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Low/= 16f;
    High/= 16f;
    High.Y-=2;

    if(Player.tileTargetX > High.X || Player.tileTargetX < Low.X) return;
    if(Player.tileTargetY > High.Y || Player.tileTargetY < Low.Y) return;

    P.showItemIcon = true;
}
public void UseStyle(Player P)
{
    Vector2 Low = P.position - new Vector2(boost,boost)*16 - new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Vector2 High = P.position + new Vector2(P.width,P.height) + new Vector2(boost,boost)*16 + new Vector2(Player.tileRangeX,Player.tileRangeY)*16;
    Low/= 16f;
    High/= 16f;
    High.Y-=2;

    if(Player.tileTargetX > High.X || Player.tileTargetX < Low.X) return;
    if(Player.tileTargetY > High.Y || Player.tileTargetY < Low.Y) return;

    P.showItemIcon = true;
}
#endregion

#region detectors
public bool TileValid(int t)
{
    int[] arr = new int[] {0,2,23,27,40,53,57,59,60,70,112,116,123,147};
    if(Array.IndexOf(arr,t) != -1) return true;
    return false;
}
public bool WallValid(int t)
{
    int[] arr = new int[] {2,15,16};
    if(Array.IndexOf(arr,t) != -1) return true;
    return false;
}
#endregion

#region diggers
public bool DigOutTile(Player P,int x,int y,float pow,int maxS,int ox =-1,int oy=-1)
{
    if(ox == -1) ox = x;
    if(oy == -1) oy = y;
    bool dug = false;
    if (Main.tile[x,y].active && TileValid(Main.tile[x, y].type) && (!Codable.RunTileMethod(false, new Vector2(x, y), Main.tile[x, y].type, "CanDestroyTile", x, y) || (bool)Codable.customMethodReturn==true))
    {
        WorldGen.KillTile(x, y, false, false, false, P);
        if (Main.netMode == 1)
        {
            NetMessage.SendData(17, -1, -1, "", 0, (float)x, (float)y, 0f, 0);
        }
        if(!Main.tile[x,y].active)dug = true;
    }
    if(maxS > 0 && dug)
    {
        if(Vector2.Distance(new Vector2(x-1,y),new Vector2(ox,oy))<=pow) DigOutTile(P,x-1,y,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x+1,y),new Vector2(ox,oy))<=pow) DigOutTile(P,x+1,y,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x,y-1),new Vector2(ox,oy))<=pow) DigOutTile(P,x,y-1,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x,y+1),new Vector2(ox,oy))<=pow) DigOutTile(P,x,y+1,pow,maxS-1,ox,oy);
    }
    return dug;
}


public bool DigOutWall(Player P,int x,int y,float pow,int maxS,int ox =-1,int oy=-1)
{
    if(ox == -1) ox = x;
    if(oy == -1) oy = y;
    bool dug = false;
    if (!Main.tile[x,y].active && WallValid(Main.tile[x, y].wall))
    {
        WorldGen.KillWall(x, y, false);
        if (Main.netMode == 1)
        {
            NetMessage.SendData(17, -1, -1, "", 2, (float)x, (float)y, 0f, 0);
        }
        if(Main.tile[x,y].wall == 0)dug = true;
    }
    if(maxS > 0 && dug)
    {
        if(Vector2.Distance(new Vector2(x-1,y),new Vector2(ox,oy))<=pow) DigOutWall(P,x-1,y,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x+1,y),new Vector2(ox,oy))<=pow) DigOutWall(P,x+1,y,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x,y-1),new Vector2(ox,oy))<=pow) DigOutWall(P,x,y-1,pow,maxS-1,ox,oy);
        if(Vector2.Distance(new Vector2(x,y+1),new Vector2(ox,oy))<=pow) DigOutWall(P,x,y+1,pow,maxS-1,ox,oy);
    }
    return dug;
}
#endregion

#region dusts


public void UseItemEffect(Player P, Rectangle R) 
{
	if (item.type == Config.itemDefs.byName["Molten Shovel"].type)
    {
        int D = Dust.NewDust(new Vector2(R.X,R.Y),R.Width,R.Height, 6, P.velocity.X * 0.2f + P.direction * 3f, P.velocity.Y * 0.2f, 100, default(Color), 1.9f);
	    Main.dust[D].noGravity = true;
    }
	else if (item.type == Config.itemDefs.byName["Hallowed Shovel"].type)
    {
	    int D = Dust.NewDust(new Vector2(R.X,R.Y),R.Width,R.Height, 57, P.velocity.X * 0.2f + P.direction * 3f, P.velocity.Y * 0.2f, 100, default(Color), 1.9f);
	    Main.dust[D].noGravity = true;
        Main.dust[D].velocity/=2f;
        Main.dust[D].velocity.X+=2f*P.direction;
    }
}

#endregion