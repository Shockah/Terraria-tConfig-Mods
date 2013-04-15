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

public void AI()
{
    #region basic stuff
    
    NPC N = npc;
    Player PT = Main.player[N.target];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);
    Vector2 PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

    N.spriteDirection = (int)N.ai[0];
    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 MC = M.position+new Vector2(M.width/2f,M.height/2f);

    #endregion


    N.velocity.Y = 5f;
    if (N.collideY)
    {
        int tx = (int)(NC.X/16f),ty = (int)(NC.Y/16f);  
        tx-=1;ty-=1;
        int tym = ty;
        for(int i = -1; i < 2; i++,tx++)
        {
            ty = tym;
            for(int j = -1; j < 2; j++,ty++)
            {
                Tile T = Main.tile[tx,ty];
                if (!T.active)
                {
                    WorldGen.PlaceTile(tx,ty, 51, false, true, -1, 0);
                    T = Main.tile[tx,ty];
                    if (T.active && (int)Main.tile[tx,ty].type == 51)
                    {
                        NetMessage.SendData(17, -1, -1, "", 1, (float)tx, (float)ty, (float)51, 0);
                    }
                }
                N.life = -1;
                N.HitEffect(0, 10.0);
                N.active = false;
            }
        }
    }
}


#endregion


#region NPC Loot


public void NPCLoot()
{
}


#endregion

#region DamagePlayer

public void DamagePlayer(Player P,ref int DMG)
{
    P.AddBuff(32,600);
}

#endregion
