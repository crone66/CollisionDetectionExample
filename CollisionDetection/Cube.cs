using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CollisionDetection
{
    internal class Cube
    {
        private VertexPositionColor[] vertices;       
        private int[] indices;
        private Color lineColor;

        internal VertexPositionColor[] Vertices
        {
            get
            {
                return vertices;
            }

            set
            {
                vertices = value;
            }
        }

        internal int[] Indices
        {
            get
            {
                return indices;
            }

            set
            {
                indices = value;
            }
        }

        /// <summary>
        /// Represants a 3D Cube
        /// </summary>
        /// <param name="leftCorner">Position of the upper left corner</param>
        /// <param name="size">Cube size</param>
        /// <param name="lineColor">Cube line color</param>
        /// <param name="startIndices">start indices (can be used for connected cubes)</param>
        internal Cube(Vector3 leftCorner, float size,  Color lineColor, int startIndices = 0)
        {
            this.lineColor = lineColor;
            vertices = new VertexPositionColor[8];
            indices = new int[24];

            float x = leftCorner.X;
            float y = leftCorner.Y;
            float z = leftCorner.Z;

            int index = startIndices;
            vertices[0] = new VertexPositionColor(new Vector3(x, y, z), lineColor);
            indices[0] = index;

            vertices[1] = new VertexPositionColor(new Vector3(x + size, y, z), lineColor);
            indices[1]=index + 1;

            vertices[2] = new VertexPositionColor(new Vector3(x, y + size, z), lineColor);
            indices[2]=index;
            indices[3]=index + 2;

            vertices[3] = new VertexPositionColor(new Vector3(x + size, y + size, z), lineColor);
            indices[4]=index + 1;
            indices[5]=index + 3;

            indices[6]=index + 2;
            indices[7]=index + 3;

            vertices[4] = new VertexPositionColor(new Vector3(x, y, z + size), lineColor);
            indices[8]=index;
            indices[9]=index + 4;

            vertices[5] = new VertexPositionColor(new Vector3(x + size, y, z + size), lineColor);
            indices[10]=index + 4;
            indices[11]=index + 5;
            indices[12]=index + 1;
            indices[13]=index + 5;

            vertices[6] = new VertexPositionColor(new Vector3(x, y + size, z + size), lineColor);
            indices[14]=index + 4;
            indices[15]=index + 6;

            indices[16]=index + 2;
            indices[17]=index + 6;

            vertices[7] = new VertexPositionColor(new Vector3(x + size, y + size, z + size), lineColor);

            indices[18]=index + 6;
            indices[19]=index + 7;

            indices[20]=index + 5;
            indices[21]=index + 7;

            indices[22]=index + 3;
            indices[23]=index + 7;
        }
    }
}
