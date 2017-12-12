﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlienFace
{
    using AlienRace;

    using FacialStuff;

    using UnityEngine;

    using Verse;

    public class AlienDrawer : PawnDrawer
    {
        public AlienDrawer()
        {
            // Needs a constructor
        }


        public override Mesh GetPawnMesh(Rot4 facing, bool wantsBody, bool portrait) =>
            this.CompFace.Pawn.def is ThingDef_AlienRace alienProps ?
                portrait ?
                    wantsBody ?
                        alienProps.alienRace.generalSettings.alienPartGenerator.bodyPortraitSet.MeshAt(facing) :
                        alienProps.alienRace.generalSettings.alienPartGenerator.headPortraitSet.MeshAt(facing) :
                    wantsBody ?
                        alienProps.alienRace.generalSettings.alienPartGenerator.bodySet.MeshAt(facing) :
                        alienProps.alienRace.generalSettings.alienPartGenerator.headSet.MeshAt(facing) :
                wantsBody ?
                    MeshPool.humanlikeBodySet.MeshAt(facing) :
                    MeshPool.humanlikeHeadSet.MeshAt(facing);

        public override Mesh GetPawnHairMesh(PawnGraphicSet graphics, Rot4 headFacing, bool portrait) =>
            this.CompFace.Pawn.def is ThingDef_AlienRace alienProps ?
                (this.CompFace.Pawn.story.crownType == CrownType.Narrow ?
                     (portrait ?
                          alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetNarrow :
                          alienProps.alienRace.generalSettings.alienPartGenerator.hairSetNarrow) :
                     (portrait ?
                          alienProps.alienRace.generalSettings.alienPartGenerator.hairPortraitSetAverage :
                          alienProps.alienRace.generalSettings.alienPartGenerator.hairSetAverage)).MeshAt(headFacing) :
                graphics.HairMeshSet.MeshAt(headFacing);

        public override void DrawAlienBodyAddons(
            Quaternion quat,
            Rot4 bodyFacing,
            Vector3 rootLoc,
            bool portrait,
            bool renderBody,
            PawnGraphicSet graphics)
        {
            var pawn = this.CompFace.Pawn;
            if (pawn.def is ThingDef_AlienRace alienProps)
            {

                List<AlienPartGenerator.BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
                AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                for (int i = 0; i < addons.Count; i++)
                {
                    AlienPartGenerator.BodyAddon ba = addons[i];


                    if (ba.CanDrawAddon(pawn))
                    {

                        Mesh mesh = portrait ? ba.addonPortraitMeshFlipped : ba.addonMesh;

                        Rot4 rotation = pawn.Rotation;
                        if (portrait)
                            rotation = Rot4.South;
                        AlienPartGenerator.RotationOffset offset = rotation == Rot4.South ? ba.offsets.front : rotation == Rot4.North ? ba.offsets.back : ba.offsets.side;
                        //Log.Message("front: " + (offset == ba.offsets.front).ToString() + "\nback: " + (offset == ba.offsets.back).ToString() + "\nside :" + (offset == ba.offsets.side).ToString());
                        Vector2 bodyOffset = offset?.bodyTypes?.FirstOrDefault(to => to.bodyType == pawn.story.bodyType)?.offset ?? Vector2.zero;
                        Vector2 crownOffset = offset?.crownTypes?.FirstOrDefault(to => to.crownType == alienComp.crownType)?.offset ?? Vector2.zero;

                        //front 0.42f, -0.3f, -0.22f
                        //back     0f,  0.3f, -0.55f
                        //side -0.42f, -0.3f, -0.22f   

                        float MoffsetX = 0.42f;
                        float MoffsetZ = -0.22f;
                        float MoffsetY = ba.inFrontOfBody ? 0.3f : -0.3f;
                        float num = ba.angle;

                        if (rotation == Rot4.North)
                        {
                            MoffsetX = 0f;
                            MoffsetY = !ba.inFrontOfBody ? 0.3f : -0.3f;
                            MoffsetZ = -0.55f;
                            num = 0;
                        }

                        MoffsetX += bodyOffset.x + crownOffset.x;
                        MoffsetZ += bodyOffset.y + crownOffset.y;

                        if (rotation == Rot4.East)
                        {
                            MoffsetX = -MoffsetX;
                            num = -num; //Angle
                            mesh = ba.addonMeshFlipped;
                        }

                        Vector3 scaleVector = new Vector3(MoffsetX, MoffsetY, MoffsetZ);
                        scaleVector.x *= 1f + (1f - (portrait ?
                                                         alienProps.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize :
                                                         alienProps.alienRace.generalSettings.alienPartGenerator.customDrawSize)
                                               .x);
                        scaleVector.z *= 1f + (1f - (portrait ?
                                                         alienProps.alienRace.generalSettings.alienPartGenerator.customPortraitDrawSize :
                                                         alienProps.alienRace.generalSettings.alienPartGenerator.customDrawSize)
                                               .y);

                        GenDraw.DrawMeshNowOrLater(mesh, rootLoc + scaleVector, Quaternion.AngleAxis(num, Vector3.up), alienComp.addonGraphics[i].MatAt(rotation), portrait);
                    }
                }
            }
        }

    }
}