using System;

namespace CollisionDetection
{
    public class CollisionEventArgs : EventArgs
    {
        /// <summary>
        /// Collision source object
        /// </summary>
        public CollisionObject SourceObject;

        /// <summary>
        /// Collision destination object
        /// </summary>
        public CollisionObject DestinationObject;

        /// <summary>
        /// Collision field
        /// </summary>
        public CollisionField Field;

        /// <summary>
        /// Initzializes a new collision event args object
        /// </summary>
        /// <param name="field">Collision field</param>
        /// <param name="sourceObject">Collision source object</param>
        /// <param name="destinationObject">Collision destination object</param>
        public CollisionEventArgs(CollisionField field, CollisionObject sourceObject, CollisionObject destinationObject)
        {
            Field = field;
            SourceObject = sourceObject;
            DestinationObject = destinationObject;
        }
    }
}
