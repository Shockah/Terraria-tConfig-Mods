public class LegCon
{
    public NPC N;
    public NPC N2;
    public Vector2[] pos = new Vector2[4];

    public float con = 0.785f;
    public float CSize1 = 90f;
    public float CSize2 = 90f;
    public float conClose = 0.2f;
    public float conFar = 0.8f;
    public Color col = Color.White;
    public Texture2D tex;
    public LegCon(NPC S,NPC E,float s1 = -1f,float s2 = -1f,float c1 = -1f,float c2 = -1f,Color C = default(Color))
    {
        N = S;
        N2 = E;
        con = 0.785f;
        pos = new Vector2[4];
        
        Vector2 NC = N.position+new Vector2(N.width/2f,N.height/2f);
        for(int i = 0; i < pos.Length; i++) pos[i] = NC;

        if(s1 != -1f) conClose = s1;
        if(s2 != -1f) conFar = s2;
        if(c1 != -1f) CSize1 = c1;
        if(c2 != -1f) CSize2 = c2;
        if(C!=default(Color)) col = C;

        //tex = Main.goreTexture[Config.goreID["Frostmaw CPath"]];
        tex = new Texture2D(Config.mainInstance.GraphicsDevice,4,4);
        Color[] ar = new Color[16]; for(int i = 0; i < ar.Length; i++) ar[i] = Color.White;
        tex.SetData(ar , 0,16);
    }

    public void Update()
    {
        Vector2 NC = N.position+new Vector2(N.width/2f,N.height/2f);
        Vector2 N2C = N2.position+new Vector2(N2.width/2f,N2.height/16f);
               
        
        for(int i = 1; i < 3; i++)
        {
            Vector2 P = Vector2.Zero;
            if(i == 1)
            {
                P = Vector2.Lerp(NC,N2C,conClose);
                P.Y = (float)Math.Max(N2C.Y,NC.Y) + 40f;
                P = P-NC;
                P *= CSize1/P.Length();
                P = NC+P;
            }
            if(i == 2)
            {
                P = Vector2.Lerp(NC,N2C,conFar);
                P.Y = (float)Math.Min(N2C.Y,NC.Y) - 60f;
                P = P-N2C;
                P *= CSize2/P.Length();
                P = N2C+P;
            }
            pos[i] = Vector2.Lerp(pos[i],P,0.1f);
        }

        pos[0] = Vector2.Lerp(pos[0],NC,1f);
        pos[3] = Vector2.Lerp(pos[3],N2C,1f);
    }

    public void Draw(SpriteBatch SP,int phase,Color special = default(Color))
    {
        if(special == default(Color))DrawLine(SP,pos[phase],pos[phase+1],tex,1f,col,2);
        else DrawLine(SP,pos[phase],pos[phase+1],tex,0.2f,special,1);
        //DrawLine(SP,pos[phase],pos[phase+1],tex,0.67f,col,8);
        //Texture2D TEX = Main.goreTexture[Config.goreID["Frostmaw CEnd"]];
        //SP.Draw(TEX,pos[phase+1]-Main.screenPosition,null,Color.White,0f,new Vector2(),0.5f,SpriteEffects.None,0f);
    }
    
    public static bool LineOnRect(Vector2 RPos,double RWidth,double RHeight,Vector2 P1,Vector2 P2)
    {
        // Find min and max X for the segment

        double minX = P1.X;
        double maxX = P2.X;

        if(P1.X > P2.X)
        {
          minX = P2.X;
          maxX = P1.X;
        }
        if(maxX > RWidth+RPos.X)
        {
          maxX = RWidth+RPos.X;
        }
        if(minX < RPos.X)
        {
          minX = RPos.X;
        }
        if(minX > maxX)
        {
          return false;
        }
        double minY = P1.Y;
        double maxY = P2.Y;

        double dx = P2.X - P1.X;

        if(Math.Abs(dx) > 0.0000001)
        {
          double a = (P2.Y - P1.Y) / dx;
          double b = P1.Y - a * P1.X;
          minY = a * minX + b;
          maxY = a * maxX + b;
        }
        if(minY > maxY)
        {
          double tmp = maxY;
          maxY = minY;
          minY = tmp;
        }
        if(maxY > RHeight+RPos.Y)
        {
          maxY = RHeight+RPos.Y;
        }
        if(minY < RPos.Y)
        {
          minY = RPos.Y;
        }
        if(minY > maxY) // If Y-projections do not intersect return false
        {
          return false;
        }
        return true;
    }
    
    public void DrawLine(SpriteBatch SP,Vector2 start,Vector2 end,Texture2D TEX,float S,Color C,float Jump)
    {
        float TEXW = (float)TEX.Width;
        float TEXH = (float)TEX.Height;
        Vector2 TC = new Vector2(TEXW / 2f, TEXH / 2f);
        TC*=S;
        Vector2 Pstart = start;
        Vector2 Pend = end;
        Vector2 dir = Pend - Pstart;
        dir.Normalize();
        //dir/=dir.Length();
        bool screenSafety = true;
        if(screenSafety)
        {
            int alloRad = 16;
            #region pstart
            if(Pstart.X < Main.screenPosition.X-alloRad)
            {
                Pstart.Y+=((Main.screenPosition.X-alloRad-Pstart.X)/dir.X)*dir.Y;
                Pstart.X = Main.screenPosition.X-alloRad;
            }
            if(Pstart.X > Main.screenPosition.X+Main.screenWidth+alloRad)
            {
                Pstart.Y-=((Pstart.X-Main.screenPosition.X-Main.screenWidth-alloRad)/dir.X)*dir.Y;
                Pstart.X = Main.screenPosition.X+Main.screenWidth+alloRad;
            }
            if(Pstart.Y < Main.screenPosition.Y-alloRad)
            {
                Pstart.X+=((Main.screenPosition.Y-alloRad-Pstart.Y)/dir.Y)*dir.X;
                Pstart.Y = Main.screenPosition.Y-alloRad;
            }
            if(Pstart.Y > Main.screenPosition.Y+Main.screenHeight+alloRad)
            {
                Pstart.X-=((Pstart.Y-Main.screenPosition.Y-Main.screenHeight-alloRad)/dir.Y)*dir.X;
                Pstart.Y = Main.screenPosition.Y+Main.screenHeight+alloRad;
            }
            #endregion
            #region pend
            if(Pend.X < Main.screenPosition.X-alloRad)
            {
                Pend.Y+=((Main.screenPosition.X-alloRad-Pend.X)/dir.X)*dir.Y;
                Pend.X = Main.screenPosition.X-alloRad;
            }
            if(Pend.X > Main.screenPosition.X+Main.screenWidth+alloRad)
            {
                Pend.Y-=((Pend.X-Main.screenPosition.X-Main.screenWidth-alloRad)/dir.X)*dir.Y;
                Pend.X = Main.screenPosition.X+Main.screenWidth+alloRad;
            }
            if(Pend.Y < Main.screenPosition.Y-alloRad)
            {
                Pend.X+=((Main.screenPosition.Y-alloRad-Pend.Y)/dir.Y)*dir.X;
                Pend.Y = Main.screenPosition.Y-alloRad;
            }
            if(Pend.Y > Main.screenPosition.Y+Main.screenHeight+alloRad)
            {
                Pend.X-=((Pend.Y-Main.screenPosition.Y-Main.screenHeight-alloRad)/dir.Y)*dir.X;
                Pend.Y = Main.screenPosition.Y+Main.screenHeight+alloRad;
            }
            #endregion
        }
        float rot = (float)Math.Atan2(dir.Y,dir.X) + (float)Math.PI/2f;
        float length = Vector2.Distance(Pstart,Pend);
        float Way = 0f;

        while (Way < length)
        {
            Vector2 v = (Pstart + dir * Way) - Main.screenPosition + TC;
            Color C2 = Lighting.GetColor((int)(v.X+Main.screenPosition.X)/16,(int)(v.Y+Main.screenPosition.Y)/16,C);
            SP.Draw(
            TEX,
            v,
            new Rectangle?(new Rectangle(0, 0, (int)TEXW, (int)TEXH)),
            C2,
            rot,
            TC,
            S,
            SpriteEffects.None, 0f
            );
            Way += Jump;
        }
    }
}

