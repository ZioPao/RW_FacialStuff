using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FacialStuff.GraphicsFS;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;


//This isn't hair related. Jesus

namespace FacialStuff.HairCut
{
    // ReSharper disable once InconsistentNaming
    [StaticConstructorOnStartup]
    public static class PawnElementsDB
    {
        const string STR_MergedHair = "/Textures/MergedHair/";
        private static readonly Dictionary<GraphicRequest, Graphic> AllGraphics =
            new Dictionary<GraphicRequest, Graphic>();
        private static readonly List<HairCutPawn> PawnHairCache = new List<HairCutPawn>();
        private static Texture2D _maskTexFrontBack;
        private static Texture2D _maskTexSide;
        private static string _mergedHairPath;


        private static string MergedHairPath
        {
            get
            {
                if (!_mergedHairPath.NullOrEmpty())
                {
                    return _mergedHairPath;
                }

                ModMetaData mod = ModLister.AllInstalledMods.FirstOrDefault(
                    x => { return x?.Name != null && x.Active && x.Name.StartsWith("Facial Stuff"); });
                if (mod != null)
                {
                    _mergedHairPath = mod.RootDir + STR_MergedHair;
                }

                return _mergedHairPath;
            }
        }

        public static Graphic Get<T>(string path, Shader shader, Vector2 drawSize, Color color, HeadCoverage coverage)
            where T : Graphic, new()
        {
                        // Added second 'color' to get a separate graphic
            GraphicRequest req = new GraphicRequest(typeof(T), path, shader, drawSize, color, color, null, 0, new List<ShaderParameter>());
            return GetInner<T>(req, coverage);
        }

        public static HairCutPawn GetHairCache(Pawn pawn)
        {
            foreach (HairCutPawn c in PawnHairCache)
            {
                if (c.Pawn == pawn)
                {
                    return c;
                }
            }

            HairCutPawn n = new HairCutPawn { Pawn = pawn };
            PawnHairCache.Add(n);
            return n;
        }

        private static void CutOutHair([NotNull] ref Texture2D hairTex, Texture2D maskTex)
        {
            for (int x = 0; x < hairTex.width; x++)
            {
                for (int y = 0; y < hairTex.height; y++)
                {
                    Color maskColor = maskTex.GetPixel(x, y);
                    Color hairColor = hairTex.GetPixel(x, y);

                    Color finalColor1 = hairColor * maskColor;

                    hairTex.SetPixel(x, y, finalColor1);
                }
            }

            hairTex.Apply();
        }
		
		
		//I think this method is basically the source of everything. And its variable names make no sense
        private static T GetInner<T>(GraphicRequest req, HeadCoverage coverage)
            where T : Graphic, new()
        {
			
			//I think it should be generic. It's the only method that runs Init
			
			
			//it's trying to find the graphics related to that part of the pawn.
			// it won't find anything unless we're using mergedHair. what the hell
            string oldPath = req.path;
            string name = Path.GetFileNameWithoutExtension(oldPath);

            string newPath = MergedHairPath + name + "_" + coverage;
            req.path = newPath;

			//res is the key that we're searching. If it finds it, it returns graphic... Casted as whatever it should be
            if (AllGraphics.TryGetValue(req, out Graphic graphic))
            {
                return (T)graphic;
            }
			
			// if there are no active graphics, we have to make everything from here
            graphic = Activator.CreateInstance<T>();

            // Check if textures already present and readable, else create
            if (ContentFinder<Texture2D>.Get(req.path + "_north", false) != null)
            {
				//Init whatever is in req to graphic
				graphic.Init(req);
            }
			else
			{
				req.path = oldPath;
				//Init whatever is in req to graphic
				graphic.Init(req);
				
				//Now graphic has basically every info that was inside req. We start forming our textures from now on

				//Creates a var for every element of this pawn
				//Mostly generating materials
				Texture2D tempTextureFront = graphic.MatSouth.mainTexture as Texture2D;
				Texture2D tempTextureSide = graphic.MatEast.mainTexture as Texture2D;
				Texture2D tempTextureBack = graphic.MatNorth.mainTexture as Texture2D;

				//No idea what it's trying to do here
				Texture2D tempTextureSide2 = (graphic as Graphic_Multi)?.MatWest.mainTexture as Texture2D;

				tempTextureFront = FaceTextures.MakeReadable(tempTextureFront);
				tempTextureSide = FaceTextures.MakeReadable(tempTextureSide);
				tempTextureBack = FaceTextures.MakeReadable(tempTextureBack);

				tempTextureSide2 = FaceTextures.MakeReadable(tempTextureSide2);

				// new mask textures 
				if (coverage == HeadCoverage.UpperHead)
				{
					_maskTexFrontBack = FaceTextures.MaskTexUppherheadFrontBack;
					_maskTexSide = FaceTextures.MaskTexUpperheadSide;
				}
				else
				{
					_maskTexFrontBack = FaceTextures.MaskTexFullheadFrontBack;
					_maskTexSide = FaceTextures.MaskTexFullheadSide;
				}


				// I really doubt it's hair related. I think it's just applying some stuff 
				CutOutHair(ref tempTextureFront, _maskTexFrontBack);
				CutOutHair(ref tempTextureSide, _maskTexSide);
				CutOutHair(ref tempTextureBack, _maskTexFrontBack);
				CutOutHair(ref tempTextureSide2, _maskTexSide);

				req.path = newPath;

				//Optimization and stuff for textures
				tempTextureFront.Compress(true);
				tempTextureSide.Compress(true);
				tempTextureBack.Compress(true);
				tempTextureSide2.Compress(true);

				tempTextureFront.mipMapBias = 0.5f;
				tempTextureSide.mipMapBias = 0.5f;
				tempTextureBack.mipMapBias = 0.5f;
				tempTextureSide2.mipMapBias = 0.5f;

				tempTextureFront.Apply(false, true);
				tempTextureSide.Apply(false, true);
				tempTextureBack.Apply(false, true);
				tempTextureSide2.Apply(false, true);

				graphic.MatSouth.mainTexture = tempTextureFront;
				graphic.MatEast.mainTexture = tempTextureSide;
				graphic.MatNorth.mainTexture = tempTextureBack;
				((Graphic_Multi) graphic).MatWest.mainTexture = tempTextureSide2;


			}

		//adds all to the dict
		AllGraphics.Add(req, graphic);

		//Returns the single element that we've just created
		return (T)graphic;
        }

