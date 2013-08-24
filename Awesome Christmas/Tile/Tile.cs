int SnowAbdudance = 20;

int SnowDepthMin = 20;
int SnowDepthMax = 40;

int snowWidthMin = 20;
int snowWidthMax = 50;
int snowWidthBase = 20;

public void ModifyWorld()
{
    if (Settings.GetBool("snow")) {
		Main.statusText = "Making snow masses";
		for(int zzz = 0; zzz < SnowAbdudance; zzz++) AddSnows();
	}
	if (Settings.GetBool("icegen")) {
		Main.statusText = "Adding frosty caves";
		for (int i = 0; i < 3; i++) AddIceArena(); //made 3 ice cone areas generate
	}
}

#region add ice arena
public void AddIceArena()
{
    #region generate point
    int px = WorldGen.genRand.Next(Main.maxTilesX);
    while ((float)px < (float)Main.maxTilesX * 0.25f || (float)px > (float)Main.maxTilesX * 0.75f)
    {
    	px = WorldGen.genRand.Next(Main.maxTilesX);
    }
    #endregion

    #region make start and end
    int rad = snowWidthBase;
    float rate = (float)(Main.maxTilesX / 4200)*1.5f; //increased radius size by .5
    rad += (int)((float)WorldGen.genRand.Next(snowWidthMin, snowWidthMax) * rate);
    int start = px - rad;
    rad = snowWidthBase;
    rad += (int)((float)WorldGen.genRand.Next(snowWidthMin, snowWidthMax) * rate);
    int end = px + rad;
    if (start < 0)
    {
    	start = 0;
    }
    if (end > Main.maxTilesX)
    {
    	end = Main.maxTilesX;
    }
    #endregion


    Func<int,int,bool> ChestTile = (x,y) => Main.tile[x,y].type == 21 || Main.tile[x,y].type == 29 || Main.tile[x,y].type == 97;
    for(int phase = 1; phase < 6; phase++)
    {
        #region run snow maker
        for (int runnerX = start; runnerX < end; runnerX++)
        {
    	    int runnerY = (int)((float)Main.worldSurface*0.4f); //changed to not mess with floating islands.
            while ((double)runnerY < WorldGen.lavaLine-50) //we don't want to mess with lava.
            {
                int sx = runnerX,sy = runnerY;
    	        if (Main.tile[sx, sy].active && !ChestTile(sx,sy))
    	        {
                   if(phase == 1) if(!Main.tileSolid[(int)Main.tile[sx,sy].type]) Main.tile[sx,sy].active = false;
                   if(phase == 2) if(Main.tile[sx,sy].type == 0 || Main.tile[sx,sy].type == 1 || Main.tile[sx,sy].type == 2) Main.tile[sx,sy].type = 147;
                   if(phase == 3) 
                    {
                        int c = 0; 
                        if(!Main.tile[sx-1,sy].active && !ChestTile(sx-1,sy)) c++;
                        if(!Main.tile[sx+1,sy].active && !ChestTile(sx+1,sy)) c++;
                        if(!Main.tile[sx,sy-1].active && !ChestTile(sx,sy-1)) c++;
                        if(!Main.tile[sx,sy+1].active && !ChestTile(sx,sy+1)) c++;
                        if(c > 2) Main.tile[sx,sy].active = false;
                    }
                    if(phase == 4)
                    {
                        int c = 0; 
                        if(!Main.tile[sx-1,sy].active) c++;
                        if(!Main.tile[sx+1,sy].active) c++;
                        if(!Main.tile[sx,sy-1].active) c++;
                        if(!Main.tile[sx,sy+1].active) c++;
                        if(c == 1) Main.tile[sx,sy].type = (ushort)Config.tileDefs.ID["Icemaw"];
                    }
                    if(phase == 5)
                    {
                        int c = 0; 
                        ushort d = (ushort)Config.tileDefs.ID["Icemaw"]; 
                        if(Main.tile[sx-1,sy].active && Main.tile[sx-1,sy].type == d) c++;
                        if(Main.tile[sx+1,sy].active && Main.tile[sx+1,sy].type == d) c++;
                        if(Main.tile[sx,sy-1].active && Main.tile[sx,sy-1].type == d) c++;
                        if(Main.tile[sx,sy+1].active && Main.tile[sx,sy+1].type == d) c++;
                        int e = 0;
                        for(int ii = sx-1; ii < sx+2; ii++)
                            for(int ij = sy-1; ij < sy+2; ij++)
                              if(!Main.tile[ii,ij].active) e++;
                        if(c == 2 && e > 0) Main.tile[sx,sy].type = d;
                    }
                }
    		    runnerY++;
            }
    	}
        #endregion
    }
}
#endregion

#region crap code

public void AddSnows()
{
        #region generate point
        int px = WorldGen.genRand.Next(Main.maxTilesX);
        while ((float)px < (float)Main.maxTilesX * 0.25f || (float)px > (float)Main.maxTilesX * 0.75f)
        {
    	    px = WorldGen.genRand.Next(Main.maxTilesX);
        }
        #endregion

        #region make start and end
        int rad = snowWidthBase;
        float rate = (float)(Main.maxTilesX / 4200);
        rad += (int)((float)WorldGen.genRand.Next(snowWidthMin, snowWidthMax) * rate);
        int start = px - rad;
        rad = snowWidthBase;
        rad += (int)((float)WorldGen.genRand.Next(snowWidthMin, snowWidthMax) * rate);
        int end = px + rad;
        if (start < 0)
        {
    	    start = 0;
        }
        if (end > Main.maxTilesX)
        {
    	    end = Main.maxTilesX;
        }
        #endregion

        int snowDepth = WorldGen.genRand.Next(SnowDepthMin, SnowDepthMax+1);

        for (int runnerX = start; runnerX < end; runnerX++)
        {
            #region randomize the depth a bit
    	    if (WorldGen.genRand.Next(2) == 0)
    	    {
    		    snowDepth += WorldGen.genRand.Next(-1, 2);
    		    if (snowDepth < SnowDepthMin)
    		    {
    			    snowDepth = SnowDepthMin;
    		    }
    		    if (snowDepth > SnowDepthMax)
    		    {
    			    snowDepth = SnowDepthMax;
    		    }
    	    }
            #endregion

            #region run snow maker
    	    int runnerY = 0;
    	    while ((double)runnerY < Main.worldSurface)
    	    {
    		    if (Main.tile[runnerX, runnerY].active)
    		    {

                    #region smooth the curve
    			    int decDepth = snowDepth;
    			    if (runnerX - start < decDepth)
    			    {
                        decDepth = runnerX - start;
    			    }
    			    if (end - runnerX < decDepth)
    			    {
                        decDepth = end - runnerX;
    			    }
    			    decDepth += WorldGen.genRand.Next(5);
                    #endregion

    			    for (int snowRunner = runnerY; snowRunner < runnerY + decDepth; snowRunner++)
    			    {
                        if (runnerX > start + WorldGen.genRand.Next(5) && runnerX < end - WorldGen.genRand.Next(5))
                        {
        	                TileTurn(Main.tile[runnerX, snowRunner]);
                        }
    			    }
    			    break;
    		    }
    		    runnerY++;
    	    }
            #endregion
        }
}

#endregion

public void TileTurn(Tile T)
{
    if(!T.active) return;

    int tt = T.type;

    if(tt == 0 || tt == 2 || tt == 23)
    {
        T.type = (ushort)147;
    }

    if(tt == 3 || tt == 27 || tt == 24 || tt == 73 || tt == 52 || tt == 82 || tt == 83 || tt == 84)
    {
        T.type = 0;
        T.active = false;
    }
}