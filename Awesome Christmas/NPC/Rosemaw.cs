
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

/// I will be using the ai[] to do the following
/// ai[0] will be used for NPC Level (raged etc)
/// ai[1] will be the NPC's phase (attacking , idling etc)
/// ai[2] will be the NPC's timer 
/// ai[3] will be.....

public void AI()
{
    NPC N = npc;
    Player PT = Main.player[N.target];
    Vector2 NC = N.position+new Vector2(N.width/2,N.height/2);
    Vector2 PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

    bool locked = false;
    
    #region First Frame


    if(FirstFrame)
    {
        N.TargetClosest(true);
        N.ai[0] = 1f; // level 1
        N.ai[1] = 1f; // phase 1
        if(Main.netMode != 1)
        {
            #region claws
            for(int i = 0; i < 2; i++)
            {
                int a = NPC.NewNPC((int)NC.X, (int)NC.Y, "Rosemaw Claw", N.whoAmI);
                NPC N2 = Main.npc[a];
                N2.ai[0] = (i==0?-1f:1f);
                N2.ai[1] = (float)N.whoAmI;
                N2.ai[3] = (i==0?-300f:0f);
                N2.target = N.target;
                N2.netUpdate = true;
                //Legs.Add(new LegCon(N,N2,0.2f,0.8f));
            }
            #endregion
            #region back legs
            for(int i = 0; i < 2; i++)
            {
                int a = NPC.NewNPC((int)NC.X, (int)NC.Y, "Rosemaw Leg", N.whoAmI);
                NPC N2 = Main.npc[a];
                N2.ai[0] = (i==0?-1.3f:1.3f);
                N2.ai[1] = (float)N.whoAmI;
                N2.target = N.target;
                N2.netUpdate = true;
            }
            for(int i = 0; i < 2; i++)
            {
                int a = NPC.NewNPC((int)NC.X, (int)NC.Y, "Rosemaw Leg", N.whoAmI);
                NPC N2 = Main.npc[a];
                N2.ai[0] = (i==0?-1f:1f);
                N2.ai[1] = (float)N.whoAmI;
                N2.target = N.target;
                N2.netUpdate = true;
            }
            #endregion
            #region ass
            for(int i = 0; i < 1; i++)
            {
                int a = NPC.NewNPC((int)NC.X, (int)NC.Y, "Rosemaw Ass", N.whoAmI);
                NPC N2 = Main.npc[a];
                N2.ai[0] = -1f;
                N2.ai[1] = (float)N.whoAmI;
                N2.target = N.target;
                N2.netUpdate = true;
            }
            #endregion
        }
        FirstFrame = false;
    }


    #endregion

    #region Level 1

    if(N.ai[0] == 1f)
    {
        
        #region idling over target
        
        if(N.ai[1] == 1f)
        {
            N.TargetClosest(true);
            PT = Main.player[N.target];
            PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

            
            #region Rotation

            float Ang = (float)Math.Atan2((double)PTC.Y-NC.Y, (double)PTC.X-NC.X) - 1.57f;
            if (Ang < 0f) Ang+=(float)Math.PI*2f;
            if (Ang > (float)Math.PI*2f) Ang-=(float)Math.PI*2f;

            float RotAng = 0.1f;

            if(N.rotation < Ang)
            {
                if (Ang - N.rotation > Math.PI) N.rotation -= RotAng;
                else N.rotation += RotAng;
            }
            if(N.rotation > Ang)
            {
                if (N.rotation - Ang > Math.PI) N.rotation += RotAng;
                else N.rotation -= RotAng;
            }

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }

            if (N.rotation < 0f) N.rotation+=(float)Math.PI*2f;
            if (N.rotation > (float)Math.PI*2f) N.rotation-=(float)Math.PI*2f;

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }
            float viewangle = (float)((Math.PI*2f)/10f)*2f;
            if(N.rotation > 3f && N.rotation < Math.PI*2f -viewangle) N.rotation = (float)Math.PI*2f -viewangle;
            if(N.rotation < 3f && N.rotation > viewangle) N.rotation = viewangle;
            #endregion

            #region appoint self to above target

            float MoveSpeed = 5f;
            float FixerX = 0.03f,FixerY = 0.2f;
            Vector2 TarVec = PTC-NC+new Vector2(0,-120f);

            Vector2 Pass = Vector2.Zero;
            if(FindSolidTile(NC,2000,ref Pass))
            {
                Pass.Y-=200f;
                Pass-=NC;
                if(Pass.Y > TarVec.Y) 
                {
                    TarVec.Y = Pass.Y;
                    locked = true;
                }
            }

            TarVec *= 5/TarVec.Length();
        
            if(N.velocity.X < TarVec.X) 
                N.velocity.X += FixerX; 
            else N.velocity.X -= FixerX;
            if(N.velocity.X < 0 && TarVec.X > 0) N.velocity.X+= FixerX;
            if(N.velocity.X > 0 && TarVec.X < 0) N.velocity.X-= FixerX;

            if(N.velocity.Y < TarVec.Y) 
                N.velocity.Y += FixerY; 
            else N.velocity.Y -= FixerY;
            if(N.velocity.Y < 0 && TarVec.Y > 0) N.velocity.Y+= FixerY;
            if(N.velocity.Y > 0 && TarVec.Y < 0) N.velocity.Y-= FixerY;        

            #endregion

        }
        
        #endregion     

        #region ponting to something else!

        if(npc.life < (npc.lifeMax*1f))
        {
            N.ai[0] = 2f;
        }

        #endregion
    }

    #endregion

    #region Level 2

    if(N.ai[0] == 2f)
    {
        
        #region idling over target
        
        if(N.ai[1] == 1f)
        {
            N.TargetClosest(true);
            PT = Main.player[N.target];
            PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

            
            #region Rotation

            float Ang = (float)Math.Atan2((double)PTC.Y-NC.Y, (double)PTC.X-NC.X) - 1.57f;
            if (Ang < 0f) Ang+=(float)Math.PI*2f;
            if (Ang > (float)Math.PI*2f) Ang-=(float)Math.PI*2f;

            float RotAng = 0.1f;

            if(N.rotation < Ang)
            {
                if (Ang - N.rotation > Math.PI) N.rotation -= RotAng;
                else N.rotation += RotAng;
            }
            if(N.rotation > Ang)
            {
                if (N.rotation - Ang > Math.PI) N.rotation += RotAng;
                else N.rotation -= RotAng;
            }

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }

            if (N.rotation < 0f) N.rotation+=(float)Math.PI*2f;
            if (N.rotation > (float)Math.PI*2f) N.rotation-=(float)Math.PI*2f;

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }
            float viewangle = (float)((Math.PI*2f)/10f)*2f;
            if(N.rotation > 3f && N.rotation < Math.PI*2f -viewangle) N.rotation = (float)Math.PI*2f -viewangle;
            if(N.rotation < 3f && N.rotation > viewangle) N.rotation = viewangle;
            #endregion

            #region appoint self to above target

            float speedmult = 1.2f;
            float FixerX = 0.03f*speedmult,FixerY = 0.2f*speedmult;
            Vector2 TarVec = PTC-NC+new Vector2(0,-120f);

            Vector2 Pass = Vector2.Zero;
            if(FindSolidTile(NC,2000,ref Pass))
            {
                Pass.Y-=200f;
                Pass-=NC;
                if(Pass.Y > TarVec.Y) 
                {
                    locked = true;
                    TarVec.Y = Pass.Y;

                }
            }

            TarVec *= 5/TarVec.Length();
        
            if(N.velocity.X < TarVec.X) 
                N.velocity.X += FixerX; 
            else N.velocity.X -= FixerX;
            if(N.velocity.X < 0 && TarVec.X > 0) N.velocity.X+= FixerX;
            if(N.velocity.X > 0 && TarVec.X < 0) N.velocity.X-= FixerX;

            if(N.velocity.Y < TarVec.Y) 
                N.velocity.Y += FixerY; 
            else N.velocity.Y -= FixerY;
            if(N.velocity.Y < 0 && TarVec.Y > 0) N.velocity.Y+= FixerY;
            if(N.velocity.Y > 0 && TarVec.Y < 0) N.velocity.Y-= FixerY;        

            #endregion

        }
        
        #endregion     

        #region ponting to something else!

        if(npc.life < (npc.lifeMax*0.75f))
        {
            N.ai[0] = 3f;
        }

        #endregion
    }

    #endregion
    
    #region Level 3

    if(N.ai[0] == 3f)
    {
        
        #region idling over target
        
        if(N.ai[1] == 1f)
        {
            N.TargetClosest(true);
            PT = Main.player[N.target];
            PTC = PT.position+new Vector2(PT.width/2,PT.height/2);

            
            #region Rotation

            float Ang = (float)Math.Atan2((double)PTC.Y-NC.Y, (double)PTC.X-NC.X) - 1.57f;
            if (Ang < 0f) Ang+=(float)Math.PI*2f;
            if (Ang > (float)Math.PI*2f) Ang-=(float)Math.PI*2f;

            float RotAng = 0.1f;

            if(N.rotation < Ang)
            {
                if (Ang - N.rotation > Math.PI) N.rotation -= RotAng;
                else N.rotation += RotAng;
            }
            if(N.rotation > Ang)
            {
                if (N.rotation - Ang > Math.PI) N.rotation += RotAng;
                else N.rotation -= RotAng;
            }

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }

            if (N.rotation < 0f) N.rotation+=(float)Math.PI*2f;
            if (N.rotation > (float)Math.PI*2f) N.rotation-=(float)Math.PI*2f;

            if (N.rotation > Ang - RotAng && N.rotation < Ang + RotAng)
            {
                N.rotation = Ang;
            }
            float viewangle = (float)((Math.PI*2f)/10f)*2f;
            if(N.rotation > 3f && N.rotation < Math.PI*2f -viewangle) N.rotation = (float)Math.PI*2f -viewangle;
            if(N.rotation < 3f && N.rotation > viewangle) N.rotation = viewangle;
            #endregion

            #region appoint self to above target

            float speedmult = 2f;
            float FixerX = 0.03f*speedmult,FixerY = 0.2f*speedmult;
            Vector2 TarVec = PTC-NC+new Vector2(0,-120f);

            Vector2 Pass = Vector2.Zero;
            if(FindSolidTile(NC,2000,ref Pass))
            {
                Pass.Y-=200f;
                Pass-=NC;
                if(Pass.Y > TarVec.Y) 
                {
                    locked = true;
                    TarVec.Y = Pass.Y;

                }
            }

            TarVec *= 5/TarVec.Length();
        
            if(N.velocity.X < TarVec.X) 
                N.velocity.X += FixerX; 
            else N.velocity.X -= FixerX;
            if(N.velocity.X < 0 && TarVec.X > 0) N.velocity.X+= FixerX;
            if(N.velocity.X > 0 && TarVec.X < 0) N.velocity.X-= FixerX;

            if(N.velocity.Y < TarVec.Y) 
                N.velocity.Y += FixerY; 
            else N.velocity.Y -= FixerY;
            if(N.velocity.Y < 0 && TarVec.Y > 0) N.velocity.Y+= FixerY;
            if(N.velocity.Y > 0 && TarVec.Y < 0) N.velocity.Y-= FixerY;        

            #endregion

        }
        
        #endregion     

    }

    #endregion

    if(Vector2.Distance(NC,PTC) > 3000f || PT.dead) 
    {
        N.ai[3]-=2f;
        if(N.ai[3] < -300f) N.velocity.Y+=0.5f;
    }
    if(locked) N.ai[3]++;
    if(N.ai[3] > 300f)
    {
        Vector2 TarVec = PTC-NC+new Vector2(0,-120f);
        N.velocity.Y= TarVec.Y-NC.Y;
        if(N.velocity.Y < -10f) N.velocity.Y = -10f;
        if(NC.Y < PTC.Y) N.ai[3] = 0;
    }
    
}


