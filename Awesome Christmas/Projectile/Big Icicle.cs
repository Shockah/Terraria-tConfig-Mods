public void PostAI()
{
    Projectile P = projectile;
    projectile.rotation-=1.57f/2f;
    if(Main.rand.Next(2)==0)
    {
        int z = Dust.NewDust(P.position,P.width,P.height,68,0,0,0,default(Color),1f);
        Main.dust[z].noGravity = true;
        Main.dust[z].velocity *= 1.5f;
        Main.dust[z].scale *= 0.9f;
    }
}

public void PostKill()
{
    Projectile P = projectile;
    Main.PlaySound(0, (int)P.position.X, (int)P.position.Y, 1);
    for (int a = 0; a < 10; a++)
    {
        int z = Dust.NewDust(P.position,P.width,P.height,68,0,0,0,default(Color),1f);
        Main.dust[z].noGravity = true;
        Main.dust[z].velocity *= 1.5f;
        Main.dust[z].scale *= 0.9f;
    }
	
	if (Main.rand.Next(20) == 0) Item.NewItem((int)P.position.X,(int)P.position.Y,P.width,P.height,"Needlessly Large Icicle",1,false,-1);
}
public void DealtPlayer(double dmg,Player P)
{
    P.AddBuff("Frostbite",900);
    P.AddBuff("Frostbite",900);
    P.AddBuff("Frostbite",900);
    P.AddBuff(30,900);
}