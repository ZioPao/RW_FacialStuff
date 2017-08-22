﻿namespace FacialStuff.FaceStyling_Bench
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using FaceStyling;

    using FacialStuff.ColorPicker;
    using FacialStuff.Defs;
    using FacialStuff.Detouring;
    using FacialStuff.Enums;
    using FacialStuff.FaceStyling_Bench.UI.DTO;
    using FacialStuff.FaceStyling_Bench.UI.DTO.SelectionWidgetDTOs;
    using FacialStuff.FaceStyling_Bench.UI.Util;
    using FacialStuff.Genetics;
    using FacialStuff.Graphics_FS;
    using FacialStuff.Utilities;

    using RimWorld;

    using UnityEngine;

    using Verse;

    [StaticConstructorOnStartup]
    public class DialogFaceStyling : Window
    {
        #region Public Fields

        public static readonly Vector2 PortraitSize = new Vector2(192f, 192f);

        #endregion Public Fields

        #region Private Fields

        private static readonly Color ColorSwatchBorder = new Color(0.77255f, 0.77255f, 0.77255f);

        private static readonly Color ColorSwatchSelection = new Color(0.9098f, 0.9098f, 0.9098f);

        private static readonly int Columns;

        private static readonly float EntrySize;

        private static readonly List<BeardDef> FullBeardDefs;

        private static readonly float ListWidth;

        private static readonly List<BeardDef> LowerBeardDefs;

        // private static Texture2D _icon;
        private static readonly float MarginFS;

        private static readonly long MAX_AGE = 1000000000 * TICKS_PER_YEAR;

        // private FacePreset SelectedFacePreset
        // {
        // get { return _selFacePresetInt; }
        // set
        // {
        // CheckSelectedFacePresetHasName();
        // _selFacePresetInt = value;
        // }
        // }
        private static readonly List<MoustacheDef> MoustacheDefs;

        // private FacePreset _selFacePresetInt;
        private static readonly Texture2D NameBackground;

        private static readonly float PreviewSize;

        private static readonly long TICKS_PER_YEAR = 3600000L;

        private static readonly string Title;

        private static readonly float TitleHeight;

        private static readonly List<string> VanillaHairTags = new List<string> { "Urban", "Rural", "Punk", "Tribal" };

        private static List<BrowDef> browDefs;

        private ColorWrapper colourWrapper;

        private static List<EyeDef> eyeDefs;

        private CompFace faceComp;

        private bool hadSameBeardColor;

        private static List<HairDef> hairDefs;

        private bool hats;

        private BeardDef newBeard;

        private Color newBeardColor;

        private BrowDef newBrow;

        private EyeDef newEye;

        private HairDef newHair;

        private Color newHairColor;

        private float newMelanin;

        private MoustacheDef newMoustache;

        private BeardDef originalBeard;

        private Color originalBeardColor;

        private BrowDef originalBrow;

        private EyeDef originalEye;

        private HairDef originalHair;

        private Color originalHairColor;

        private float originalMelanin;

        private MoustacheDef originalMoustache;

        private static Pawn pawn;

        private Vector2 swatchSize = new Vector2(14, 14);

        private Vector2 swatchSpacing = new Vector2(20, 20);

        private DresserDTO dresserDto;

        private GenderTab genderTab;
        private BeardTab beardTab;

        private bool initialized = false;

        private long originalAgeBio;

        private long originalAgeChrono;

        private BodyType originalBodyType;

        private CrownType originalCrownType;

        private Gender originalGender;

        private string originalHeadGraphicPath;

        private bool rerenderPawn = false;

        private bool saveChangedOnExit = false;

        private Vector2 scrollPositionBeard1 = Vector2.zero;
        private Vector2 scrollPositionBeard2 = Vector2.zero;
        private Vector2 scrollPositionEye = Vector2.zero;
        private Vector2 scrollPositionBrow = Vector2.zero;
        private Vector2 scrollPositionHairFemale = Vector2.zero;
        private Vector2 scrollPositionHairMale = Vector2.zero;
        private Vector2 scrollPositionHairAny = Vector2.zero;

        private FaceStyleTab tab;

        private object heads;

        private bool reInit;

        #endregion Private Fields

        #region Public Constructors

        static DialogFaceStyling()
        {
            Title = "FacialStuffEditor.FaceStylerTitle".Translate();
            TitleHeight = 30f;
            PreviewSize = 250f;

            // _previewSize = 100f;

            // _icon = ContentFinder<Texture2D>.Get("ClothIcon");
            MarginFS = 6f;
            ListWidth = 450f;

            // _listWidth = 200f;
            Columns = 12;
            EntrySize = ListWidth / Columns;
            NameBackground = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0f, 0f, 0.3f));

            hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                x => x.hairTags.SharesElementWith(VanillaHairTags));
            eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading;
            FullBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType == BeardType.FullBeard)
                .ToList();
            LowerBeardDefs = DefDatabase<BeardDef>.AllDefsListForReading.Where(x => x.beardType != BeardType.FullBeard)
                .ToList();
            MoustacheDefs = DefDatabase<MoustacheDef>.AllDefsListForReading;

            browDefs = DefDatabase<BrowDef>.AllDefsListForReading;
            FullBeardDefs.SortBy(i => i.LabelCap);
            LowerBeardDefs.SortBy(i => i.LabelCap);
            MoustacheDefs.SortBy(i => i.LabelCap);
        }

        public DialogFaceStyling(Pawn p)
        {
            pawn = p;
            this.faceComp = pawn.TryGetComp<CompFace>();
            this.hats = Prefs.HatsOnlyOnMap;
            Prefs.HatsOnlyOnMap = true;
            this.hadSameBeardColor = this.faceComp.pawnFace.HasSameBeardColor;
            if (pawn.gender == Gender.Female)
            {
                this.genderTab = GenderTab.Female;

            }
            else
            {
                this.genderTab = GenderTab.Male;
            }

            if (this.faceComp.pawnFace.BeardDef == null)
            {
                this.faceComp.pawnFace.BeardDef = BeardDefOf.Beard_Shaved;
            }

            this.beardTab = this.faceComp.pawnFace.BeardDef.beardType == BeardType.FullBeard
                                ? BeardTab.FullBeards
                                : BeardTab.Combinable;


            this.originalHairColor = pawn.story.hairColor;
            this.newBeardColor = this.originalBeardColor = this.faceComp.pawnFace.BeardColor;
            this.newBeard = this.originalBeard = this.faceComp.pawnFace.BeardDef;
            this.newMoustache = this.originalMoustache = this.faceComp.pawnFace.MoustacheDef;
            this.newEye = this.originalEye = this.faceComp.pawnFace.EyeDef;
            this.newBrow = this.originalBrow = this.faceComp.pawnFace.BrowDef;

            this.newHairColor = pawn.story.hairColor;

            this.colourWrapper = new ColorWrapper(Color.cyan);

            this.newMelanin = this.originalMelanin = pawn.story.melanin;

            this.newHair = this.originalHair = pawn.story.hairDef;

            this.originalBodyType = pawn.story.bodyType;
            this.originalGender = pawn.gender;
            this.originalHeadGraphicPath = pawn.story.HeadGraphicPath;
            this.originalCrownType = pawn.story.crownType;

            this.originalAgeBio = pawn.ageTracker.AgeBiologicalTicks;
            this.originalAgeChrono = pawn.ageTracker.AgeChronologicalTicks;

            // this.absorbInputAroundWindow = false;
            this.closeOnClickedOutside = false;

            this.closeOnEscapeKey = true;
            this.doCloseButton = false;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;



        }

        #endregion Public Constructors

        #region Private Enums

        private enum FaceStyleTab : byte
        {
            Hair,

            Beard,

            Eye,

            Brow,

            TypeSelector
        }
        private enum GenderTab : byte
        {
            Male,
            Female, Any
        }
        private enum BeardTab : byte
        {
            Combinable,
            FullBeards
        }

        #endregion Private Enums

        #region Public Properties

        public override Vector2 InitialSize => new Vector2(
                    PreviewSize + MarginFS + ListWidth + 36f,
                    40f + PreviewSize * 2f + MarginFS * 3f + 38f + 36f + 80f);

        public BeardDef NewBeard
        {
            get => this.newBeard;

            set
            {
                this.newBeard = value;

                this.UpdatePawn(value);
                if (value.beardType == BeardType.FullBeard && !this.reInit)
                {
                    this.newMoustache = MoustacheDefOf.Shaved;
                    this.UpdatePawn(MoustacheDefOf.Shaved);

                }
            }
        }

        public Color NewBeardColor
        {
            get => this.newBeardColor;

            set
            {
                this.newBeardColor = value;
                this.UpdatePawn(this.NewBeard, value);
            }
        }

        public BrowDef NewBrow
        {
            get => this.newBrow;

            set
            {
                this.newBrow = value;
                this.UpdatePawn(value);
            }
        }

        public EyeDef NewEye
        {
            get => this.newEye;

            set
            {
                this.newEye = value;

                this.UpdatePawn(value);
            }
        }

        public HairDef NewHair
        {
            get => this.newHair;

            set
            {
                this.newHair = value;
                this.UpdatePawn(value);
            }
        }

        public Color NewHairColor
        {
            get => this.newHairColor;

            set
            {
                this.newHairColor = value;
                this.UpdatePawn(this.NewHair, value);

                if (this.faceComp != null && this.faceComp.pawnFace.HasSameBeardColor && !this.reInit)
                {
                    Color color = FacialGraphics.DarkerBeardColor(value);
                    this.UpdatePawn(this.NewBeard, color);

                }
            }
        }

        public float NewMelanin
        {
            get => this.newMelanin;

            set
            {
                this.newMelanin = value;
                this.UpdatePawn(value, PawnSkinColors.GetSkinColor(value));
            }
        }

        public MoustacheDef NewMoustache
        {
            get => this.newMoustache;

            set
            {
                this.newMoustache = value;
                this.UpdatePawn(value);

                if (this.newBeard.beardType == BeardType.FullBeard && !this.reInit)
                {
                    this.newBeard = PawnFaceChooser.RandomBeardDefFor(pawn, BeardType.LowerBeard);
                    this.UpdatePawn(this.newBeard);
                }
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static Rect AddPortraitWidget(float left, float top)
        {
            // Portrait
            Rect rect = new Rect(left, top, PortraitSize.x, PortraitSize.y);

            // Draw the pawn's portrait
            GUI.BeginGroup(rect);
            Vector2 size = new Vector2(128f, 180f);
            Rect position = new Rect(rect.width * 0.5f - size.x * 0.5f, 10f + rect.height * 0.5f - size.y * 0.5f, size.x, size.y);
            RenderTexture image = PortraitsCache.Get(pawn, size, new Vector3(0f, 0f, 0f), 1.3f);
            GUI.DrawTexture(position, image);
            GUI.EndGroup();

            GUI.color = Color.white;
            Widgets.DrawBox(rect, 1);

            return rect;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(MarginFS, 0f, inRect.width - MarginFS, TitleHeight);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect, Title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            try
            {
                if (this.rerenderPawn)
                {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(pawn);
                    this.rerenderPawn = false;
                }

                if (!this.initialized)
                {
                    this.dresserDto = new DresserDTO(pawn);
                    this.dresserDto.SetUpdatePawnListeners(this.UpdatePawn);

                }
            }
            catch (Exception ex)
            {
            }


            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "FacialStuffEditor.Hair".Translate(),
                delegate { this.tab = FaceStyleTab.Hair; },
                this.tab == FaceStyleTab.Hair);
            list.Add(item);

            if (pawn.gender == Gender.Male)
            {
                TabRecord item2 = new TabRecord(
                    "FacialStuffEditor.Beard".Translate(),
                    delegate { this.tab = FaceStyleTab.Beard; },
                    this.tab == FaceStyleTab.Beard);
                list.Add(item2);
            }

            TabRecord item3 = new TabRecord(
                "FacialStuffEditor.Eye".Translate(),
                delegate { this.tab = FaceStyleTab.Eye; },
                this.tab == FaceStyleTab.Eye);
            list.Add(item3);

            TabRecord item4 = new TabRecord(
                "FacialStuffEditor.Brow".Translate(),
                delegate { this.tab = FaceStyleTab.Brow; },
                this.tab == FaceStyleTab.Brow);
            list.Add(item4);

            if (Controller.settings.ShowBodyChange)
            {
                TabRecord item5 = new TabRecord(
                    "FacialStuffEditor.TypeSelector".Translate(),
                    delegate { this.tab = FaceStyleTab.TypeSelector; },
                    this.tab == FaceStyleTab.TypeSelector);
                list.Add(item5);
            }

            Rect rect3 = new Rect(0f, TitleHeight + TabDrawer.TabHeight + MarginFS, inRect.width, inRect.height - TitleHeight - 25f - MarginFS * 2 - TabDrawer.TabHeight - MarginFS);

            TabDrawer.DrawTabs(rect3, list);
            this.DrawUi(rect3);


            DialogUtility.DoNextBackButtons(
                inRect,
                "FacialStuffEditor.Accept".Translate(),
                delegate
                    {
                        this.saveChangedOnExit = true;
                        this.Close();
                    },
                delegate
                    {
                        while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
                        {
                        }

                        this.ResetPawnFace();
                    });
        }

        public override void PostOpen()
        {
            switch (pawn.gender)
            {
                case Gender.Male:
                    hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags)
                             && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                    eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    break;

                case Gender.Female:
                    hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                        x => x.hairTags.SharesElementWith(VanillaHairTags)
                             && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                    eyeDefs = DefDatabase<EyeDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    break;
            }
            hairDefs.SortBy(i => i.LabelCap);
            eyeDefs.SortBy(i => i.LabelCap);
            browDefs.SortBy(i => i.LabelCap);
        }

        public override void PreClose()
        {
            Prefs.HatsOnlyOnMap = this.hats;

            if (!this.saveChangedOnExit)
            {
                this.ResetPawnFace();
            }

            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker), false))
            {
            }
        }

        #endregion Public Methods

        #region Private Methods

        private bool AddLongInput(float labelLeft, float top, float inputLeft, float inputWidth, string label, ref long value, long maxValue, long factor = 1)
        {
            string stringValue;
            if (value == -1)
            {
                stringValue = string.Empty;
            }
            else
            {
                stringValue = (value / factor).ToString();
            }

            string result = WidgetUtil.AddNumberTextInput(labelLeft, top, inputLeft, inputWidth, label, stringValue);
            try
            {
                if (result.Length == 0)
                {
                    value = -1;
                    return true;
                }
                else if (result.Length > 0 && !result.Equals(stringValue))
                {
                    value = long.Parse(result);
                    if (value < 0)
                    {
                        value = 0;
                    }
                    else
                    {
                        value *= factor;
                        if (value > maxValue || value < 0)
                            value = maxValue;
                    }

                    return true;
                }
            }
            catch { }
            return false;
        }

        private Graphic_Multi_NaturalHeadParts BeardGraphic(BeardDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                string path = def.texPath + "_" + this.faceComp.PawnCrownType + "_" + this.faceComp.PawnHeadType;
                if (def == BeardDefOf.Beard_Shaved)
                {
                    path = def.texPath;
                }

                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic_Multi_NaturalHeadParts BrowGraphic(BrowDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                             this.faceComp.BrowTexPath(def),
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private void DrawBeardColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewBeardColor)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }

                this.colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this.colourWrapper,
                        delegate
                            {
                                if (this.faceComp.pawnFace.HasSameBeardColor)
                                {
                                    this.NewHairColor = this.colourWrapper.Color;
                                }

                                this.NewBeardColor = this.colourWrapper.Color;
                            },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawBeardPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "FacialStuffEditor.FullBeards".Translate(),
                delegate
                    {
                        hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                        hairDefs.SortBy(i => i.LabelCap);
                        this.beardTab = BeardTab.FullBeards;
                    },
                this.beardTab == BeardTab.FullBeards);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                "FacialStuffEditor.Combinable".Translate(),
                delegate
                    {
                        hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                        hairDefs.SortBy(i => i.LabelCap);
                        this.beardTab = BeardTab.Combinable;
                    },
                this.beardTab == BeardTab.Combinable);
            list.Add(item2);

            TabDrawer.DrawTabs(rect, list);


            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            int rowsBeard = Mathf.CeilToInt(FullBeardDefs.Count / (float)thisColumns);
            int rowsTache = Mathf.CeilToInt(MoustacheDefs.Count / (float)thisColumns);
            int rowsLowerBeards = Mathf.CeilToInt(LowerBeardDefs.Count / (float)thisColumns);

            int allRows;

            if (this.beardTab == BeardTab.Combinable)
            {
                allRows = rowsTache + rowsLowerBeards;
            }
            else
            {
                allRows = rowsBeard;
            }

            rect3.height = allRows * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * 2, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * allRows;
            }

            switch (this.beardTab)
            {
                case BeardTab.Combinable:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionBeard1, rect3);
                    break;
                case BeardTab.FullBeards:
                    Widgets.BeginScrollView(rect2, ref this.scrollPositionBeard2, rect3);
                    break;
            }
            GUI.BeginGroup(rect3);

            float curY = 0f;
            float thisY = 0f;
            if (this.beardTab == BeardTab.Combinable)
            {
                for (int i = 0; i < MoustacheDefs.Count; i++)
                {
                    int yPos = i / thisColumns;
                    int xPos = i % thisColumns;
                    Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                    this.DrawMoustachePickerCell(MoustacheDefs[i], rect4.ContractedBy(3f));
                    thisY = rect4.yMax;
                }

                curY = thisY;
                for (int i = 0; i < LowerBeardDefs.Count; i++)
                {
                    int num2 = i / thisColumns;
                    int num3 = i % thisColumns;
                    Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y + curY, vector.x, vector.y);
                    this.DrawBeardPickerCell(LowerBeardDefs[i], rect4.ContractedBy(3f));
                }
            }

            if (this.beardTab == BeardTab.FullBeards)
            {
                for (int i = 0; i < FullBeardDefs.Count; i++)
                {
                    int num2 = i / thisColumns;
                    int num3 = i % thisColumns;
                    Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                    this.DrawBeardPickerCell(FullBeardDefs[i], rect4.ContractedBy(3f));
                }
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBeardPickerCell(BeardDef beard, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = beard.LabelCap;
            float offset = ((rect.width / 2) - rect.height) / 3;
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if (beard == this.NewBeard)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                }
                else
                {
                    if (beard == this.originalBeard)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }

            if (this.newMoustache != MoustacheDefOf.Shaved)
            {
                // if (beard.beardType == BeardType.FullBeard)
                // {
                // Widgets.DrawBoxSolid(rect, new Color(0.8f, 0f, 0f, 0.3f));
                // }
                {
                    // else
                    if (this.NewBeard == BeardDefOf.Beard_Shaved || this.NewMoustache != MoustacheDefOf.Shaved)
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.29f, 0.7f, 0.8f, 0.3f));
                    }
                    else
                    {
                        Widgets.DrawBoxSolid(rect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
                    }
                }
            }

            // Get the offset, cause width != 2 * height
            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);

            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);
            GUI.color = this.faceComp.pawnFace.HasSameBeardColor ? pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(rect1, this.BeardGraphic(beard).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.BeardGraphic(beard).MatSide.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewBeard = beard;
                this.DoColorWindowBeard();
            }
        }

        private void DoColorWindowBeard()
        {
            while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
            {
            }
            {
                this.colourWrapper.Color = this.faceComp.pawnFace.HasSameBeardColor ? this.NewHairColor : this.NewBeardColor;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this.colourWrapper,
                        delegate
                            {
                                if (this.faceComp.pawnFace.HasSameBeardColor)
                                {
                                    this.NewHairColor = this.colourWrapper.Color;
                                }

                                this.NewBeardColor = this.colourWrapper.Color;
                            },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawBrowPicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;


            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(browDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / Columns;
                vector.y -= 16f / Columns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this.scrollPositionBrow, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < browDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawBrowPickerCell(browDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawBrowPickerCell(BrowDef brow, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = brow.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (brow == this.NewBrow)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (brow == this.originalBrow)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.color = Color.black;
            GUI.DrawTexture(rect, this.BrowGraphic(brow).MatFront.mainTexture);
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewBrow = brow;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawEyePicker(Rect rect)
        {
            // 12 columns as base
            int divider = 3;
            int iconSides = 1;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect2 = rect.ContractedBy(1f);
            Rect rect3 = rect2;
            int num = Mathf.CeilToInt(eyeDefs.Count / (float)thisColumns);

            rect3.height = num * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect2.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * num;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            Widgets.BeginScrollView(rect2, ref this.scrollPositionEye, rect3);
            GUI.BeginGroup(rect3);

            for (int i = 0; i < eyeDefs.Count; i++)
            {
                int num2 = i / thisColumns;
                int num3 = i % thisColumns;
                Rect rect4 = new Rect(num3 * vector.x, num2 * vector.y, vector.x, vector.y);
                this.DrawEyePickerCell(eyeDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DrawEyePickerCell(EyeDef eye, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = eye.LabelCap;
            Widgets.DrawHighlightIfMouseover(rect);
            if (eye == this.NewEye)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }
            else
            {
                if (eye == this.originalEye)
                {
                    Widgets.DrawAltRect(rect);
                    text += "\n(original)";
                }
            }

            GUI.DrawTexture(rect, this.RightEyeGraphic(eye).MatFront.mainTexture);
            GUI.DrawTexture(rect, this.LeftEyeGraphic(eye).MatFront.mainTexture);


            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewEye = eye;
                Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker));
            }
        }

        private void DrawHairColorPickerCell(Color color, Rect rect, string colorName)
        {
            Widgets.DrawBoxSolid(rect, color);
            string text = colorName;
            Widgets.DrawHighlightIfMouseover(rect);
            if (color == this.NewHairColor)
            {
                Widgets.DrawHighlightSelected(rect);
                text += "\n(selected)";
            }

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }

                this.colourWrapper.Color = color;
                Find.WindowStack.Add(
                    new Dialog_ColorPicker(
                        this.colourWrapper,
                        delegate { this.NewHairColor = this.colourWrapper.Color; },
                        false,
                        true)
                    {
                        initialPosition = new Vector2(this.windowRect.xMax + MarginFS, this.windowRect.yMin)
                    });
            }
        }

        private void DrawHairPicker(Rect rect)
        {
            List<TabRecord> list = new List<TabRecord>();

            TabRecord item = new TabRecord(
                "Female".Translate(),
                delegate
                    {
                        hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually));
                        hairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.Female;
                    },
                this.genderTab == GenderTab.Female);
            list.Add(item);

            TabRecord item2 = new TabRecord(
                "Male".Translate(),
                delegate
                    {
                        hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags)
                                 && (x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually));
                        hairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.Male;
                    },
                this.genderTab == GenderTab.Male);
            list.Add(item2);

            TabRecord item3 = new TabRecord(
                "FacialStuffEditor.Any".Translate(),
                delegate
                    {
                        hairDefs = DefDatabase<HairDef>.AllDefsListForReading.FindAll(
                            x => x.hairTags.SharesElementWith(VanillaHairTags) && x.hairGender == HairGender.Any);
                        hairDefs.SortBy(i => i.LabelCap);
                        this.genderTab = GenderTab.Any;
                    },
                this.genderTab == GenderTab.Any);

            list.Add(item3);

            TabDrawer.DrawTabs(rect, list);

            // 12 columns as base
            int divider = 3;
            int iconSides = 2;
            int thisColumns = Columns / divider / iconSides;
            float thisEntrySize = EntrySize * divider;

            Rect rect3 = rect;
            int rowsCount = Mathf.CeilToInt(hairDefs.Count / (float)thisColumns);

            rect3.height = rowsCount * thisEntrySize;
            Vector2 vector = new Vector2(thisEntrySize * iconSides, thisEntrySize);
            if (rect3.height > rect.height)
            {
                vector.x -= 16f / thisColumns;
                vector.y -= 16f / thisColumns;
                rect3.width -= 16f;
                rect3.height = vector.y * rowsCount;
            }

            Rect selectHair = rect;
            selectHair.height = 30f;
            switch (this.genderTab)
            {
                case GenderTab.Male:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairMale, rect3);
                    break;
                case GenderTab.Female:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairFemale, rect3);
                    break;
                case GenderTab.Any:
                    Widgets.BeginScrollView(rect, ref this.scrollPositionHairAny, rect3);
                    break;
            }
            GUI.BeginGroup(rect3);

            for (int i = 0; i < hairDefs.Count; i++)
            {
                int yPos = i / thisColumns;
                int xPos = i % thisColumns;
                Rect rect4 = new Rect(xPos * vector.x, yPos * vector.y, vector.x, vector.y);
                this.DrawHairPickerCell(hairDefs[i], rect4.ContractedBy(3f));
            }

            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private static readonly Color DarkBackground = new Color(0.12f, 0.12f, 0.12f);

        private void DrawHairPickerCell(HairDef hair, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);

            string text = hair.LabelCap;

            // Get the offset, cause width != 2 * height
            float offset = ((rect.width / 2) - rect.height) / 3;

            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if (hair == this.NewHair)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                }
                else
                {
                    if (hair == this.originalHair)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }
            }


            // Rect rect3 = new Rect(rect2.xMax, rect.y, rect.height, rect.height);
            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);

            GUI.color = pawn.story.hairColor;
            GUI.DrawTexture(rect1, this.HairGraphic(hair).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.HairGraphic(hair).MatSide.mainTexture);

            GUI.color = Color.white;

            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewHair = hair;

                while (Find.WindowStack.TryRemove(typeof(Dialog_ColorPicker)))
                {
                }
                {
                    this.colourWrapper.Color = this.NewHairColor;

                    Find.WindowStack.Add(
                        new Dialog_ColorPicker(
                            this.colourWrapper,
                            delegate { this.NewHairColor = this.colourWrapper.Color; },
                            false,
                            true)
                        {
                            initialPosition = new Vector2(
                                    this.windowRect.xMax + MarginFS,
                                    this.windowRect.yMin)
                        });
                }
            }
        }

        // Blatantly stolen from Prepare Carefully
        private void DrawHumanlikeColorSelector(Rect melaninRect)
        {
            int currentSwatchIndex = PawnSkinColors_FS.GetSkinDataIndexOfMelanin(this.NewMelanin);

            Rect swatchRect = new Rect(melaninRect.x, melaninRect.y, this.swatchSize.x, this.swatchSize.y);

            // Draw the swatch selection boxes.
            int colorCount = PawnSkinColors_FS._SkinColors.Length;
            int clickedIndex = -1;
            for (int i = 0; i < colorCount; i++)
            {
                Color color = PawnSkinColors_FS._SkinColors[i].color;

                // If the swatch is selected, draw a heavier border around it.
                bool isThisSwatchSelected = i == currentSwatchIndex;
                if (isThisSwatchSelected)
                {
                    Rect selectionRect = new Rect(
                        swatchRect.x - 2,
                        swatchRect.y - 2,
                        this.swatchSize.x + 4,
                        this.swatchSize.y + 4);
                    GUI.color = ColorSwatchSelection;
                    GUI.DrawTexture(selectionRect, BaseContent.WhiteTex);
                }

                // Draw the border around the swatch.
                Rect borderRect = new Rect(swatchRect.x - 1, swatchRect.y - 1, this.swatchSize.x + 2, this.swatchSize.y + 2);
                GUI.color = ColorSwatchBorder;
                GUI.DrawTexture(borderRect, BaseContent.WhiteTex);

                // Draw the swatch itself.
                GUI.color = color;
                GUI.DrawTexture(swatchRect, BaseContent.WhiteTex);

                if (!isThisSwatchSelected)
                {
                    if (Widgets.ButtonInvisible(swatchRect))
                    {
                        clickedIndex = i;

                        // currentSwatchColor = color;
                    }
                }

                // Advance the swatch rect cursor position and wrap it if necessary.
                swatchRect.x += this.swatchSpacing.x;
                if (swatchRect.x >= melaninRect.width - this.swatchSize.x)
                {
                    swatchRect.y += this.swatchSpacing.y;
                    swatchRect.x = melaninRect.x;
                }
            }

            // Draw the slider
            WidgetUtil.AddSliderWidget(
                melaninRect.x,
                swatchRect.yMax,
                melaninRect.width,
                "FacialStuffEditor.SkinColor".Translate() + ":",
                this.dresserDto.SkinColorSliderDto);

            // Draw the current color box.
            GUI.color = Color.white;
            Rect currentColorRect = new Rect(melaninRect.x, swatchRect.y + 24f, 25, 25);
            if (swatchRect.x != melaninRect.x)
            {
                currentColorRect.y += this.swatchSpacing.y;
            }

            GUI.color = ColorSwatchBorder;
            GUI.DrawTexture(currentColorRect, BaseContent.WhiteTex);
            GUI.color = PawnSkinColors.GetSkinColor(this.NewMelanin);
            GUI.DrawTexture(currentColorRect.ContractedBy(1), BaseContent.WhiteTex);
            GUI.color = Color.white;

            // Figure out the lerp value so that we can draw the slider.
            float minValue = 0.00f;
            float maxValue = 0.99f;
            float t = PawnSkinColors_FS.GetRelativeLerpValue(this.NewMelanin);
            if (t < minValue)
            {
                t = minValue;
            }
            else if (t > maxValue)
            {
                t = maxValue;
            }

            if (clickedIndex != -1)
            {
                t = minValue;
            }

            // Draw the slider.
            float newValue = GUI.HorizontalSlider(
                new Rect(currentColorRect.x + 35, currentColorRect.y + 18, 136, 16),
                t,
                minValue,
                1);
            if (newValue < minValue)
            {
                newValue = minValue;
            }
            else if (newValue > maxValue)
            {
                newValue = maxValue;
            }

            GUI.color = Color.white;

            // If the user selected a new swatch or changed the lerp value, set a new color value.
            if (t != newValue || clickedIndex != -1)
            {
                if (clickedIndex != -1)
                {
                    currentSwatchIndex = clickedIndex;
                }

                float melaninLevel = PawnSkinColors_FS.GetValueFromRelativeLerp(currentSwatchIndex, newValue);
                this.NewMelanin = melaninLevel;
            }
        }

        private void DrawMoustachePickerCell(MoustacheDef moustache, Rect rect)
        {
            Widgets.DrawBoxSolid(rect, DarkBackground);
            string text = moustache.LabelCap;
            float offset = ((rect.width / 2) - rect.height) / 3;
            {
                // Highlight box
                Widgets.DrawHighlightIfMouseover(rect);
                if (moustache == this.NewMoustache)
                {
                    Widgets.DrawHighlightSelected(rect);
                    text += "\n(selected)";
                }
                else
                {
                    if (moustache == this.originalMoustache)
                    {
                        Widgets.DrawAltRect(rect);
                        text += "\n(original)";
                    }
                }

            }

            // if (newBeard.beardType == BeardType.FullBeard)
            // {
            // Widgets.DrawBoxSolid(rect, new Color(0.8f, 0f, 0f, 0.3f));
            // }
            {
                // else
                if (this.NewMoustache == MoustacheDefOf.Shaved)
                {
                    Widgets.DrawBoxSolid(rect, new Color(0.29f, 0.7f, 0.8f, 0.3f));
                }
                else
                {
                    Widgets.DrawBoxSolid(rect, new Color(0.8f, 0.8f, 0.8f, 0.3f));
                }
            }

            Rect rect1 = new Rect(rect.x + offset, rect.y, rect.height, rect.height);
            Rect rect2 = new Rect(rect1.xMax + offset, rect.y, rect.height, rect.height);

            GUI.color = pawn.story.SkinColor;
            GUI.DrawTexture(rect1, pawn.Drawer.renderer.graphics.headGraphic.MatFront.mainTexture);
            GUI.DrawTexture(rect2, pawn.Drawer.renderer.graphics.headGraphic.MatSide.mainTexture);
            GUI.color = this.faceComp.pawnFace.HasSameBeardColor ? pawn.story.hairColor : this.NewBeardColor;
            GUI.DrawTexture(rect1, this.MoustacheGraphic(moustache).MatFront.mainTexture);
            GUI.DrawTexture(rect2, this.MoustacheGraphic(moustache).MatSide.mainTexture);
            GUI.color = Color.white;


            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rect, text);
            if (Widgets.ButtonInvisible(rect))
            {
                this.NewMoustache = moustache;
                this.DoColorWindowBeard();
            }
        }

        private void DrawTypeSelector(Rect rect)
        {
            float editorLeft = rect.x;
            float editorTop = 30f + WidgetUtil.SelectionRowHeight;
            float editorWidth = 325f;


            float top = editorTop + 64f;

            WidgetUtil.AddSelectorWidget(
                editorLeft,
                top,
                editorWidth,
                "FacialStuffEditor.BodyType".Translate() + ":",
                this.dresserDto.BodyTypeSelectionDto);

            top += WidgetUtil.SelectionRowHeight + 20f;
            WidgetUtil.AddSelectorWidget(
                editorLeft,
                top,
                editorWidth,
                "FacialStuffEditor.HeadType".Translate() + ":",
                this.dresserDto.HeadTypeSelectionDto);

            top += WidgetUtil.SelectionRowHeight + 20f;


            if (Controller.settings.ShowGenderAgeChange)
            {
                GUI.Label(
                    new Rect(editorLeft, top, editorWidth, 64f),
                    "FacialStuffEditor.GenderChangeWarning".Translate());

                top += 64f + 20f;

                WidgetUtil.AddSelectorWidget(
                    editorLeft,
                    top,
                    editorWidth,
                    "FacialStuffEditor.Gender".Translate() + ":",
                    this.dresserDto.GenderSelectionDto);

                top += WidgetUtil.SelectionRowHeight + 5;
                long ageBio = pawn.ageTracker.AgeBiologicalTicks;
                if (this.AddLongInput(
                    editorLeft,
                    top,
                    120,
                    80,
                    "FacialStuffEditor.AgeBiological".Translate() + ":",
                    ref ageBio,
                    MAX_AGE,
                    TICKS_PER_YEAR))
                {
                    pawn.ageTracker.AgeBiologicalTicks = ageBio;
                    this.rerenderPawn = true;
                    if (ageBio > pawn.ageTracker.AgeChronologicalTicks)
                    {
                        pawn.ageTracker.AgeChronologicalTicks = ageBio;
                    }
                }

                top += WidgetUtil.SelectionRowHeight + 5;
                long ageChron = pawn.ageTracker.AgeChronologicalTicks;
                if (this.AddLongInput(
                    editorLeft,
                    top,
                    120,
                    80,
                    "FacialStuffEditor.AgeChronological".Translate() + ":",
                    ref ageChron,
                    MAX_AGE,
                    TICKS_PER_YEAR))
                {
                    pawn.ageTracker.AgeChronologicalTicks = ageChron;
                }
            }

            GUI.color = Color.white;

        }

        private void DrawUi(Rect parentRect)
        {
            GUI.BeginGroup(parentRect);
            string nameStringShort = pawn.NameStringShort;
            Vector2 vector = Text.CalcSize(nameStringShort);

            Rect pawnRect = AddPortraitWidget(0f, TitleHeight);
            Rect labelRect = new Rect(0f, pawnRect.yMax, vector.x, vector.y);
            labelRect = labelRect.CenteredOnXIn(pawnRect);

            Rect melaninRect = new Rect(2f, labelRect.yMax + MarginFS, PreviewSize - 5f, 65f);
            Rect selectionRect = new Rect(0f, melaninRect.yMax + MarginFS, PreviewSize, PreviewSize);
            Rect listRect = new Rect(PreviewSize + MarginFS, TitleHeight, ListWidth, parentRect.height - MarginFS * 2 - TitleHeight);

            GUI.DrawTexture(
                new Rect(labelRect.xMin - 3f, labelRect.yMin, labelRect.width + 6f, labelRect.height),
                NameBackground);
            Widgets.Label(labelRect, nameStringShort);

            // float spacing = 10f;
            this.DrawHumanlikeColorSelector(melaninRect);

            if (this.tab == FaceStyleTab.Hair || this.tab == FaceStyleTab.Beard)
            {
                listRect.yMin += TabDrawer.TabHeight;
            }

            Widgets.DrawMenuSection(listRect);

            Rect set = new Rect(selectionRect) { height = 30f, width = selectionRect.width / 2 - 10f };
            set.y += 10f;

            // if (Widgets.ButtonText(set, "SelectFacePreset".Translate(), true, false))
            // {
            // var list = new List<FloatMenuOption>();
            // foreach (var current in Current.Game.outfitDatabase.AllOutfits)
            // {
            // var localOut = current;
            // list.Add(new FloatMenuOption(localOut.label, delegate { SelectedFacePreset = localOut; },
            // MenuOptionPriority.Medium, null, null));
            // }
            // Find.WindowStack.Add(new FloatMenu(list));
            // }
            set.x = selectionRect.x;
            set.width = selectionRect.width - 5f;

            bool faceCompDrawMouth = this.faceComp.pawnFace.DrawMouth;
            Widgets.CheckboxLabeled(set, "FacialStuffEditor.DrawMouth".Translate(), ref faceCompDrawMouth);
            this.faceComp.pawnFace.DrawMouth = faceCompDrawMouth;
            if (pawn.gender == Gender.Male)
            {
                set.y += 24f;
                bool faceCompHasSameBeardColor = this.faceComp.pawnFace.HasSameBeardColor;
                Widgets.CheckboxLabeled(set, "FacialStuffEditor.SameColor".Translate(), ref faceCompHasSameBeardColor);
                this.faceComp.pawnFace.HasSameBeardColor = faceCompHasSameBeardColor;
            }

            if (GUI.changed)
            {
                if (this.faceComp.pawnFace.HasSameBeardColor)
                {
                    this.NewBeardColor = FacialGraphics.DarkerBeardColor(this.NewHairColor);
                }

            }

            set.width = selectionRect.width / 2 - 10f;

            set.y += 36f;
            set.x = selectionRect.x;


            if (this.tab == FaceStyleTab.Eye)
            {
                this.DrawEyePicker(listRect);
            }

            if (this.tab == FaceStyleTab.Brow)
            {
                if (pawn.gender == Gender.Female)
                {
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Female || x.hairGender == HairGender.FemaleUsually);
                    browDefs.SortBy(i => i.LabelCap);
                }

                if (pawn.gender == Gender.Male)
                {
                    browDefs = DefDatabase<BrowDef>.AllDefsListForReading.FindAll(
                        x => x.hairGender == HairGender.Male || x.hairGender == HairGender.MaleUsually);
                    browDefs.SortBy(i => i.LabelCap);
                }

                this.DrawBrowPicker(listRect);
            }


            if (this.tab == FaceStyleTab.Hair)
            {
                set.width = selectionRect.width / 7.5f - 10f;
                set.x = selectionRect.x;

                this.DrawHairColorPickerCell(this.faceComp.pawnFace.HairColorOrg, set, "FacialStuffEditor.Original".Translate());
                set.x += set.width * 1.5f + 10f;

                foreach (Color color in HairMelanin.NaturalHairColors)
                {
                    this.DrawHairColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }

                set.x = selectionRect.x;
                set.y += 36f;
                foreach (Color color in HairMelanin.ArtificialHairColors)
                {
                    this.DrawHairColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }

                if (true)
                {

                    set.x = selectionRect.x;
                    set.width = selectionRect.width;
                    set.y += 30f;

                    HairColorRequest hairColorRequest = new HairColorRequest(
                        this.faceComp.pawnFace.EuMelanin,
                        this.faceComp.pawnFace.PheoMelanin,
                        this.faceComp.pawnFace.Cuticula,
                        this.faceComp.pawnFace.Greyness);

                    Color hairColor = HairMelanin.GetHairColor(hairColorRequest);

                    this.faceComp.pawnFace.PheoMelanin = Widgets.HorizontalSlider(set, this.faceComp.pawnFace.PheoMelanin, 0f, 1f);
                    set.y += 30f;
                    this.faceComp.pawnFace.EuMelanin = Widgets.HorizontalSlider(set, this.faceComp.pawnFace.EuMelanin, 0f, 1f);
                    set.y += 30f;
                    this.faceComp.pawnFace.Cuticula = Widgets.HorizontalSlider(set, this.faceComp.pawnFace.Cuticula, 0.75f, 1.25f);
                    set.y += 30f;
                    this.faceComp.pawnFace.Greyness = Widgets.HorizontalSlider(set, this.faceComp.pawnFace.Greyness, 0f, 0.7f);

                    if (GUI.changed)
                    {
                        this.NewHairColor = hairColor;
                    }
                }
                this.DrawHairPicker(listRect);
            }

            if (this.tab == FaceStyleTab.Beard)
            {
                set.width = selectionRect.width / 7.5f - 10f;
                set.x = selectionRect.x;

                this.DrawBeardColorPickerCell(this.originalBeardColor, set, "FacialStuffEditor.Original".Translate());
                set.x += set.width * 1.5f + 10f;

                foreach (Color color in HairMelanin.NaturalHairColors)
                {
                    this.DrawBeardColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }

                set.x = selectionRect.x;
                set.y += 36f;
                foreach (Color color in HairMelanin.ArtificialHairColors)
                {
                    this.DrawBeardColorPickerCell(color, set, color.ToString());
                    set.x += set.width + 10f;
                }

                this.DrawBeardPicker(listRect);
            }

            if (this.tab == FaceStyleTab.TypeSelector)
            {
                this.DrawTypeSelector(listRect);
            }

            GUI.EndGroup();
        }

        private Graphic HairGraphic(HairDef def)
        {
            Graphic result;
            if (def.texPath != null)
            {
                result = GraphicDatabase.Get<Graphic_Multi>(
                    def.texPath,
                    ShaderDatabase.Cutout,
                    new Vector2(38f, 38f),
                    Color.white,
                    Color.white);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic_Multi_NaturalEyes LeftEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = this.faceComp.EyeTexPath(def.texPath, Side.Left);

                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private Graphic_Multi_NaturalHeadParts MoustacheGraphic(MoustacheDef def)
        {
            Graphic_Multi_NaturalHeadParts result;
            if (def.texPath != null)
            {
                string path = def == MoustacheDefOf.Shaved ? def.texPath : def.texPath + "_" + this.faceComp.PawnCrownType;

                result = GraphicDatabase.Get<Graphic_Multi_NaturalHeadParts>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalHeadParts;
            }
            else
            {
                result = null;
            }

            return result;
        }

        private void ResetPawnFace()
        {
            this.reInit = true;
            this.NewHairColor = this.originalHairColor;
            this.NewHair = this.originalHair;
            this.NewMelanin = this.originalMelanin;

            this.NewBeard = this.originalBeard;
            this.NewMoustache = this.originalMoustache;

            this.faceComp.pawnFace.HasSameBeardColor = this.hadSameBeardColor;
            this.NewBeardColor = this.originalBeardColor;

            this.NewEye = this.originalEye;
            this.NewBrow = this.originalBrow;

            pawn.story.bodyType = this.originalBodyType;
            pawn.gender = this.originalGender;
            typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, this.originalHeadGraphicPath);
            pawn.story.crownType = this.originalCrownType;
            pawn.ageTracker.AgeBiologicalTicks = this.originalAgeBio;
            pawn.ageTracker.AgeChronologicalTicks = this.originalAgeChrono;

            this.reInit = false;
            this.rerenderPawn = true;
        }

        private Graphic_Multi_NaturalEyes RightEyeGraphic(EyeDef def)
        {
            Graphic_Multi_NaturalEyes result;
            if (def.texPath != null)
            {
                string path = this.faceComp.EyeTexPath(def.texPath, Side.Right);

                // "Eyes/Eye_" + pawn.gender + faceComp.crownType + "_" + def.texPath   + "_Right";
                result = GraphicDatabase.Get<Graphic_Multi_NaturalEyes>(
                             path,
                             ShaderDatabase.Cutout,
                             new Vector2(38f, 38f),
                             Color.white,
                             Color.white) as Graphic_Multi_NaturalEyes;
            }
            else
            {
                result = null;
            }

            return result;
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        private void UpdatePawn(object newValue)
        {
            if (newValue != null)
            {
                if (newValue is BeardDef)
                {
                    this.faceComp.pawnFace.BeardDef = (BeardDef)newValue;
                }

                if (newValue is MoustacheDef)
                {
                    this.faceComp.pawnFace.MoustacheDef = (MoustacheDef)newValue;
                }

                if (newValue is EyeDef)
                {
                    this.faceComp.pawnFace.EyeDef = (EyeDef)newValue;
                }

                if (newValue is BrowDef)
                {
                    this.faceComp.pawnFace.BrowDef = (BrowDef)newValue;
                }

                if (newValue is HairDef)
                {
                    pawn.story.hairDef = (HairDef)newValue;
                }
            }

            this.rerenderPawn = true;
        }

        [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
        private void UpdatePawn(object type, object newValue)
        {
            if (type != null)
            {
                if (type is BeardDef)
                {
                    this.faceComp.pawnFace.BeardColor = (Color)newValue;
                }

                if (type is HairDef)
                {
                    pawn.story.hairColor = (Color)newValue;
                    this.faceComp.pawnFace.HairColor = (Color)newValue;
                }

                // skin color
                if (type is float && newValue is Color)
                {
                    pawn.story.melanin = (float)type;
                }
            }

            this.rerenderPawn = true;
        }

        private void UpdatePawn(object sender, object value, object value2 = null)
        {
            if (sender != null)
            {
                if (sender is BodyTypeSelectionDTO)
                {
                    pawn.story.bodyType = (BodyType)value;
                }
                else if (sender is GenderSelectionDTO)
                {
                    pawn.gender = (Gender)value;
                }
                else if (sender is HeadTypeSelectionDTO)
                {
                    typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn.story, value);
                    pawn.story.crownType = (CrownType)value2;
                }
                else if (sender is SliderWidgetDTO)
                {
                    pawn.story.melanin = (float)value;
                    this.NewMelanin = (float)value;
                }
            }

            this.rerenderPawn = true;
        }

        #endregion Private Methods

        #region Public Classes

        // private void CheckSelectedFacePresetHasName()
        // {
        // if (SelectedFacePreset != null && SelectedFacePreset.label.NullOrEmpty())
        // {
        // SelectedFacePreset.label = "Unnamed";
        // }
        // }
        #endregion Public Classes

    }
}