using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CollisionDetection
{
    public class CollisionObject
    {
        private CollisionFieldManager collisionFieldManager;
        private object baseObject;
        private Vector3 position;
        private BoundingSphere sphere;

        private List<CollisionField> inFields;
        private float requiredFieldUnits;
        private int lastMinRow;
        private int lastMaxRow;
        private int lastMinCol;
        private int lastMaxCol;
        private int lastMinDepth;
        private int lastMaxDepth;

        private bool virtualObject;
        private List<object> virtualCollisions;

        private bool disposing;

        /// <summary>
        /// Base object reference
        /// </summary>
        public object BaseObject
        {
            get
            {
                return baseObject;
            }

            set
            {
                baseObject = value;
            }
        }

        /// <summary>
        /// Current position of collision object
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }
        }

        /// <summary>
        /// Bounding sphere of collision object
        /// </summary>
        public BoundingSphere Sphere
        {
            get
            {
                return sphere;
            }
        }

        /// <summary>
        /// Indicates whenther a collision object is virtual or not (Can be used to catch all collisions without firing the OnCollision event)
        /// </summary>
        public bool VirtualObject
        {
            get
            {
                return virtualObject;
            }

            set
            {
                VirtualObject = value;
                if (value)
                {
                    virtualCollisions = new List<object>();
                }
            }
        }

        /// <summary>
        /// List of all collisions since last collision check
        /// </summary>
        public List<object> VirtualCollisions
        {
            get
            {
                return virtualCollisions;
            }

            internal set
            {
                virtualCollisions = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal float RequiredFieldUnits
        {
            get
            {
                return requiredFieldUnits;
            }

            set
            {
                requiredFieldUnits = value;
            }
        }

        /// <summary>
        /// Overlapping collision fields
        /// </summary>
        internal List<CollisionField> InFields
        {
            get
            {
                return inFields;
            }

            set
            {
                inFields = value;
            }
        }

        /// <summary>
        /// Minimum collision field row index
        /// </summary>
        internal int LastMinRow
        {
            get
            {
                return lastMinRow;
            }
        }

        /// <summary>
        /// Maximum collision field row index
        /// </summary>
        internal int LastMaxRow
        {
            get
            {
                return lastMaxRow;
            }
        }

        /// <summary>
        /// Minimum collision field column index
        /// </summary>
        internal int LastMinCol
        {
            get
            {
                return lastMinCol;
            }
        }

        /// <summary>
        /// Maximum collision field column index
        /// </summary>
        internal int LastMaxCol
        {
            get
            {
                return lastMaxCol;
            }
        }

        /// <summary>
        /// Minimum collision field depth index
        /// </summary>
        internal int LastMinDepth
        {
            get
            {
                return lastMinDepth;
            }
        }

        /// <summary>
        /// Maximum collision field depth index
        /// </summary>
        internal int LastMaxDepth
        {
            get
            {
                return lastMaxDepth;
            }
        }

        /// <summary>
        /// Initzializes a new collision object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="baseObject">Base object refernece</param>
        /// <param name="sphere">Bounding sphere of collision object</param>
        public CollisionObject(CollisionFieldManager collisionFieldManager, object baseObject, BoundingSphere sphere)
        {
            this.collisionFieldManager = collisionFieldManager;
            this.collisionFieldManager.CollisionObjects.Add(this);
            this.sphere = sphere;
            position = sphere.Center;
            inFields = new List<CollisionField>();
            virtualCollisions = new List<object>();
            BaseObject = baseObject;

            RequiredFieldUnits = 0;
            lastMinRow = -1;
            lastMaxRow = -1;
            lastMinCol = -1;
            lastMaxCol = -1;
            lastMinDepth = -1;
            lastMaxDepth = -1;
            virtualObject = false;
            disposing = false;
        }

        /// <summary>
        /// Initzializes a new collision object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="baseObject">Base object refernece</param>
        /// <param name="position">Position of object</param>
        /// <param name="radius">Radius of object</param>
        public CollisionObject(CollisionFieldManager collisionFieldManager, object baseObject, Vector3 position, float radius)
            :this(collisionFieldManager, baseObject, new BoundingSphere(position, radius))
        {
        }

        /// <summary>
        /// Initzializes a new collision object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="position">Position of object</param>
        /// <param name="radius">Radius of object</param>
        public CollisionObject(CollisionFieldManager collisionFieldManager, Vector3 position, float radius)
            : this(collisionFieldManager, null, new BoundingSphere(position, radius))
        {
        }

        /// <summary>
        /// Initzializes a new collision object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="sphere">Bounding sphere of object</param>
        public CollisionObject(CollisionFieldManager collisionFieldManager, BoundingSphere sphere)
            : this(collisionFieldManager, null, sphere)
        {
        }

        /// <summary>
        /// Updates collision object
        /// </summary>
        /// <param name="vectorPosition">Current position</param>
        public void Update(Vector3 vectorPosition)
        {
            RequiredFieldUnits -= Vector3.Distance(this.position, vectorPosition);
            this.position = vectorPosition;

            sphere = new BoundingSphere(vectorPosition, sphere.Radius);
            if (RequiredFieldUnits <= 0)
            {
                collisionFieldManager.CheckFieldCollision(this);
            }
        }

        /// <summary>
        /// Updates collision object
        /// </summary>
        /// <param name="vectorPosition">Current position</param>
        /// <param name="radius">Current radius of bounding sphere</param>
        public void Update(Vector3 vectorPosition, float radius)
        {
            if (!disposing)
            {
                RequiredFieldUnits -= Vector3.Distance(this.position, vectorPosition);
                RequiredFieldUnits -= (radius - Sphere.Radius);

                this.position = vectorPosition;
                sphere = new BoundingSphere(vectorPosition, radius);
                if (RequiredFieldUnits <= 0)
                {
                    collisionFieldManager.CheckFieldCollision(this);
                }
            }
        }

        /// <summary>
        /// Removes collision object in all overlapping fields
        /// </summary>
        internal void ClearFields()
        {
            for (int i = 0; i < InFields.Count; i++)
            {
                collisionFieldManager.RemoveCollisionObject(InFields[i], this);
            }
            InFields.Clear();
        }

        /// <summary>
        /// Disposes a collision object and it's references in collision field manager
        /// </summary>
        public void Dispose()
        {
            if(!disposing)
            {
                disposing = true;
                ClearFields();
                collisionFieldManager.CollisionObjects.Remove(this);
            }
        }
    }
}