#endregion


#region DamagePlayer

public void DamagePlayer(Player P,ref int DMG)
{
    P.AddBuff("Frostbite",900);
    if(npc.ai[3] == 4) P.AddBuff(30,900);
}

#endregion


#region FindSolidTile
public bool FindSolidTile(Vector2 Pos,float dist,ref Vector2 Pass)
{
    Vector2 Position = Pos;
    int Width = 2;
    int Height = (int)dist;
    int Radius = 0;

    int LowX = (int)(Position.X / 16f) - Radius;
    int HighX = (int)((Position.X + (float)Width) / 16f) + Radius;
    int LowY = (int)(Position.Y / 16f) - Radius;
    int HighY = (int)((Position.Y + (float)Height) / 16f) + Radius;
    if (LowX < 0)
    {
        LowX = 0;
    }
    if (HighX > Main.maxTilesX)
    {
        HighX = Main.maxTilesX;
    }
    if (LowY < 0)
    {
        LowY = 0;
    }
    if (HighY > Main.maxTilesY)
    {
        HighY = Main.maxTilesY;
    }
    for (int i = LowX; i <= HighX; i++)
    {
        for (int j = LowY; j <= HighY; j++)
        {
            if (Main.tile[i, j] != null && Main.tile[i, j].active && Main.tileSolid[(int)Main.tile[i,j].type])
            {
               Pass.X = i*16 + 8;
               Pass.Y = j*16 + 8;
               return true;
            }
        }
    }
    return false;
}

