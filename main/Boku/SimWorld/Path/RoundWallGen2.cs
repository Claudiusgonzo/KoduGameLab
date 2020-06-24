using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using KoiX;

namespace Boku.SimWorld.Path
{
    class RoundWallGen2 : HiWallGen
    {

        #region Public
        /// <summary>
        /// Initialize data tables. Could be loaded from xml.
        /// </summary>
        public RoundWallGen2()
        {
            widthTable = new float[]  { 0.38f, 0.70f, 0.70f, 0.69f, 0.71f, 0.92f, 1.00f, 0.92f, 0.71f, 0.71f, 0.71f };
            heightTable = new float[] { 1.92f, 1.71f, 1.71f, 1.72f, 1.70f, 1.38f, 1.00f, 0.62f, 0.30f, 0.30f, -1.0f };
            tex0Source = new float[]  { 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f, 0.00f };
            uvSource = new float[]    { 0.00f, 0.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f };
            centerHeight = 2.00f;

            Shininess = 0.0f;

            StretchUp = false;
            StretchUpEnd = false;

            uvXfm = new Vector4(1.0f, 0.1f, 1.0f, 0.1f);

            InitDelTables();

            diffTex0 = null;
            diffTex1 = null;
            normTex0 = null;
            normTex1 = null;

            BokuGame.Load(this);
        }

        /// <summary>
        /// Load up textures.
        /// </summary>
        /// <param name="graphics"></param>
        public override void LoadContent(bool immediate)
        {
            if (diffTex0 == null)
            {
                diffTex0 = KoiLibrary.LoadTexture2D(@"Textures\Terrain\GroundTextures\White128x128");
            }
            if (diffTex1 == null)
            {
                diffTex1 = KoiLibrary.LoadTexture2D(@"Textures\Terrain\GroundTextures\dirt_earth-n-moss_df_");
            }
            if (normTex0 == null)
            {
                normTex0 = KoiLibrary.LoadTexture2D(@"Textures\Terrain\GroundTextures\RIVROCK1_norm");
            }
            if (normTex1 == null)
            {
                normTex1 = KoiLibrary.LoadTexture2D(@"Textures\Terrain\GroundTextures\dirt_earth-n-moss_df_norm");
            }

            base.LoadContent(immediate);
        }

        public override void UnloadContent()
        {
            DeviceResetX.Release(ref diffTex0);
            DeviceResetX.Release(ref diffTex1);
            DeviceResetX.Release(ref normTex0);
            DeviceResetX.Release(ref normTex1);
            base.UnloadContent();
        }

        #endregion Public
    }
}