        public static void ExportHairCut(HairDef hairDef,string name)
        {
            string path = MergedHairPath + name;
            if (!name.NullOrEmpty() && !File.Exists(path + "_Upperhead_south.png"))
            {
                LongEventHandler.ExecuteWhenFinished(
                    delegate
                    {
                        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, Color.white);

                        SetTempTextures(graphic, out Texture2D temptexturefront, out Texture2D temptextureside, out Texture2D temptextureback, out Texture2D temptextureside2);

                        string upperPath = path + "_Upperhead";

                        _maskTexFrontBack = FaceTextures.MaskTexUppherheadFrontBack;
                        _maskTexSide = FaceTextures.MaskTexUpperheadSide;

                        CutOutHair(upperPath, ref temptexturefront, ref temptextureside, ref temptextureback, ref temptextureside2);

                        SetTempTextures(graphic, out temptexturefront, out temptextureside, out temptextureback, out temptextureside2);

                        string fullPath = path + "_Fullhead";

                        _maskTexFrontBack = FaceTextures.MaskTexFullheadFrontBack;
                        _maskTexSide = FaceTextures.MaskTexFullheadSide;

                        CutOutHair(fullPath, ref temptexturefront, ref temptextureside, ref temptextureback, ref temptextureside2);
                    });
            }
        }

        private static void SetTempTextures(Graphic graphic, out Texture2D temptexturefront, out Texture2D temptextureside, out Texture2D temptextureback, out Texture2D temptextureside2)
        {
            temptexturefront = graphic.MatSouth.mainTexture as Texture2D;
            temptextureside = graphic.MatEast.mainTexture as Texture2D;
            temptextureback = graphic.MatNorth.mainTexture as Texture2D;
            temptextureside2 = (graphic as Graphic_Multi)?.MatWest.mainTexture as Texture2D;

            temptexturefront = FaceTextures.MakeReadable(temptexturefront);
            temptextureside = FaceTextures.MakeReadable(temptextureside);
            temptextureback = FaceTextures.MakeReadable(temptextureback);
            temptextureside2 = FaceTextures.MakeReadable(temptextureside2);
        }

        private static void CutOutHair(string exportPath, ref Texture2D temptexturefront, ref Texture2D temptextureside, ref Texture2D temptextureback, ref Texture2D temptextureside2)
        {
            CutOutHair(ref temptexturefront, _maskTexFrontBack);

            CutOutHair(ref temptextureside, _maskTexSide);

            CutOutHair(ref temptextureback, _maskTexFrontBack);
            CutOutHair(ref temptextureside2, _maskTexSide);



            byte[] bytes = temptexturefront.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_south.png", bytes);
            byte[] bytes2 = temptextureside.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_east.png", bytes2);
            byte[] bytes3 = temptextureback.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_north.png", bytes3);
            byte[] bytes4 = temptextureside2.EncodeToPNG();
            File.WriteAllBytes(exportPath + "_west.png", bytes2);
        }
    }
}