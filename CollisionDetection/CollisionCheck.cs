using System;
using System.Threading;

namespace CollisionDetection
{
    public class CollisionCheck
    {
        /// <summary>
        /// On collision event, will be fired on collision of two objects
        /// </summary>
        public event EventHandler<CollisionEventArgs> OnCollision;
        
        private CollisionFieldManager collisionFieldManager;
        private bool loop;

        private bool measurePerformance;
        private int collisionChecks;
        private int maxCollisionChecks;
        private int collisions;
        private int maxCollisions;

        private bool useThreading;
        private Thread thread;
        
        /// <summary>
        /// Measures performance for debugging purpose
        /// </summary>
        public bool MeasurePerformance
        {
            get
            {
                return measurePerformance;
            }

            set
            {
                measurePerformance = value;
            }
        }

        /// <summary>
        /// Counts collision checks for debugging purpose
        /// </summary>
        public int CollisionChecks
        {
            get
            {
                return collisionChecks;
            }
        }

        /// <summary>
        /// Maximum collision checks for debugging purpose
        /// </summary>
        public int MaxCollisionChecks
        {
            get
            {
                return maxCollisionChecks;
            }
        }

        /// <summary>
        /// Count collisions for debugging purpose
        /// </summary>
        public int Collisions
        {
            get
            {
                return collisions;
            }
        }

        /// <summary>
        /// Maxmimum collisions for debugging purpose
        /// </summary>
        public int MaxCollisions
        {
            get
            {
                return maxCollisions;
            }
        }

        /// <summary>
        /// Indicates whenther to use threading or not
        /// </summary>
        public bool UseThreading
        {
            get
            {
                return useThreading;
            }
        }

        /// <summary>
        /// Initzializes a new collision check object
        /// </summary>
        /// <param name="collisionFieldManager">Collision field manager</param>
        /// <param name="onCollision">Subscription of the OnCollision event</param>
        /// <param name="useThreading">Indicates whenther to use threading or not</param>
        /// <param name="measurePerformance">Indicates whenther to measure performance or not</param>
        public CollisionCheck(CollisionFieldManager collisionFieldManager, EventHandler<CollisionEventArgs> onCollision, bool useThreading = false, bool measurePerformance = false)
        {
            loop = true;
            OnCollision += onCollision;

            collisionChecks = 0;
            maxCollisionChecks = 0;
            collisions = 0;
            maxCollisions = 0;
            this.measurePerformance = measurePerformance;

            this.collisionFieldManager = collisionFieldManager;
            this.useThreading = useThreading;
            if (useThreading)
            {
                thread = new Thread(CollisionLoop);
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start();
            }
        }

        /// <summary>
        /// Does a collision check on a collision field
        /// </summary>
        /// <param name="field">Collision field</param>
        private void CheckField(CollisionField field)
        {       
            for (int i = 0; i < field.CollisionObjects.Count; i++)
            {
                for (int j = i + 1; j < field.CollisionObjects.Count; j++)
                {
                    if (field.CollisionObjects[i] != null && field.CollisionObjects[j] != null)
                    {
                        if (measurePerformance)
                        {
                            collisionChecks++;
                            if (maxCollisionChecks < collisionChecks)
                                maxCollisionChecks = collisionChecks;
                        }

                        if (field.CollisionObjects[i].Sphere.Contains(field.CollisionObjects[j].Sphere) != Microsoft.Xna.Framework.ContainmentType.Disjoint)
                        {
                            if (field.CollisionObjects[i].VirtualObject || field.CollisionObjects[j].VirtualObject)
                            {
                                if(field.CollisionObjects[i].VirtualObject)
                                {
                                    if (!field.CollisionObjects[i].VirtualCollisions.Contains(field.CollisionObjects[j].BaseObject))
                                        field.CollisionObjects[i].VirtualCollisions.Add(field.CollisionObjects[j].BaseObject);
                                }

                                if (field.CollisionObjects[j].VirtualObject)
                                {
                                    if(!field.CollisionObjects[j].VirtualCollisions.Contains(field.CollisionObjects[i].BaseObject))
                                        field.CollisionObjects[j].VirtualCollisions.Add(field.CollisionObjects[i].BaseObject);
                                }
                            }
                            else
                            {
                                if (measurePerformance)
                                {
                                    collisions++;
                                }

                                OnCollision?.Invoke(this, new CollisionEventArgs(field, field.CollisionObjects[i], field.CollisionObjects[j]));
                            }               
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loops though all collision fields with collidable objects
        /// </summary>
        public void CalculateCollisions()
        {
            if (measurePerformance)
            {
                collisionChecks = 0;
                collisions = 0;
            }

            collisionFieldManager.DoCollision = false;
            for (int i = 0; i < collisionFieldManager.CollisionFieldsWithObjects.Count; i++)
            {
                CheckField(collisionFieldManager.CollisionFieldsWithObjects[i]);
            }
            collisionFieldManager.UpdateObjects = true;

            if (measurePerformance)
            {
                if(collisions > maxCollisions)
                    maxCollisions = collisions;
            }
        }

        /// <summary>
        /// Repeats collision checks used by multi threaded collision checks
        /// </summary>
        private void CollisionLoop()
        {
            while (loop)
            {
                if (collisionFieldManager.DoCollision)
                    CalculateCollisions();
            }
        }

        /// <summary>
        /// Stops threaded collision detection loop
        /// </summary>
        public void StopThreadedCollision()
        {
            loop = false;
        }
    }
}
