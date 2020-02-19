using System;
using System.Collections.Generic;
using System.Reflection;
using FacialStuff.AnimatorWindows;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using static FacialStuff.Offsets;

namespace FacialStuff.HarmonyLib
{

    #region DrawPos
    [HarmonyPatch(typeof(Pawn_DrawTracker))]
    [HarmonyPatch("DrawPos", MethodType.Getter)]
    class DrawPos_Patch
    {
        public static bool offsetEnabled = false;
        public static Vector3 offset = Vector3.zero;

        static void Postfix(ref Vector3 __result)
        {
            if (offsetEnabled)
            {
                __result = offset;
            }
        }
    }
    #endregion

    #region RenderPawnInternal
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]  //This method is hidden, you have to decompile the whole thing to see it
    [HarmonyPatch(new Type[] {typeof(Vector3), typeof(float), typeof(bool), 
                                typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
                                typeof(bool), typeof(bool), typeof(bool) })]
    [HarmonyBefore("com.showhair.rimworld.mod")]
    class HarmonyPatch_PawnRenderer
    {

        // Verse.Altitudes
        public const float LayerSpacing = 0.46875f;

        private static readonly Type _pawnRendererType = typeof(PawnRenderer);

        private static readonly MethodInfo DrawEquipmentMethodInfo = _pawnRendererType.GetMethod(
        "DrawEquipment",
        BindingFlags.NonPublic | BindingFlags.Instance);


        // private static FieldInfo PawnFieldInfo;
        private static readonly FieldInfo WoundOverlayFieldInfo = _pawnRendererType.GetField(
                                                                                     "woundOverlays",
                                                                                     BindingFlags.NonPublic | BindingFlags.Instance);

        /*
                private static bool _logY = false;
        */
        private static readonly FieldInfo PawnHeadOverlaysFieldInfo = _pawnRendererType.GetField(
                                                                                        "statusOverlays",
                                                                                        BindingFlags.NonPublic | BindingFlags.Instance);




        private static void RecalcRootLocY(ref Vector3 rootLoc, Pawn pawn, CompBodyAnimator compAnimator)
        {
            Vector3 loc = rootLoc;
            CellRect viewRect = Find.CameraDriver.CurrentViewRect;
            viewRect = viewRect.ExpandedBy(1);

            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn otherPawn in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (!viewRect.Contains(otherPawn.Position)) { continue; }
                if (otherPawn == pawn) { continue; }
                if (otherPawn.DrawPos.x < loc.x - 0.5f)
                { continue; }

                if (otherPawn.DrawPos.x > loc.x + 0.5f)
                { continue; }

                if (otherPawn.DrawPos.z >= loc.z) { continue; } // ignore above

                pawns.Add(otherPawn);
            }
            // pawns = pawn.Map.mapPawns.AllPawnsSpawned
            //                .Where(
            //                       otherPawn => _viewRect.Contains(otherPawn.Position) &&
            //                       otherPawn != pawn &&
            //                                    otherPawn.DrawPos.x >= loc.x - 1 &&
            //                                    otherPawn.DrawPos.x <= loc.x + 1 &&
            //                                    otherPawn.DrawPos.z <= loc.z).ToList();
            //  List<Pawn> leftOfPawn = pawns.Where(other => other.DrawPos.x <= loc.x).ToList();
            bool flag = compAnimator != null;
            if (!pawns.NullOrEmpty())
            {
                float pawnOffset = YOffsetPawns * pawns.Count;
                loc.y -= pawnOffset;
                if (flag)
                {
                    compAnimator.DrawOffsetY = pawnOffset;
                }
                //   loc.y -= 0.1f * leftOfPawn.Count;
            }
            else
            {
                if (flag)
                {
                    compAnimator.DrawOffsetY = 0f;
                }
            }
            rootLoc = loc;
        }


        /// <summary>
        ///     Simple Circle to Circle intersection test.
        /// </summary>
        /// <param name="x0">Circle 0 X-position</param>
        /// <param name="y0">Circle 0 Y-position</param>
        /// <param name="radius0">Circle 0 Radius</param>
        /// <param name="x1">Circle 1 X-position</param>
        /// <param name="y1">Circle 1 Y-position</param>
        /// <param name="radius1">Circle 1 Radius</param>
        /// <returns>True if a intersection occured. False if not.</returns>
        public static bool CircleIntersectionTest(float x0, float y0, float radius0, float x1, float y1, float radius1)
        {
            float radiusSum = radius0 * radius0 + radius1 * radius1;
            float distance = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);

            // Intersection occured.
            if (distance <= radiusSum)
            {
                return true;
            }

            // No intersection.
            return false;
        }

        /// <summary>
        ///     Gets all Pawns inside the supplied radius. If any.
        /// </summary>
        /// <param name="center">Radius center.</param>
        /// <param name="map">Map to look in.</param>
        /// <param name="radius">The radius from the center.</param>
        /// <param name="targetPredicate">Optional predicate on each candidate.</param>
        /// <returns>Matching Pawns inside the Radius.</returns>
        public static IEnumerable<Pawn> GetPawnsInsideRadius(LocalTargetInfo center, Map map, float radius,
                                                             Predicate<Pawn> targetPredicate)
        {
            //With no predicate, just grab everything.
            if (targetPredicate == null)
            {
                targetPredicate = thing => true;
            }

            foreach (Pawn pawn in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
            {
                if (CircleIntersectionTest(pawn.Position.x, pawn.Position.y, 1f, center.Cell.x, center.Cell.y, radius)
                 && targetPredicate(pawn))
                {
                    yield return pawn;
                }
            }
        }

        public static bool Prefix(PawnRenderer __instance,
                                  ref Vector3 rootLoc,
                                  float angle,
                                  bool renderBody,
                                  Rot4 bodyFacing,
                                  Rot4 headFacing,
                                  RotDrawMode bodyDrawType,
                                  bool portrait,
                                  bool headStump)
        {
            // Pawn pawn = (Pawn)PawnFieldInfo?.GetValue(__instance);
            PawnGraphicSet graphics = __instance.graphics;

            Pawn pawn = graphics.pawn;

            if (!pawn.RaceProps.Humanlike && !Controller.settings.UsePaws)
            {
                return true;

            }

            if (!graphics.AllResolved)
            {
                graphics.ResolveAllGraphics();
            }

            CompFace compFace = pawn.GetCompFace();
            bool hasFace = compFace != null;

            // Let vanilla do the job if no FacePawn or pawn not a teenager or any other known mod accessing the renderer
            if (hasFace)
            {
                if (compFace.IsChild || compFace.Deactivated)
                {
                    return true;
                }

            }

            CompBodyAnimator compAnim = pawn.GetCompAnim();
            bool showFeet = compAnim != null && Controller.settings.UseFeet;

            // No face, no animator, return
            if (!hasFace && compAnim == null)
            {
                return true;
            }

            PawnWoundDrawer woundDrawer = (PawnWoundDrawer)WoundOverlayFieldInfo?.GetValue(__instance);

            // if (Patches2.Plants)
            // {
            //     if (pawn.Spawned)
            //     {
            //         Plant plant = (pawn.Position + IntVec3.South).GetPlant(pawn.Map);
            //         if (plant != null && Patches2.plantMoved.Contains(plant))
            //         {
            //             rootLoc.y = plant.DrawPos.y - (Patches2.steps / 2);
            //         }
            //     }
            // }

            // Try to move the y position behind while another pawn is standing near
            if (compAnim != null && (!portrait && pawn.Spawned && !compAnim.IsRider))
            {
                RecalcRootLocY(ref rootLoc, pawn, compAnim);
            }

            Vector3 baseDrawLoc = rootLoc;
            // Let's save the basic location for later
            Vector3 footPos = baseDrawLoc;



            // No face => must be animal, simplify it
            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Quaternion bodyQuat = quat;
            Quaternion footQuat = bodyQuat;


            if (MainTabWindow_WalkAnimator.IsOpen)
            {
                bodyFacing = MainTabWindow_BaseAnimator.BodyRot;
                headFacing = MainTabWindow_BaseAnimator.HeadRot;
            }

            compFace?.TickDrawers(bodyFacing, headFacing, graphics);

            compAnim?.TickDrawers(bodyFacing, graphics);

            // Use the basic quat
            Quaternion headQuat = bodyQuat;

            // Rotate head if possible and wobble around
            if (!portrait || MainTabWindow_WalkAnimator.IsOpen)
            {
                if (showFeet)
                {
                    compAnim.ApplyBodyWobble(ref baseDrawLoc, ref footPos, ref bodyQuat);
                }

                // Reset the quat as it has been changed
                headQuat = bodyQuat;
                compFace?.ApplyHeadRotation(renderBody, ref headQuat);
            }

            // Regular FacePawn rendering 14+ years

            // Render body
            // if (renderBody)
            compAnim?.DrawBody(baseDrawLoc, bodyQuat, bodyDrawType, woundDrawer, renderBody, portrait);

            Vector3 bodyPos = baseDrawLoc;
            Vector3 headPos = baseDrawLoc;
            if (bodyFacing == Rot4.North)
            {
                headPos.y += YOffset_Shell;
                bodyPos.y += YOffset_Head;
            }
            else
            {
                headPos.y += YOffset_Head;
                bodyPos.y += YOffset_Shell;
            }


            if (graphics.headGraphic != null)
            {
                // Rendererd pawn faces

                Vector3 offsetAt = !hasFace
                                   ? __instance.BaseHeadOffsetAt(bodyFacing)
                                   : compFace.BaseHeadOffsetAt(portrait);

                Vector3 b = bodyQuat * offsetAt;
                Vector3 headDrawLoc = headPos + b;

                if (!hasFace)
                {
                    Material material = graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
                    if (material != null)
                    {
                        Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                        GenDraw.DrawMeshNowOrLater(mesh2, headDrawLoc, quat, material, portrait);
                    }
                }
                else
                {
                    compFace.DrawBasicHead(out bool headDrawn, bodyDrawType, portrait, headStump, headDrawLoc, headQuat);
                    if (headDrawn)
                    {
                        if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
                        {
                            if (compFace.Props.hasWrinkles)
                            {
                                Vector3 wrinkleLoc = headDrawLoc;
                                wrinkleLoc.y += YOffset_Wrinkles;
                                compFace.DrawWrinkles(bodyDrawType, wrinkleLoc, headQuat, portrait);
                            }

                            if (compFace.Props.hasEyes)
                            {
                                Vector3 eyeLoc = headDrawLoc;
                                eyeLoc.y += YOffset_Eyes;

                                compFace.DrawNaturalEyes(eyeLoc, portrait, headQuat);

                                Vector3 browLoc = headDrawLoc;
                                browLoc.y += YOffset_Brows;
                                // the brow above
                                compFace.DrawBrows(browLoc, headQuat, portrait);

                                // and now the added eye parts
                                Vector3 unnaturalEyeLoc = headDrawLoc;
                                unnaturalEyeLoc.y += YOffset_UnnaturalEyes;
                                compFace.DrawUnnaturalEyeParts(unnaturalEyeLoc, headQuat, portrait);
                            }

                            // Portrait obviously ignores the y offset, thus render the beard after the body apparel (again)
                            if (compFace.Props.hasBeard)
                            {
                                Vector3 beardLoc = headDrawLoc;
                                Vector3 tacheLoc = headDrawLoc;

                                beardLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Beard : YOffset_Beard;
                                tacheLoc.y += headFacing == Rot4.North ? -YOffset_Head - YOffset_Tache : YOffset_Tache;

                                compFace.DrawBeardAndTache(beardLoc, tacheLoc, portrait, headQuat);
                            }

                            if (compFace.Props.hasMouth)
                            {
                                Vector3 mouthLoc = headDrawLoc;
                                mouthLoc.y += YOffset_Mouth;
                                compFace.DrawNaturalMouth(mouthLoc, portrait, headQuat);
                            }
                            // Deactivated, looks kinda crappy ATM
                            // if (pawn.Dead)
                            // {
                            // Material deadEyeMat = faceComp.DeadEyeMatAt(headFacing, bodyDrawType);
                            // if (deadEyeMat != null)
                            // {
                            // GenDraw.DrawMeshNowOrLater(mesh2, locFacialY, headQuat, deadEyeMat, portrait);
                            // locFacialY.y += YOffsetInterval_OnFace;
                            // }

                            // }
                            // else
                        }
                    }

                }


                if (!headStump)
                {
                    Vector3 overHead = baseDrawLoc + b;
                    overHead.y += YOffset_OnHead;

                    Vector3 hairLoc = overHead;
                    Vector3 headgearLoc = overHead;
                    Vector3 hatInFrontOfFace = baseDrawLoc + b;


                    hairLoc.y += YOffset_HairOnHead;
                    headgearLoc.y += YOffset_GearOnHead;
                    hatInFrontOfFace.y += ((!(headFacing == Rot4.North)) ? YOffset_PostHead : YOffset_Behind);

                    compFace?.DrawHairAndHeadGear(hairLoc, headgearLoc,
                                                 bodyDrawType,
                                                 portrait,
                                                 renderBody,
                                                 headQuat, hatInFrontOfFace);

                    compFace?.DrawAlienHeadAddons(headPos, portrait, headQuat, overHead);
                }

            }

            if (!portrait)
            {

                DrawEquipmentMethodInfo?.Invoke(__instance, new object[] { baseDrawLoc });
            }

            if (!portrait)
            {
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    foreach (Apparel ap in wornApparel)
                    {
                        DrawPos_Patch.offset = baseDrawLoc;
                        DrawPos_Patch.offsetEnabled = true;
                        ap.DrawWornExtras();
                        DrawPos_Patch.offsetEnabled = false;
                    }
                }

                Vector3 bodyLoc = baseDrawLoc;
                bodyLoc.y += YOffset_Status;

                PawnHeadOverlays headOverlays = (PawnHeadOverlays)PawnHeadOverlaysFieldInfo?.GetValue(__instance);
                if (headOverlays != null)
                {
                    compFace?.DrawHeadOverlays(headOverlays, bodyLoc, headQuat);
                }
            }


            compAnim?.DrawApparel(bodyQuat, bodyPos, portrait, renderBody);

            compAnim?.DrawAlienBodyAddons(bodyQuat, bodyPos, portrait, renderBody, bodyFacing);

            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0
             && graphics.packGraphic != null)
            {
                Mesh mesh = graphics.nakedGraphic.MeshAt(bodyFacing);
                Graphics.DrawMesh(mesh, bodyPos, quat, graphics.packGraphic.MatAt(bodyFacing), 0);
            }

            // No wobble for equipment, looks funnier - nah!
            // Vector3 equipPos = rootLoc;
            // equipPos.y = drawPos.y;

            //compAnim.DrawEquipment(drawPos, portrait);



            bool showHands = Controller.settings.UseHands;
            Vector3 handPos = bodyPos;
            if (renderBody || Controller.settings.IgnoreRenderBody)
            {
                if (showHands)
                {
                    // Reset the position for the hands
                    handPos.y = baseDrawLoc.y;
                    compAnim?.DrawHands(bodyQuat, handPos, portrait);
                }

                if (showFeet)
                {
                    compAnim.DrawFeet(bodyQuat, footQuat, footPos, portrait);
                }
            }

            return false;
        }
    }
}
    #endregion
   