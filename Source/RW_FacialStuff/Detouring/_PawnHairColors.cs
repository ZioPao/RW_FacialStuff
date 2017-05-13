﻿using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_FacialStuff.Detouring
{
    public static class _PawnHairColors
    {
        // changed Midnight Black - 10/10/10 - 2016-07-28
        public static Color HairPlatinum = new Color32(255, 245, 226, 255);//(0,929411769f, 0,7921569f, 0,6117647f);
        public static Color HairYellowBlonde = new Color32(255, 203, 89, 255);//(0,75686276f, 0,572549045f, 0,333333343f);
        public static Color HairTerraCotta = new Color32(200, 50, 0, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        public static Color HairMediumDarkBrown = new Color32(110, 70, 10, 255);//(0.25f, 0.2f, 0.15f);
        public static Color HairDarkBrown = new Color32(64, 41, 19, 255);//(0.25f, 0.2f, 0.15f);
        public static Color HairMidnightBlack = new Color32(30, 30, 30, 255);//(0.2f, 0.2f, 0.2f);

        public static Color HairDarkPurple = new Color32(191, 92, 85, 255);
        public static Color HairBlueSteel = new Color32(57, 115, 199, 255);
        public static Color HairBurgundyBistro = new Color32(208, 65, 82, 255);
        public static Color HairGreenGrape = new Color32(124, 189, 14, 255);
        public static Color HairMysticTurquois = new Color32(71, 191, 165, 255);
        public static Color HairPinkPearl = new Color32(230, 74, 103, 255);
        public static Color HairPurplePassion = new Color32(145, 50, 191, 255);
        public static Color HairRosaRosa = new Color32(243, 214, 200, 255);
        public static Color HairRubyRed = new Color32(227, 35, 41, 255);
        public static Color HairUltraViolet = new Color32(191, 53, 132, 255);



        //      public static Color Dark01 = new Color32(50, 50, 50, 255);//(0.2f, 0.2f, 0.2f);
        //      public static Color Dark02 = new Color32(79, 71, 66, 255);//(0.31f, 0.28f, 0.26f);
        //      public static Color Dark03 = new Color32(64, 50, 37, 255);//(0.25f, 0.2f, 0.15f);
        //      public static Color Dark04 = new Color32(75, 50, 25, 255);//(0.3f, 0.2f, 0.1f);
        //
        //      public static Color Color01 = new Color32(90, 58, 32, 255);//(0,3529412f, 0,227450982f, 0,1254902f);
        //      public static Color Color02 = new Color32(132, 83, 47, 255);//(0,5176471f, 0,3254902f, 0,184313729f);
        //      public static Color Color03 = new Color32(193, 146, 85, 255);//(0,75686276f, 0,572549045f, 0,333333343f);
        //      public static Color Color04 = new Color32(237, 202, 156, 255);//(0,929411769f, 0,7921569f, 0,6117647f);

        [Detour(typeof(PawnHairColors), bindingFlags = (BindingFlags.Static | BindingFlags.Public))]
        // ReSharper disable once UnusedMember.Global
        public static Color RandomHairColor(Color skinColor, int ageYears)
        {
            Color tempColor;

            if (Rand.Value < 0.02f)
            {
                float rand = Rand.Value;
                if (rand < 0.2f)
                    return new Color(HairBlueSteel.r * Rand.Range(0.25f, 1f), HairBlueSteel.g * Rand.Range(0.25f, 1f), HairBlueSteel.b * Rand.Range(0.25f, 1f), 255);
                if (rand < 0.4f)
                    return new Color(HairGreenGrape.r * Rand.Range(0.25f, 1f), HairGreenGrape.g * Rand.Range(0.25f, 1f), HairGreenGrape.b * Rand.Range(0.25f, 1f), 255);
                if (rand < 0.6f)
                    return new Color(HairMysticTurquois.r * Rand.Range(0.25f, 1f), HairMysticTurquois.g * Rand.Range(0.25f, 1f), HairMysticTurquois.b * Rand.Range(0.25f, 1f), 255);
                if (rand < 0.8f)
                    return new Color(HairPurplePassion.r * Rand.Range(0.25f, 1f), HairPurplePassion.g * Rand.Range(0.25f, 1f), HairPurplePassion.b * Rand.Range(0.25f, 1f), 255);

                return new Color(HairUltraViolet.r * Rand.Range(0.25f, 1f), HairUltraViolet.g * Rand.Range(0.25f, 1f), HairUltraViolet.b * Rand.Range(0.25f, 1f), 255);
            }          //if (Rand.Value < 0.02f)

            if (_PawnSkinColors.IsDarkSkin(skinColor))// || Rand.Value < 0.4f)
            {
                tempColor = Color.Lerp(HairMidnightBlack, HairDarkBrown, Rand.Range(0f, 0.35f));

                tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0f, 0.3f));
                tempColor = Color.Lerp(tempColor, HairPlatinum, Rand.Range(0f, 0.45f));
            }
            else
            {
                float value2 = Rand.Value;

                //dark hair
                if (value2 < 0.125f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 0.7f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0.1f, 0.8f));
                }
                //brown hair
                else if (value2 < 0.25f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairTerraCotta, Rand.Range(0f, 0.6f));
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0.3f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.5f));
                }
                //dirty blonde hair
                else if (value2 < 0.5f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Value);
                    tempColor = Color.Lerp(tempColor, HairMediumDarkBrown, Rand.Range(0f, 0.35f));
                }
                // dark red/brown hair
                else if (value2 < 0.7f)
                {
                    tempColor = Color.Lerp(HairPlatinum, HairTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.35f));
                }
                // pure blond / albino
                else if (value2 < 0.85f)
                    tempColor = Color.Lerp(HairPlatinum, HairYellowBlonde, Rand.Range(0f, 0.5f));

                // red hair
                else
                {
                    tempColor = Color.Lerp(HairYellowBlonde, HairTerraCotta, Rand.Range(0.25f, 1f));
                    tempColor = Color.Lerp(tempColor, HairMidnightBlack, Rand.Range(0f, 0.25f));
                }

            }

            // age to become gray as float
            float agingGreyFloat = Rand.Range(0.35f, 0.7f);

            float greyness = 0f;

            if ((float)ageYears / 100 > agingGreyFloat)
            {
                greyness = ((float)ageYears / 100 - agingGreyFloat) * 5f;

                if (greyness > 0.85f)
                {
                    greyness = 0.85f;
                }

                if (PawnSkinColors.IsDarkSkin(skinColor))
                    greyness *= Rand.Range(0.4f, 0.8f);
            }

            return Color.Lerp(tempColor, new Color32(245, 245, 245, 255), greyness);
        }
    }
}
