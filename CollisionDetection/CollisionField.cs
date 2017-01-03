using System.Collections.Generic;

namespace CollisionDetection
{
    /// <summary>
    /// Describes a collision field
    /// </summary>
    public class CollisionField
    {
        private int fieldId;
        private float x;
        private float y;
        private float z;

        private float size;

        private List<CollisionObject> collisionObjects;

        /// <summary>
        /// Unique collision field id
        /// </summary>
        internal int FieldId
        {
            get { return fieldId; }
        }

        /// <summary>
        /// Grid column index
        /// </summary>
        internal float X
        {
            get { return x; }
        }
        
        /// <summary>
        /// Grid row index
        /// </summary>
        internal float Y
        {
            get { return y; }
        }
        
        /// <summary>
        /// Grid depth index
        /// </summary>
        internal float Z
        {
            get { return z; }
        }
        
        /// <summary>
        /// Collidable objects in field
        /// </summary>
        public List<CollisionObject> CollisionObjects
        {
            get { return collisionObjects; }
            set { collisionObjects = value; }
        }

        /// <summary>
        /// Initzializes a new collision field
        /// </summary>
        /// <param name="fieldId">Unique field index</param>
        /// <param name="x">Grid column index</param>
        /// <param name="y">Grid row index</param>
        /// <param name="z">Grid depth index</param>
        /// <param name="size">Grid size</param>
        public CollisionField(int fieldId, float x, float y, float z, float size)
        {
            this.fieldId = fieldId;
            this.x = x;
            this.y = y;
            this.z = z;
            this.size = size;
            collisionObjects = new List<CollisionObject>();
        }

        /// <summary>
        /// Initzializes a new collision field
        /// </summary>
        /// <param name="fieldId">Unique field index</param>
        /// <param name="x">Grid column index</param>
        /// <param name="y">Grid row index</param>
        /// <param name="size">Grid size</param>
        public CollisionField(int fieldId, float x, float y, float size)
        {
            this.fieldId = fieldId;
            this.x = x;
            this.y = y;
            this.z = 0;
            this.size = size;
            collisionObjects = new List<CollisionObject>();
        }
    }
}
