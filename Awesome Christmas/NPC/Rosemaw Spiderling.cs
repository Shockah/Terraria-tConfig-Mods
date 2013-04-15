public bool FirstFrame = true; //change to true

#region Spawn NPC


public bool SpawnNPC(int x, int y, int playerID)
{
	return false;
}


#endregion


#region Initialize

public void Initialize()
{
    npc.netAlways = true;
    NPC N = npc;
}


#endregion


#region AI

public bool PreAI()
{
    NPC N = npc;
    return true;
}

public void PostAI()
{
    NPC N = npc;
}

/// ai explanation
/// 0 is for direction
/// 1 is for owner
/// 2 is for phase
/// 3 is for timer

public void AI()
{
    #region basic stuff
    
    NPC N = npc;
    Player PT = Main.player[N.target];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);
    Vector2 PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

    N.spriteDirection = 1;
    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 MC = M.position+new Vector2(M.width/2f,M.height/2f);

    #endregion

    #region Hover , shoot etc

    if (PT.dead || !PT.active)
    {
        N.TargetClosest(true);
        if(Main.player[N.target].dead) N.velocity.Y-=10f;
    }
    PT = Main.player[N.target];
    PTC = PT.position+new Vector2(PT.width/2,PT.height/2);
    
    #region appoint self to above target

    float FixerX = 0.03f,FixerY = 0.03f;
    Vector2 TarVec = PTC-NC+new Vector2(0,-250f);
    TarVec+=new Vector2((float)Math.Sin(N.ai[3]*Math.PI*2/300f +1.57f)*3f,(float)Math.Sin(N.ai[3]*2f*Math.PI*2/300f +1.57f)*-1f)*100f;
   

    if(TarVec.Length() > 4f) TarVec*=(float)(4f/TarVec.Length());
    if(N.velocity.Y < TarVec.Y) 
        N.velocity.Y += FixerY; 
    else N.velocity.Y -= FixerY;
    if(N.velocity.Y < 0 && TarVec.Y > 0) N.velocity.Y+= FixerY;
    if(N.velocity.Y > 0 && TarVec.Y < 0) N.velocity.Y-= FixerY;     

    float vXm = 4f,vXc = 0.1f;
    if(TarVec.X < 0 && N.velocity.X > -vXm)
    {
        N.velocity.X-=vXc;
        if(N.velocity.X > vXm) N.velocity.X-=vXc;
        else if (N.velocity.X > 0f) N.velocity.X+=vXc/2f;
        if(N.velocity.X < -vXm) N.velocity.X = -vXm;
    }
    else if(TarVec.X > 0 && N.velocity.X < vXm)
    {
        N.velocity.X+=vXc;
        if(N.velocity.X < -vXm) N.velocity.X+=vXc;
        else if (N.velocity.X < 0f) N.velocity.X-=vXc/2f;
        if(N.velocity.X > vXm) N.velocity.X = vXm;
    }

    #endregion
    
    #region taking care of other phases
        
    N.ai[3]++;
    N.ai[0]++;
    int wait = 300;
    if(N.ai[3] > wait) N.ai[3] = 0;
    if(N.ai[0] > wait)
    {
        N.ai[0] = 0;
        if (Main.netMode != 1) NPC.NewNPC((int)NC.X,(int)NC.Y,"Frostmaw Web");
    }

    #endregion

    #endregion
    
}


#endregion


#region Dealt NPC


public void DealtNPC(double DMG, Player P)
{

}


#endregion


#region NPC Loot


public void NPCLoot()
{
	Color c = new Color(.75f,0f,0f);
	for (int i = 0; i < 10; i++) Dust.NewDust(npc.Center,4,4,45,(float)(Main.rand.NextDouble()*2f),(float)(Main.rand.NextDouble()*2f),0,c,(float)(1.5f+Main.rand.NextDouble()*.5f));
}


#endregion

#region DamagePlayer

public void DamagePlayer(Player P,ref int DMG)
{
    P.AddBuff("Frostbite",900);
}

#endregion