using System;

namespace CollisionDetection
{
    public class OutOfGridException : Exception
    {
        public CollisionObject CollisionObject;

        public OutOfGridException(CollisionObject collisionObject)
            :base("Collision objects doesn't overlap with the collision grid!")
        {
            CollisionObject = collisionObject;
        }
    }
}
