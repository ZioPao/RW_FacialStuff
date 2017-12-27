﻿namespace FacialStuff
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FacialStuff.Components;

    using JetBrains.Annotations;

    using RimWorld;

    using Verse;
    using Verse.AI;

    public static class PawnExtensions
    {
        public static bool GetCompFace([NotNull] this Pawn pawn, [NotNull] out CompFace compFace)
        {
            compFace = pawn.GetComp<CompFace>();
            return compFace != null;
        }
        public static bool GetCompAnim([NotNull] this Pawn pawn, [NotNull] out CompBodyAnimator compAnim)
        {
            compAnim = pawn.GetComp<CompBodyAnimator>();
            return compAnim != null;
        }

        public static bool GetPawnFace([NotNull] this Pawn pawn, [NotNull] out PawnFace pawnFace)
        {
            pawnFace = null;

            if (!pawn.GetCompFace(out CompFace compFace))
            {
                return false;
            }

            PawnFace face = compFace.PawnFace;
            if (face != null)
            {
                pawnFace = face;
                return true;
            }

            return false;
        }

        public static bool HasCompFace([CanBeNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompFace));
        }

        public static bool HasCompAnimator([CanBeNull] this Pawn pawn)
        {
            return pawn.def.HasComp(typeof(CompBodyAnimator));
        }

        public static bool HasPawnFace([NotNull] this Pawn pawn)
        {
            if (pawn.GetCompFace(out CompFace compFace))
            {
                PawnFace face = compFace.PawnFace;
                return face != null;
            }

            return false;
        }

        public static bool Fleeing(this Pawn pawn)
        {
            Job job = pawn.CurJob;
            return pawn.MentalStateDef == MentalStateDefOf.PanicFlee
                   || (job != null && (job.def == JobDefOf.Flee || job.def == JobDefOf.FleeAndCower));
        }


        public static void CheckForAddedOrMissingParts(this Pawn pawn)
        {
            if (!Controller.settings.ShowExtraParts)
            {
                return;
            }

            // no head => no face
            if (!pawn.health.hediffSet.HasHead)
            {
                return;
            }
            // Reset the stats
            if (pawn.GetCompFace(out CompFace face))
            {
                face.bodyStat.eyeLeft = PartStatus.Natural;
                face.bodyStat.eyeRight = PartStatus.Natural;
                face.bodyStat.jaw = PartStatus.Natural;
            }
            if (pawn.GetCompAnim(out CompBodyAnimator anim))
            {
                anim.bodyStat.handLeft = PartStatus.Natural;
                anim.bodyStat.handRight = PartStatus.Natural;
                anim.bodyStat.footLeft = PartStatus.Natural;
                anim.bodyStat.footRight = PartStatus.Natural;
            }

            List<BodyPartRecord> allParts = pawn?.RaceProps?.body?.AllParts;
            if (allParts.NullOrEmpty())
            {
                return;
            }
            List<BodyPartRecord> body = allParts;
            List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;

            if (hediffs.NullOrEmpty() || body.NullOrEmpty())
            {
                // || hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.Part.def == BodyPartDefOf.Head))
                return;
            }

            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null && diff.def == HediffDefOf.MissingBodyPart))
            {
                CheckPart(body, diff, face, anim);
            }
            foreach (Hediff diff in hediffs.Where(diff => diff?.def?.defName != null && diff.def.addedPartProps != null))
            {
                CheckPart(body, diff, face, anim);
            }
        }

        private static void CheckPart(List<BodyPartRecord> body, Hediff hediff, [CanBeNull] CompFace face,
                                      [CanBeNull] CompBodyAnimator anim)
        {
            if (body.NullOrEmpty() || hediff.def == null)
            {
                return;
            }


            BodyPartRecord leftEye = body.Find(x => x.def == BodyPartDefOf.LeftEye);
            BodyPartRecord rightEye = body.Find(x => x.def == BodyPartDefOf.RightEye);
            BodyPartRecord jaw = body.Find(x => x.def == BodyPartDefOf.Jaw);



            BodyPartRecord leftArm = body.Find(x => x.def == BodyPartDefOf.LeftArm);
            BodyPartRecord rightArm = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("RightShoulder"));
            BodyPartRecord leftHand = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("LeftShoulder"));
            BodyPartRecord rightHand = body.Find(x => x.def == BodyPartDefOf.RightHand);
            BodyPartRecord leftLeg = body.Find(x => x.def == BodyPartDefOf.LeftLeg);
            BodyPartRecord rightLeg = body.Find(x => x.def == BodyPartDefOf.RightLeg);
            BodyPartRecord leftFoot = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("LeftFoot"));
            BodyPartRecord rightFoot = body.Find(x => x.def == DefDatabase<BodyPartDef>.GetNamed("RightFoot"));


            // Missing parts firs, hands and feet can be replaced by arms/legs
            if (hediff.def == HediffDefOf.MissingBodyPart)
            {
                if (face != null && face.Props.hasEyes)
                {
                    if (leftEye != null && hediff.Part == leftEye)
                    {
                        face.bodyStat.eyeLeft = PartStatus.Missing;
                        face.texPathEyeLeft = face.EyeTexPath("Missing", Side.Left);
                    }

                    // ReSharper disable once InvertIf
                    if (rightEye != null && hediff.Part == rightEye)
                    {
                        face.bodyStat.eyeRight = PartStatus.Missing;
                        face.texPathEyeRight = face.EyeTexPath("Missing", Side.Right);
                    }
                }

                if (anim != null && anim.Props.bipedWithHands)
                {
                    if (hediff.Part == leftHand)
                    {
                        anim.bodyStat.handLeft = PartStatus.Missing;
                    }
                    if (hediff.Part == rightHand)
                    {
                        anim.bodyStat.handRight = PartStatus.Missing;
                    }
                    if (hediff.Part == leftFoot)
                    {
                        anim.bodyStat.footLeft = PartStatus.Missing;
                    }
                    if (hediff.Part == rightFoot)
                    {
                        anim.bodyStat.footRight = PartStatus.Missing;
                    }
                }
            }

            AddedBodyPartProps addedPartProps = hediff.def?.addedPartProps;
            if (addedPartProps != null)
            {
                if (hediff.def?.defName != null && hediff.Part != null)
                {
                    if (face != null)
                    {
                        if (face.Props.hasEyes)
                        {
                            if (hediff.Part == leftEye)
                            {
                                face.texPathEyeLeftPatch = "AddedParts/" + hediff.def.defName + "_Left" + "_"
                                                           + face.PawnCrownType;
                            }

                            if (hediff.Part == rightEye)
                            {
                                face.texPathEyeRightPatch = "AddedParts/" + hediff.def.defName + "_Right" + "_"
                                                            + face.PawnCrownType;
                            }
                        }
                        if (face.Props.hasMouth)
                        {
                            if (hediff.Part == jaw)
                            {
                                face.texPathJawAddedPart = "Mouth/Mouth_" + hediff.def.defName;
                            }
                        }
                    }


                    if (anim != null)
                    {
                        if (anim.Props.bipedWithHands)
                        {
                            if (hediff.Part == leftHand || hediff.Part == leftArm)
                            {
                                anim.bodyStat.handLeft = PartStatus.Artificial;
                            }
                            if (hediff.Part == rightHand || hediff.Part == rightArm)
                            {
                                anim.bodyStat.handRight = PartStatus.Artificial;
                            }
                        }
                        if (hediff.Part == leftFoot || hediff.Part == leftLeg)
                        {
                            anim.bodyStat.footLeft = PartStatus.Artificial;
                        }
                        if (hediff.Part == rightFoot || hediff.Part == rightLeg)
                        {
                            anim.bodyStat.footRight = PartStatus.Artificial;
                        }
                    }
                }
            }


        }

    }
}
