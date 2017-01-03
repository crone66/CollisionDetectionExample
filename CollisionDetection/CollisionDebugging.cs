using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace CollisionDetection
{
    public class CollisionDebugging
    {
        private CollisionFieldManager collisionFieldManager;
        private GraphicsDevice device;
        private BasicEffect effect;
        private Color LineColor;
        private Color LastHighlightColor;
        private ConcurrentQueue<HighlightField> highlightFields;
        private bool enabled;

        private List<VertexPositionColor> vertices;
        private List<int> indices;
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        private bool highlightFieldWithObjects;

        private List<CollisionField> durationLink;
        private List<float> durationTable;

        public bool Enabled
        {
            get
            {
                return enabled;
            }

            set
            {
                enabled = value;
            }
        }

        /// <summary>
        /// Initzializes a collision debugging object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="device">Graphics device</param>
        /// <param name="LineColor">Line color of cubes</param>
        public CollisionDebugging(CollisionFieldManager collisionFieldManager, GraphicsDevice device, Color LineColor)
        {
            this.LineColor = LineColor;
            this.device = device;
            this.collisionFieldManager = collisionFieldManager;

            Enabled = true;
            highlightFieldWithObjects = false;
            effect = new BasicEffect(device);
            highlightFields = new ConcurrentQueue<HighlightField>();
            vertices = new List<VertexPositionColor>();
            indices = new List<int>();
            durationLink = new List<CollisionField>();
            durationTable = new List<float>();

            CreateLines();
        }

        /// <summary>
        /// Creates grid
        /// </summary>
        private void CreateLines()
        {
            int x = collisionFieldManager.CollisionFields.GetUpperBound(0) + 1;
            int y = collisionFieldManager.CollisionFields.GetUpperBound(1) + 1;
            int z = collisionFieldManager.CollisionFields.GetUpperBound(2) + 1;
            int index = 0;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    for (int k = 0; k < z; k++)
                    {
                        float vx = collisionFieldManager.CollisionFields[i, j, k].X;
                        float vy = collisionFieldManager.CollisionFields[i, j, k].Y;
                        float vz = collisionFieldManager.CollisionFields[i, j, k].Z;

                        Cube cube = new Cube(new Vector3(vx, vy, vz), collisionFieldManager.FieldSize, LineColor, index);
                        vertices.AddRange(cube.Vertices);
                        indices.AddRange(cube.Indices);
                        index += 8;
                    }
                }
            }

            vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }

        /// <summary>
        /// Highlights collision field with collidable objects for a given duration
        /// </summary>
        /// <param name="color">Highlight color</param>
        public void ToggleHighlightFieldsWithObjects(Color color)
        {
            device.SetVertexBuffer(null);
            if (!highlightFieldWithObjects)
            {
                LastHighlightColor = color;
                for (int i = 0; i < collisionFieldManager.CollisionFieldsWithObjects.Count; i++)
                {
                    CollisionField field = collisionFieldManager.CollisionFieldsWithObjects[i];
                    int index = field.FieldId * 8;
                    for (int j = 0; j < 8; j++)
                    {
                        vertices[index + j] = new VertexPositionColor(vertices[index + j].Position, color);
                    }
                }
            }
            else
            {
                int icount = collisionFieldManager.CollisionFields.GetUpperBound(0) + 1;
                int jcount = collisionFieldManager.CollisionFields.GetUpperBound(1) + 1;
                int kcount = collisionFieldManager.CollisionFields.GetUpperBound(2) + 1;
                for (int i = 0; i < icount; i++)
                {
                    for (int j = 0; j < jcount; j++)
                    {
                        for (int k = 0; k < kcount; k++)
                        {
                            int id = collisionFieldManager.CollisionFields[i, j, k].FieldId * 8;
                            for (int l = 0; l < 8; l++)
                            {
                                vertices[id + l] = new VertexPositionColor(vertices[id + l].Position, LineColor);
                            }
                        }
                    }
                }
            }

            highlightFieldWithObjects = !highlightFieldWithObjects;
            vertexBuffer.SetData(vertices.ToArray());

            for (int i = 0; i < durationTable.Count; i++)
            {
                durationTable[i] = 0.01f;
            }
        }

        /// <summary>
        /// Highlights a given collision field
        /// </summary>
        /// <param name="field">Collision field to highlight</param>
        /// <param name="color">Highlight color</param>
        /// <param name="duration">Highlight duration (0 = infinity)</param>
        public void AddHighlightToQueue(CollisionField field, Color color, float duration = 0)
        {
            highlightFields.Enqueue(new HighlightField(field, color, duration));
        }

        /// <summary>
        /// Highlights fields with collision
        /// </summary>
        private void HighlightFieldWithCollision()
        {
            do
            {
                try
                {
                    HighlightField hfield;
                    if (highlightFields.TryDequeue(out hfield))
                    {
                        int index = -1;
                        if ((index = durationLink.IndexOf(hfield.Field)) > -1)
                        {
                            if (durationTable[index] < hfield.Duration)
                                durationTable[index] = hfield.Duration;
                        }
                        else
                        {
                            device.SetVertexBuffer(null);
                            durationLink.Add(hfield.Field);
                            durationTable.Add(hfield.Duration);

                            int id = hfield.Field.FieldId * 8;
                            for (int j = 0; j < 8; j++)
                            {
                                vertices[id + j] = new VertexPositionColor(vertices[id + j].Position, hfield.Color);
                            }
                            vertexBuffer.SetData(vertices.ToArray());
                        }
                    }
                }
                catch
                {
                }
            } while (highlightFields.Count > 0);
        }

        /// <summary>
        /// Draws grid
        /// </summary>
        /// <param name="view">Camera view matix</param>
        /// <param name="projection">Camera projection matrix</param>
        public void Draw(Matrix view, Matrix projection)
        {
            if (Enabled)
            {
                device.BlendState = BlendState.Opaque;
                device.DepthStencilState = DepthStencilState.Default;

                device.SetVertexBuffer(vertexBuffer);
                device.Indices = indexBuffer;
                effect.VertexColorEnabled = true;
                effect.View = view;
                effect.Projection = projection;

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    //device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, vertices.Count, 0, indices.Count / 2);
                    device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, indices.Count / 2);
                }
            }
        }

        /// <summary>
        /// Updates highlighted field
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            if (enabled)
            {
                HighlightFieldWithCollision();
                float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                for (int i = 0; i < durationLink.Count; i++)
                {
                    if (durationTable[i] > 0)
                    {
                        durationTable[i] -= ms;
                        if (durationTable[i] < 0)
                        {
                            int id = durationLink[i].FieldId * 8;
                            for (int j = 0; j < 8; j++)
                            {
                                vertices[id + j] = new VertexPositionColor(vertices[id + j].Position, highlightFieldWithObjects ? LastHighlightColor : LineColor);
                            }

                            durationLink.RemoveAt(i);
                            durationTable.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}
