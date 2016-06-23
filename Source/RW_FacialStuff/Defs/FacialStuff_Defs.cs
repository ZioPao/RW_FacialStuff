﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RW_FacialStuff
{
    public class BeardDef : Def
    {
        public string texPathAverageNormal;
        public string texPathAveragePointy;
        public string texPathAverageWide;

        public string texPathNarrowNormal;
        public string texPathNarrowPointy;
        public string texPathNarrowWide;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class TacheDef : Def
    {
        public string texPathAverageNormal;
        public string texPathAveragePointy;
        public string texPathAverageWide;

        public string texPathNarrowNormal;
        public string texPathNarrowPointy;
        public string texPathNarrowWide;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class SideburnDef : Def
    {
        public string texPathAverage;

        public string texPathNarrow;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class EyeDef: Def
    {

        public string texPathAverage;

        public string texPathNarrow;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

    public class WrinkleDef : Def
    {

        public string texPathAverage;

        public string texPathNarrow;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }


    public class LipDef : Def
    {
        public string texPathAverage;

        public string texPathNarrow;

        public HairGender hairGender = HairGender.Any;

        public List<string> hairTags = new List<string>();

    }

}