#endregion

#region Find Frame


public void FindFrame(int currentFrame)
{
    NPC N = npc;

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
	if (Main.netMode == 1) return;
	
    for(int i = 0; i < 2; i++)
    {
	    ModGeneric.WeightedRandom<string> wr = new ModGeneric.WeightedRandom<string>();
	    wr.Add("Rosetta Sword",1);
	    wr.Add(null,1);
	
	    string s = wr.Get();
	    if (s != null) Item.NewItem((int)npc.position.X,(int)npc.position.Y,npc.width,npc.height,s,1,false,-1);
    }
	if (!ModWorld.flagFrostmaw) Item.NewItem((int)npc.position.X,(int)npc.position.Y,npc.width,npc.height,"Divine Stone of Returning Soul",1,false,0);
	ModWorld.flagFrostmaw = true;
	Item.NewItem((int)npc.position.X,(int)npc.position.Y,npc.width,npc.height,"Frozen Shard",Main.rand.Next(1,3),false,0);
	
	ModWorld.AcAchieve("SHK_XMAS_ROSEMAW",null);
}


#endregion


#region Rotate Angle


public float RotateAngle(float Angle, float TargetAngle, float Percents) 
{

    float c, d;

    if (TargetAngle < Angle) 
    {
        c = TargetAngle + MathHelper.TwoPi;
        d = c - Angle > Angle - TargetAngle ? MathHelper.Lerp(Angle, TargetAngle, Percents) : MathHelper.Lerp(Angle, c, Percents);
    } 
    else if (TargetAngle > Angle) 
    {
        c = TargetAngle - MathHelper.TwoPi;
        d = TargetAngle - Angle > Angle - c ? MathHelper.Lerp(Angle, c, Percents) : MathHelper.Lerp(Angle, TargetAngle, Percents);
    } 
    else 
    { 
        return Angle; 
    }

    return MathHelper.WrapAngle(d);
}


#endregion