public LegCon MyLeg;

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

    N.spriteDirection = (int)N.ai[0];
    NPC M = Main.npc[(int)N.ai[1]];
    Vector2 MC = M.position+new Vector2(M.width/2f,M.height/2f);

    N.dontTakeDamage=true;

    #endregion

    #region die if boss is gone

    N.displayName = M.displayName;
    if(!M.active || M.type != Config.npcDefs.byName["Frostmaw"].type)
    {
		N.life = -1;
		N.HitEffect(0, 10.0);
		N.active = false;
    }

    #endregion

    #region first frame
    
    if(FirstFrame)
    {
        if(!Main.dedServ) MyLeg = new LegCon(M,N,0.8f,0.8f,100,100,Color.White);
        FirstFrame = false;
        N.damage = 0;
        N.realLife = (int)N.ai[1];
    }

    #endregion

    #region first phase : IDLE

    if(N.ai[2] == 0)
    {
        N.target = M.target;
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
        float viewangle = (float)((Math.PI*2f)/10f)*1f;
        if(N.rotation > 3f && N.rotation < Math.PI*2f -viewangle) N.rotation = (float)Math.PI*2f -viewangle;
        if(N.rotation < 3f && N.rotation > viewangle) N.rotation = viewangle;
        #endregion

        #region appoint self to above target

        float speedmult = 1f;
        if(M.ai[0] == 2) speedmult = 1.2f;
        if(M.ai[0] == 3) speedmult = 1.4f;
        if(M.ai[0] == 4) speedmult = 1.5f;
        float FixerX = 0.05f*speedmult,FixerY = 0.2f*speedmult;
        Vector2 TarVec = PTC-NC+new Vector2(100f*N.ai[0],-30f);


        #region limit to not go to the skies
        Vector2 Pass = Vector2.Zero;
        if(FindSolidTile(NC,2000,ref Pass))
        {
            Pass.Y-=400f;
            Pass-=NC;
            if(Pass.Y > TarVec.Y) 
            {
                TarVec.Y = Pass.Y;

            }
        }
        #endregion

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
    
        #region taking care of other phases
        
        N.ai[3]++;
        int wait = 510;
        if(M.ai[0] == 4) wait = (int)(wait*0.4f);
        else if(M.ai[0] == 3) wait = (int)(wait*0.6f);
        else if(M.ai[0] == 2) wait = (int)(wait*0.8f); 
        if(N.ai[3] > wait)
        {
            N.ai[3] = 0;
            N.ai[2] = 1;
            N.velocity = (NC-PTC)+new Vector2(Main.rand.Next(101)*N.ai[0],Main.rand.Next(-100,1));
            N.velocity*= 2f/N.velocity.Length();
        }

        #endregion
    }

    #endregion
    
    #region second phase : Float
    
    if(N.ai[2] == 1)
    {
        N.ai[3]++;
        int wait = 60;
        if(M.ai[0] == 3) wait = (int)(wait*0.6f);
        else if(M.ai[0] == 2) wait = (int)(wait*0.8f);
        if(N.ai[3] > wait)
        {
            N.ai[3] = 0;
            N.ai[2] = 2;
            N.velocity = (PTC-NC);
            float chargespeed = 10f;
            if(M.ai[0] == 3) chargespeed = 15f;
            else if(M.ai[0] == 2) chargespeed = 12f;
            N.velocity*= chargespeed/N.velocity.Length();
            N.damage = 20;
        }
    }

    #endregion
    
    #region third phase : Charge
    
    if(N.ai[2] == 2)
    {
        N.ai[3]++;
        int wait = 40;
        if(M.ai[0] == 3) wait = (int)(wait*0.8f);
        else if(M.ai[0] == 2) wait = (int)(wait*0.6f);
        if(N.ai[3] > wait)
        {
            N.ai[3] = 0;
            N.ai[2] = 0;
            N.velocity = Vector2.Zero;
            N.damage = 0;
        }
    }

    #endregion

    #region limit range from head
    float ranger = 500f;
    if(M.ai[0] == 2) ranger = 300f;
    if((MC-NC).Length() > ranger) 
    {
        N.position = MC-(MC-NC)*ranger/(MC-NC).Length() - new Vector2(N.width,N.height)/2f;
    }
    #endregion

    if(M.ai[3] > 300f) N.velocity.Y=-10f;
}


#endregion

#region RunIceDetection

public bool RunIceDetection()
{
    Vector2 Position = npc.position;
    int Width = npc.width;
    int Height = npc.height;
    int Radius = -1;

    int[] A = { 0,1,2,147 };

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
            if (Main.tile[i, j] != null && Main.tile[i, j].active && IsATargetTile(Main.tile[i,j].type,A))
            {
               return true;
            }
        }
    }
    return false;
}

public static bool IsATargetTile(int x,int[] t)
{
    foreach(int y in t) if(x==y) return true;
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
    //N.spriteDirection = -1;
    }
    else
    {
    N.direction = 1;
    //N.spriteDirection = 1;
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

#region DamagePlayer

public void DamagePlayer(Player P,ref int DMG)
{
    P.AddBuff("Frostbite",900);
}

#endregion

public bool PreDraw(SpriteBatch SP)
{
    if(!Main.gamePaused) MyLeg.Update();
    MyLeg.Draw(SP,0);
    MyLeg.Draw(SP,1);
    MyLeg.Draw(SP,2);
    return true;
}
