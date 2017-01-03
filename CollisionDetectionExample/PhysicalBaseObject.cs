using System;
using Microsoft.Xna.Framework;

namespace CollisionDetectionExample
{
    public class PhysicalBaseObject
    {
        public static int Counter = 0;
        public static Random random = new Random();

        private int id;
        private Vector3 position;
        private float radius;
        private BoundingSphere sphere;
        private float speed;
        private CollisionDetection.CollisionObject collisionObject;

        public int Id
        {
            get { return id; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public BoundingSphere Sphere
        {
            get { return sphere; }
        }

        public PhysicalBaseObject(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
            id = Counter++;
            speed = 0.01f;
            sphere = new BoundingSphere(position, radius);
            collisionObject = new CollisionDetection.CollisionObject(Game1.game.CollisionFieldManager, this, this.sphere);
        }

        public void Update(GameTime gameTime)
        {
            int value = random.Next(0, 100);
            if (value < 50)
            {
                this.position.X += (float)gameTime.ElapsedGameTime.TotalMilliseconds * speed * (random.Next(0, 100) < 50 ? -1 : 1);
                if (position.X < -470)
                    position.X = -470;
                else if (position.X > 470)
                    position.X = 470;
            }
            else
            {
                this.position.Y += (float)gameTime.ElapsedGameTime.TotalMilliseconds * speed * (random.Next(0, 100) < 50 ? -1 : 1);
                if (position.Y < -470)
                    position.Y = -470;
                else if (position.Y > 470)
                    position.Y = 470;
            }

            
            collisionObject.Update(new Vector3(this.position.X, this.position.Y, this.position.Z));
        }
    }
}
