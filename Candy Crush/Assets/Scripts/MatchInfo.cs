﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//classe q vai guardar informaçoes
public class MatchInfo
{
    public List<GridItem> match; // para conseguir acessar

    public int horizontalMatchStart;//guardar indices
    public int horizontalMatchEnd;

    public int verticalMatchStart;
    public int verticalMatchEnd;

    public bool IsMatchValid
    {
        get { return match != null; }//verifiva se e nulo ou nao 
    }
}