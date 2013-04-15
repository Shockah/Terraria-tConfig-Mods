public void AI()
{
    Projectile P = projectile;
    Player Pr = Main.player[projectile.owner];
	Vector2 PrC = P.position+new Vector2(P.width,P.height)/2f;
    P.ai[0]++;
    if (Main.myPlayer == P.owner)
    {
	    if (Pr.channel)
	    {
		    float spd = Pr.inventory[Pr.selectedItem].shootSpeed * P.scale;
            Vector2 MO = Main.screenPosition+new Vector2(Main.mouseX,Main.mouseY);
            Vector2 Tar = MO-PrC;
            Tar*=spd/Tar.Length();
		    if (Tar.X != P.velocity.X || Tar.Y != P.velocity.Y)
		    {
			    P.netUpdate = true;
		    }
		    P.velocity.X = Tar.X;
		    P.velocity.Y = Tar.Y;
	    }
	    else
	    {
		    P.Kill();
	    }
    }
    if (P.velocity.X > 0f)
    {
	    P.direction = 1;
    }
    else
    {
	    if (P.velocity.X < 0f)
	    {
		    P.direction = -1;
	    }
    }
    P.spriteDirection = P.direction;
    Pr.direction = P.direction;
    Pr.heldProj = P.whoAmI;
    //P.scale = 0;
    Pr.itemTime = 2;
    Pr.itemAnimation = 2;
    P.position.X = Pr.position.X + (float)(Pr.width / 2) - (float)(P.width / 2);
    P.position.Y = Pr.position.Y + (float)(Pr.height / 2) - (float)(P.height / 2);
    Pr.itemRotation = (float)Math.Atan2((double)(P.velocity.Y * (float)P.direction), (double)(P.velocity.X * (float)P.direction));
}

public void PostKill()
{
    if(Main.myPlayer != projectile.owner) return;
    Projectile P = projectile;
    Player Pr = Main.player[projectile.owner];
    Pr.itemTime = 20;
    Pr.itemAnimation = 20;
    float rate = P.ai[0]/180f; //charges to max over 3 seconds i guess
    //if(rate < 0.5f) rate = 0.5f;
    rate = 0.5f+rate*1.5f;
    P.velocity*=rate;
    int dmg = (int)(projectile.damage*rate);
    float kb = (int)(projectile.knockBack*rate);
    Projectile.NewProjectile(P.position.X,P.position.Y,P.velocity.X,P.velocity.Y,"Snowball",dmg,kb,Main.myPlayer);
}