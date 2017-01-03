using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CollisionDetection
{
    public class CollisionFieldManager
    {
        private bool activated = false;

        private Vector3 upperLeftCorner;
        private List<CollisionObject> collisionObjects = new List<CollisionObject>();
        private List<CollisionField> collisionFieldsWithObjects = new List<CollisionField>();
        private bool updateObjects = true;
        private bool doCollision = false;
        private int fieldCount = 0;
        private CollisionField[,,] collisionFields;
        private float fieldSize;
        private bool useDepth = false;

        /// <summary>
        /// Indicates whenther the collision detection is on or off
        /// </summary>
        public bool Activated
        {
            get { return activated; }
            set
            {
                activated = value;
                if (value)
                {
                    SetupFieldCollisions();
                }
            }
        }

        /// <summary>
        /// Size per Field
        /// </summary>
        internal float FieldSize
        {
            get { return fieldSize; }
        }

        /// <summary>
        /// List of all collision objects
        /// </summary>
        internal List<CollisionObject> CollisionObjects
        {
            get
            {
                return collisionObjects;
            }

            set
            {
                collisionObjects = value;
            }
        }

        /// <summary>
        /// List of field containing collision objects
        /// </summary>
        internal List<CollisionField> CollisionFieldsWithObjects
        {
            get
            {
                return collisionFieldsWithObjects;
            }
        }

        /// <summary>
        /// Indicates whenther to update collision objects or not
        /// </summary>
        public bool UpdateObjects
        {
            get
            {
                return updateObjects;
            }

            set
            {
                updateObjects = value;
            }
        }

        /// <summary>
        /// Indicates whenther to do a collision check or not
        /// </summary>
        public bool DoCollision
        {
            get
            {
                return doCollision;
            }

            set
            {
                doCollision = value;
            }
        }

        /// <summary>
        /// Number of fields
        /// </summary>
        public int FieldCount
        {
            get
            {
                return fieldCount;
            }
        }

        /// <summary>
        /// Number of fields containing collision objects
        /// </summary>
        public int FieldsWithObjectsCount
        {
            get
            {
                return CollisionFieldsWithObjects.Count;
            }
        }

        /// <summary>
        /// 3D array of collision fields
        /// </summary>
        internal CollisionField[,,] CollisionFields
        {
            get
            {
                return collisionFields;
            }

            set
            {
                collisionFields = value;
            }
        }

        /// <summary>
        /// Initzializes a collision grid
        /// </summary>
        /// <param name="upperLeftCorner">Grid position of the upper left corner</param>
        /// <param name="worldWidth">Grid width in units</param>
        /// <param name="worldHeight">Grid height in units</param>
        /// <param name="fieldSize">Grid cube size</param>
        /// <param name="forceSymmetric">Indicates whenther to force a grid symmetry or not</param>
        public CollisionFieldManager(Vector3 upperLeftCorner, float worldWidth, float worldHeight, float fieldSize, bool forceSymmetric = false)
        {
            ChangeField(upperLeftCorner, worldWidth, worldHeight, fieldSize, forceSymmetric);
        }

        /// <summary>
        /// Initzializes a collision grid
        /// </summary>
        /// <param name="upperLeftCorner">Grid position of the upper left corner</param>
        /// <param name="worldWidth">Grid width in units</param>
        /// <param name="worldHeight">Grid height in units</param>
        /// <param name="worldDepth">Grid depth in units</param>
        /// <param name="fieldSize">Grid cube size</param>
        /// <param name="forceSymmetric">Indicates whenther to force a grid symmetry or not</param>
        public CollisionFieldManager(Vector3 upperLeftCorner, float worldWidth, float worldHeight, float worldDepth, float fieldSize, bool forceSymmetric = false)
        {
            ChangeField(upperLeftCorner, worldWidth, worldHeight, worldDepth, fieldSize, forceSymmetric);
        }

        /// <summary>
        /// Changes the collision grid size
        /// </summary>
        /// <param name="upperLeftCorner">Grid position of the upper left corner</param>
        /// <param name="worldWidth">Grid width in units</param>
        /// <param name="worldHeight">Grid height in units</param>
        /// <param name="fieldSize">Grid cube size</param>
        /// <param name="forceSymmetric">Indicates whenther to force a grid symmetry or not</param>
        private void ChangeField(Vector3 upperLeftCorner, float worldWidth, float worldHeight, float fieldSize, bool forceSymmetric = false)
        {
            this.upperLeftCorner = upperLeftCorner;
            this.fieldSize = fieldSize;

            int fieldCount = 0;
            if (forceSymmetric)
            {
                float worldSize = worldWidth >= worldHeight ? worldWidth : worldHeight;
                int RowCount = Convert.ToInt32(Math.Ceiling(worldSize / fieldSize));
                CollisionFields = new CollisionField[RowCount, RowCount, 1];
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < RowCount; j++)
                    {
                        CollisionFields[i, j, 0] = new CollisionField(fieldCount++, upperLeftCorner.X + (fieldSize * j), upperLeftCorner.Y + (fieldSize * i), fieldSize);
                    }
                }
            }
            else
            {
                int RowCount = Convert.ToInt32(Math.Ceiling(worldHeight / fieldSize));
                int ColumnCount = Convert.ToInt32(Math.Ceiling(worldWidth / fieldSize));
                CollisionFields = new CollisionField[RowCount, ColumnCount, 1];
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < RowCount; j++)
                    {
                        CollisionFields[i, j, 0] = new CollisionField(fieldCount++, upperLeftCorner.X + (fieldSize * j), upperLeftCorner.Y + (fieldSize * i), fieldSize);
                    }
                }
            }
            this.fieldCount = fieldCount;
            Activated = true;
        }

        /// <summary>
        /// Changes the collision grid size
        /// </summary>
        /// <param name="upperLeftCorner">Grid position of the upper left corner</param>
        /// <param name="worldWidth">Grid width in units</param>
        /// <param name="worldHeight">Grid height in units</param>
        /// <param name="worldDepth">Grid depth in units</param>
        /// <param name="fieldSize">Grid cube size</param>
        /// <param name="forceSymmetric">Indicates whenther to force a grid symmetry or not</param>
        private void ChangeField(Vector3 upperLeftCorner, float worldWidth, float worldHeight, float worldDepth, float fieldSize, bool forceSymmetric = false)
        {
            this.upperLeftCorner = upperLeftCorner;
            this.fieldSize = fieldSize;
            this.useDepth = true;

            int fieldCount = 0;
            if (forceSymmetric)
            {
                float worldSize = worldWidth >= worldHeight ? worldWidth : worldHeight;
                int RowCount = Convert.ToInt32(Math.Ceiling(worldSize / fieldSize));
                CollisionFields = new CollisionField[RowCount, RowCount, RowCount];
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < RowCount; j++)
                    {
                        for (int k = 0; k < RowCount; k++)
                        {
                            CollisionFields[i, j, k] = new CollisionField(fieldCount++, upperLeftCorner.X + (fieldSize * j), upperLeftCorner.Y + (fieldSize * i), upperLeftCorner.Z + (fieldSize * k), fieldSize);
                        }
                    }
                }
            }
            else
            {
                int RowCount = Convert.ToInt32(Math.Ceiling(worldHeight / fieldSize));
                int ColumnCount = Convert.ToInt32(Math.Ceiling(worldWidth / fieldSize));
                int DepthCount = Convert.ToInt32(Math.Ceiling(worldDepth / fieldSize));
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < RowCount; j++)
                    {
                        for (int k = 0; k < DepthCount; k++)
                        {
                            CollisionFields[i, j, k] = new CollisionField(fieldCount++, upperLeftCorner.X + (fieldSize * j), upperLeftCorner.Y + (fieldSize * i), upperLeftCorner.Z + (fieldSize * k), fieldSize);
                        }
                    }
                }
            }
            this.fieldCount = fieldCount;
        }

        /// <summary>
        /// Executes a CheckFieldCollision foreach collision object
        /// </summary>
        private void SetupFieldCollisions()
        {
            for (int i = 0; i < CollisionObjects.Count; i++)
            {
                CheckFieldCollision(CollisionObjects[i]);
            }
        }

        /// <summary>
        /// Checks if a collision object overlaps with a collision field and how long it takes to reach the next field
        /// </summary>
        /// <param name="collisionObject">Collision object</param>
        internal void CheckFieldCollision(CollisionObject collisionObject)
        {
            if (activated)
            {
                //Load Balancing (split on Threads)
                //Edit field
                //Edit minimumTime
                float x = collisionObject.Position.X - upperLeftCorner.X, y = collisionObject.Position.Y - upperLeftCorner.Y, z = collisionObject.Position.Z - upperLeftCorner.Z; //vectorPosition collisionObject.vectorPosition
                //float x = collisionObject.vectorPosition.X, y = collisionObject.vectorPosition.Y, z = collisionObject.vectorPosition.Z; //vectorPosition collisionObject.vectorPosition
                float xmin, xmax, ymin, ymax, zmin, zmax;
                float radius = collisionObject.Sphere.Radius;

                xmin = x - radius;
                xmax = x + radius;
                ymin = y - radius;
                ymax = y + radius;

                float _colDiff = x / fieldSize;
                float colDiff = _colDiff - (float)Math.Floor(_colDiff);

                float _rowDiff = y / fieldSize;
                float rowDiff = _rowDiff - (float)Math.Floor(_rowDiff);

                float colValue = colDiff > 0.5 ? colDiff : 1 - colDiff;
                float rowValue = rowDiff > 0.5 ? rowDiff : 1 - rowDiff;

                float depthValue = 0;
                float depthDiff = 0;
                if (useDepth)
                {
                    float _depthDiff = z / fieldSize;
                    depthDiff = _depthDiff / (float)Math.Floor(_depthDiff);
                    depthValue = depthDiff > 0.5 ? depthDiff : 1 - depthDiff;
                }

                float reqUnits = 0;
                if (colValue >= rowValue && colValue >= depthValue)
                {
                    if (colDiff >= 0.5)
                        reqUnits = ((1f - colDiff) * fieldSize);
                    else
                        reqUnits = (colDiff * fieldSize);
                }
                else if (rowValue >= colValue && rowValue >= depthValue)
                {
                    if (rowDiff >= 0.5)
                        reqUnits = ((1f - rowDiff) * fieldSize);
                    else
                        reqUnits = (rowDiff * fieldSize);
                }
                else
                {
                    if (depthDiff >= 0.5)
                        reqUnits = ((1f - depthDiff) * fieldSize);
                    else
                        reqUnits = (depthDiff * fieldSize);
                }

                collisionObject.RequiredFieldUnits = reqUnits;

                int colMin = Convert.ToInt32(Math.Floor(xmin / fieldSize));
                int colMax = Convert.ToInt32(Math.Floor(xmax / fieldSize)) + 1;

                int rowMin = Convert.ToInt32(Math.Floor(ymin / fieldSize));
                int rowMax = Convert.ToInt32(Math.Floor(ymax / fieldSize)) + 1;

                int depthMin = 0;
                int depthMax = 1;
                if (useDepth)
                {
                    zmin = z - radius;
                    zmax = z + radius;
                    depthMin = Convert.ToInt32(Math.Floor(zmin / fieldSize));
                    depthMax = Convert.ToInt32(Math.Floor(zmax / fieldSize)) + 1;
                }

                if ((collisionObject.LastMinRow != rowMin || collisionObject.LastMaxRow != rowMax ||
                    collisionObject.LastMinCol != colMin || collisionObject.LastMaxCol != colMax) ||
                    (useDepth &&
                    (collisionObject.LastMinDepth != depthMin || collisionObject.LastMaxDepth != depthMax)))
                {
                    //something Changed!
                    collisionObject.ClearFields();
                    for (int i = rowMin; i < rowMax; i++)
                    {
                        for (int j = colMin; j < colMax; j++)
                        {
                            for (int k = depthMin; k < depthMax; k++)
                            {
                                CollisionField field = CollisionFields[i, j, k];
                                collisionObject.InFields.Add(field);
                                AddCollisionObject(field, collisionObject);
                            }
                        }
                    }

                    if (collisionObject.InFields.Count <= 0)
                        throw new OutOfGridException(collisionObject);
                }
            }
        }

        /// <summary>
        /// Adds a collision object to a collision field
        /// </summary>
        /// <param name="field">Collision field</param>
        /// <param name="cObject">Collision object</param>
        private void AddCollisionObject(CollisionField field, CollisionObject cObject)
        {
            field.CollisionObjects.Add(cObject);
            if (field.CollisionObjects.Count == 1)
                CollisionFieldsWithObjects.Add(field);
        }

        /// <summary>
        /// Removes a collision object from a collision field
        /// </summary>
        /// <param name="field">Collision field</param>
        /// <param name="cObject">Collision object</param>
        internal void RemoveCollisionObject(CollisionField field, CollisionObject cObject)
        {
            field.CollisionObjects.Remove(cObject);
            if (field.CollisionObjects.Count == 0)
                CollisionFieldsWithObjects.Remove(field);
        }
    }
}
