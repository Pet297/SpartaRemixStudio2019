using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpartaRemixStudio2019
{
    public class TextureInfo
    {
        //Obsahuje UV Transformaci
        public int TextureIndex { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Matrix4 RenderMatrix { get; set; }
        public Matrix4 RenderColorMatrix { get; private set; }
        public Vector4 RenderColorOffset { get; private set; }

        public float RenderUVX0 { get; set; }
        public float RenderUVX1 { get; set; }
        public float RenderUVY0 { get; set; }
        public float RenderUVY1 { get; set; }

        public TextureInfo(int texture, float Width, float Height)
        {
            TextureIndex = texture;
            this.Width = Width;
            this.Height = Height;

            RenderMatrix = new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            RenderColorMatrix = new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            RenderColorOffset = new Vector4(0, 0, 0, 0);

            RenderUVX0 = 0;
            RenderUVX1 = 1;
            RenderUVY0 = 0;
            RenderUVY1 = 1;
        }
        public void Reset()
        {
            TextureIndex = 0;
            this.Width = Width;
            this.Height = Height;

            RenderMatrix = new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            RenderColorMatrix = new Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            RenderColorOffset = new Vector4(0, 0, 0, 0);

            RenderUVX0 = 0;
            RenderUVX1 = 1;
            RenderUVY0 = 0;
            RenderUVY1 = 1;
        }

        public void PrependTransformColor(Matrix4 M, Vector4 O)
        {
            RenderColorOffset = RenderColorMatrix * O + RenderColorOffset;
            RenderColorMatrix = RenderColorMatrix * M;
        }
        public void AppendTransformColor(Matrix4 M, Vector4 O)
        {
            RenderColorOffset = M * RenderColorOffset + O;
            RenderColorMatrix = M * RenderColorMatrix;
        }
        public void FlipX()
        {
            float p = RenderUVX0;
            RenderUVX0 = RenderUVX1;
            RenderUVX1 = p;
        }
        public void FlipY()
        {
            float p = RenderUVY0;
            RenderUVY0 = RenderUVY1;
            RenderUVY1 = p;
        }

        public bool IncludesPreprocessing
        {
            get => IncludesPreUV || IncludesPreColor || IncludesPreTrans;
        }
        public bool IncludesPreUV
        {
            get => !(RenderUVX0 == 0 && RenderUVX1 == 1 && RenderUVY0 == 0 && RenderUVY1 == 1);
        }
        public bool IncludesPreColor
        {
            get => !(RenderColorMatrix == Matrix4.Identity && RenderColorOffset == Vector4.Zero);
        }
        public bool IncludesPreTrans
        {
            get => !(RenderMatrix == Matrix4.Identity);
        }
    }
}
