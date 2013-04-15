public void AI()
{
    Projectile P = projectile;
    if(P.velocity.Y < 16f) P.velocity.Y+=0.2f;
    if(P.velocity.Y > 16f) P.velocity.Y = 16f;
}

public bool tileCollide(Vector2 col)
{
    Projectile P = projectile;
    if(col.X != P.velocity.X) 
    {
        P.velocity.X = 0;
        return false;
    }
    if(col.Y == P.velocity.Y || P.velocity.Y < 0) return false;
    int x = (int)(P.position.X+P.width/2f)/16;
    int y = (int)(P.position.Y+P.height/2f)/16;
    if (!Main.tile[x, y].active && (Main.tile[x-1,y].active || Main.tile[x+1,y].active || Main.tile[x,y-1].active || Main.tile[x,y+1].active))
    {
        WorldGen.PlaceTile(x, y, 147, false, true, -1, 0);
        if (Main.tile[x, y].active && (int)Main.tile[x, y].type == 147)
        {
            NetMessage.SendData(17, -1, -1, "", 1, (float)x, (float)y, (float)147, 0);
        }
    }
    P.Kill();
    return false;
}