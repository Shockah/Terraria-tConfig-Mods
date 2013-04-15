
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

/// ai explain
/// 0 is stage
/// 1 is owner
/// 2 is
/// 3 is
/// 

public void AI()
{
    NPC N = npc;
    Player PT = Main.player[N.target];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);
    Vector2 PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 MC = M.position+new Vector2(M.width/2f,M.height/2f);
    N.spriteDirection = -1;
    N.displayName = M.displayName;
    if(!M.active || M.type != Config.npcDefs.byName["Frostmaw"].type)
    {
		N.life = -1;
		N.HitEffect(0, 10.0);
		N.active = false;
    }
    N.realLife = (int)N.ai[1];
    N.position = MC-(NC-N.position)-new Vector2(0,M.height);
    
    if(N.ai[0] < 1)
    {
        if(M.ai[0] >= 2)
        {
            N.ai[0] = 1;
        }
    }
    if(N.ai[0] < 2)
    {
        if(M.ai[0] >= 3)
        {
            N.ai[0] = 2;
            
        }
    }
    if(N.ai[0] < 3 && M.ai[0] >= 3)
    {
        N.ai[0] = 3;
        if(Main.netMode == 1) return;
        int babies = 15; //a number dividable by 300 is preferable
        int rand = 0;
        rand = Config.syncedRand.Next(1,4);
        for(int i = 0; i < rand;i++) Gore.NewGore(N.position, new Vector2((float)Config.syncedRand.Next(-30, 31) * 0.2f, (float)Config.syncedRand.Next(-50, 31) * 0.2f), "Frostmaw Shard 1", 0.8f);
        rand = Config.syncedRand.Next(1,4);
        for(int i = 0; i < rand;i++) Gore.NewGore(N.position, new Vector2((float)Config.syncedRand.Next(-30, 31) * 0.2f, (float)Config.syncedRand.Next(-50, 31) * 0.2f), "Frostmaw Shard 2", 0.7f);
        rand = Config.syncedRand.Next(1,4);
        for(int i = 0; i < rand;i++) Gore.NewGore(N.position, new Vector2((float)Config.syncedRand.Next(-30, 31) * 0.2f, (float)Config.syncedRand.Next(-50, 31) * 0.2f), "Frostmaw Shard 3", 0.8f);
        rand = Config.syncedRand.Next(1,4);
        for(int i = 0; i < rand;i++) Gore.NewGore(N.position, new Vector2((float)Config.syncedRand.Next(-30, 31) * 0.2f, (float)Config.syncedRand.Next(-50, 31) * 0.2f), "Frostmaw Shard 4", 0.8f);
        
        for(int i = 0; i < babies; i++) 
        {
            int poor = NPC.NewNPC((int)NC.X,(int)NC.Y,"Frostmaw Spiderling");
            NPC Z = Main.npc[poor];
            Z.velocity = new Vector2((float)Math.Cos(i),(float)Math.Sin(i))*5f;
            Z.ai[0] = 300f*(float)Config.syncedRand.NextDouble();
            Z.ai[3] = 300f*(float)Config.syncedRand.NextDouble();
        }
    }
    if(N.ai[0] > 0) N.ai[2]++;
    if(N.ai[0] > 0 && N.ai[2] == 480)
    {
        N.ai[2] = 0;
        if(Main.netMode != 1)
        {
            for(int i = 0; i < Main.player.Length; i++)
            {
                Player Z = Main.player[i];
                if(!Z.active) continue;
                if(Z.dead) continue;
                Vector2 ZC = new Vector2(Z.width,Z.height)/2f +Z.position;
                if(Vector2.Distance(ZC,NC) < 400f && i != N.target) continue;
                
                Vector2 SP = ZC + new Vector2((200f+300f*(float)Config.syncedRand.NextDouble())*(((float)Config.syncedRand.Next(2) - 0.5f)*2f),-600f);
                Vector2 Velo = (ZC-SP);
                Velo.Normalize();
                Velo*=7f;
                Projectile.NewProjectile(SP.X,SP.Y,Velo.X,Velo.Y,"Big Icicle",30,2f,Main.myPlayer);
                for(int d = 0; d < 3; d++)
                {
                    SP = ZC + new Vector2((200f+300f*(float)Config.syncedRand.NextDouble())*(((float)Config.syncedRand.Next(2) - 0.5f)*2f),-600f);
                    Velo = (ZC-SP);
                    Velo.Normalize();
                    Velo*=7f;
                    Velo+=new Vector2(Config.syncedRand.Next(-40,41)*0.1f,Config.syncedRand.Next(-40,41)*0.1f);
                    Projectile.NewProjectile(SP.X,SP.Y,Velo.X,Velo.Y,"Small Icicle",30,2f,Main.myPlayer);
                
                }
            }
        }
    }
}


#endregion

#region Find Frame


public void FindFrame(int currentFrame)
{
    NPC N = npc;
    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);

    if (N.velocity.X < 0)
    {
    N.direction = -1;
    N.spriteDirection = -1;
    }
    else
    {
    N.direction = 1;
    N.spriteDirection = 1;
    }

    int num = Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type];
    int num2 = (int)N.ai[0];
    if(num2 > 2) num2 = 2; if(num2 < 0) num2 = 0;
    N.frame.Y =  num * num2;
    #region frame switching
    //N.frameCounter += 1.0;
    //if (N.frameCounter >= 4.0)
    //{
    //    N.frame.Y = N.frame.Y + num;
    //    N.frameCounter = 0.0;
    //}
    //if (N.frame.Y >= num * Main.npcFrameCount[N.type])
    //{
    //    N.frame.Y = 0;
    //}
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
}


#endregion


