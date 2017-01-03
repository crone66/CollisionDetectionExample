using CollisionDetection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CollisionDetectionExample
{
    public class Game1 : Game
    {
        public static Game1 game;
        private const float modelScale = 2f;
        private const float camSpeed = 0.3f;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;

        private CollisionFieldManager collisionFieldManager;       
        private CollisionCheck collisionCheck;
        private CollisionDebugging collisionDebugging;

        private KeyboardState lastKeyboardState;
        private Vector3 camPos;

        private List<int> collisionIds;
        private List<PhysicalBaseObject> collideableObjects;

        private Model model;
        private bool drawObjects;

        private int frameRate;
        private int frameCounter;
        private TimeSpan elapsedTime;
        private Stopwatch drawStopwatch;
        private Stopwatch updateStopwatch;
        private Stopwatch collisionStopWatch;
        private float requiredDrawTime;
        private float requiredUpdateTime;
        private float requiredCollisionTime;
        private int UnoptimizedCollisions;
        private int UnoptimizedCollisionChecks;   

        private Stopwatch dr;
        private List<float> skips;
        private float skipMax;
        private float skipping;

        private Vector3 defaultColor;
        private Vector3 collisionColor;

        private Matrix projection;

        public CollisionFieldManager CollisionFieldManager
        {
            get { return collisionFieldManager; }
            set { collisionFieldManager = value; }
        }
        
        public Game1()
        {
            Game1.game = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            //update projection
            float aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    aspectRatio,
                                                                    1, 10000);
            
            //Load font and sphere
            font = Content.Load<SpriteFont>("font");
            model = Content.Load<Model>("Planet");

            Init();
        }

        private void Init()
        {
            camPos = new Vector3(0, 0, 600);
            collideableObjects = new List<PhysicalBaseObject>();
            collisionIds = new List<int>();

            drawObjects = true;
            frameRate = 0;
            frameCounter = 0;
            elapsedTime = TimeSpan.Zero;
            drawStopwatch = new Stopwatch();
            updateStopwatch = new Stopwatch();
            collisionStopWatch = new Stopwatch();
            requiredDrawTime = 0;
            requiredUpdateTime = 0;
            requiredCollisionTime = 0;
            UnoptimizedCollisions = 0;
            UnoptimizedCollisionChecks = 0;

            dr = new Stopwatch();
            skips = new List<float>();
            skipMax = 0;
            skipping = 0;

            defaultColor = new Vector3(0, 1, 0);
            collisionColor = new Vector3(1, 0, 0);

            collisionFieldManager = new CollisionFieldManager(new Vector3(-500, -500, 0), 1000, 1000, 20, true);

            //init CollisionDebugging
            collisionDebugging = new CollisionDebugging(collisionFieldManager, graphics.GraphicsDevice, Color.Gray);
            
            //Create collidable objects
            CreateObjects(1000);

            //init CollisionCheck
            collisionCheck = new CollisionCheck(collisionFieldManager, CollisionCheck_OnCollision, true, true);
        }

        /// <summary>
        /// This function will be called on each collision of two objects. Tracks all collided objects  
        /// </summary>
        /// <param name="sender">OnCollision sender</param>
        /// <param name="e">Collision event args</param>
        private void CollisionCheck_OnCollision(object sender, CollisionEventArgs e)
        {
            PhysicalBaseObject bo1 = e.SourceObject.BaseObject as PhysicalBaseObject;
            PhysicalBaseObject bo2 = e.DestinationObject.BaseObject as PhysicalBaseObject;

            //Highlight field
            collisionDebugging.AddHighlightToQueue(e.Field, Color.Red, 0);

            //track collisions
            if (!collisionIds.Contains(bo1.Id))
            {
                collisionIds.Add(bo1.Id);
            }

            if (!collisionIds.Contains(bo2.Id))
            {
                collisionIds.Add(bo2.Id);
            }
        }
        
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Creates a given number of collidable objects
        /// </summary>
        /// <param name="count"></param>
        private void CreateObjects(int count)
        {
            //Calculate boundingSphere radius of the sphere
            float radius = getModelSize(model) * modelScale;
            Random random = new Random();

            //Create objects with random start position
            for (int i = 0; i < count; i++)
            {
                collideableObjects.Add(new PhysicalBaseObject(new Vector3(random.Next(-4700, 4700) / 10f, random.Next(-4700, 4700) / 10f, 0f), radius));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            updateStopwatch.Restart();

            //clear tracked collision since last update
            collisionIds.Clear();

            //Calculate framerate
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            //Handle Keyboard input
            KeyboardInput();

            //Update camera position
            UpdateCamera(gameTime);

            //Update debugging grid
            collisionDebugging.Update(gameTime);

            
            skipping += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            collisionStopWatch.Restart();

            if (collisionCheck.UseThreading)
            {
                //update objects when collision detection is done (prevents race condition)
                if (collisionFieldManager.UpdateObjects)
                {
                    //Update objects
                    UpdateObjects(gameTime);

                    //measuring time between updates
                    if (skipMax < skipping)
                        skipMax = skipping;
                    skips.Add(skipping);
                    skipping = 0f;
                }
            }
            else
            {
                //update objects
                UpdateObjects(gameTime);
                //do collision detection
                collisionCheck.CalculateCollisions();
            }
            collisionStopWatch.Stop();

            base.Update(gameTime);
            updateStopwatch.Stop();
            requiredUpdateTime = updateStopwatch.ElapsedMilliseconds;
            requiredCollisionTime = collisionStopWatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateObjects(GameTime gameTime)
        {
            //update (move) all objects
            for (int i = 0; i < collideableObjects.Count; i++)
            {
                collideableObjects[i].Update(gameTime);
            }

            //deativeate update objects and reactivate collision detection (prevents race condition)
            collisionFieldManager.UpdateObjects = false;
            collisionFieldManager.DoCollision = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
            drawStopwatch.Restart();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            //update view
            Matrix view = Matrix.CreateTranslation(0, 0, 0) *
            Matrix.CreateLookAt(camPos, new Vector3(camPos.X, camPos.Y, 0), Vector3.Up);

            frameCounter++;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            collisionDebugging.Draw(view, projection);

            //draw all objects
            if (drawObjects)
            {
                for (int i = 0; i < collideableObjects.Count; i++)
                {
                    DrawModel(model, Matrix.CreateTranslation(collideableObjects[i].Position), view, projection, collisionIds.Contains(collideableObjects[i].Id));
                }
            }

            //draw debugging, collision and performance information
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FieldCount: " + collisionFieldManager.FieldCount.ToString(), new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(font, "UsedFields: " + collisionFieldManager.FieldsWithObjectsCount.ToString(), new Vector2(10, 30), Color.Black);
            spriteBatch.DrawString(font, "Collideable Objects: " + collideableObjects.Count.ToString(), new Vector2(10, 50), Color.Black);

            spriteBatch.DrawString(font, "Required Collisionchecks: " + collisionCheck.CollisionChecks.ToString() + " / " + collisionCheck.MaxCollisionChecks.ToString(), new Vector2(10, 90), Color.Black);
            spriteBatch.DrawString(font, "Collisions: " + collisionCheck.Collisions.ToString() + " / " + collisionCheck.MaxCollisions.ToString(), new Vector2(10, 110), Color.Black);
            spriteBatch.DrawString(font, "Unoptimized CollisionDetection: " + UnoptimizedCollisions.ToString() + " / " + UnoptimizedCollisionChecks.ToString(), new Vector2(10, 130), Color.Black);

            spriteBatch.DrawString(font, "FPS: " + frameRate.ToString(), new Vector2(10, 170), Color.Black);
            spriteBatch.DrawString(font, "DrawTime: " + requiredDrawTime.ToString(), new Vector2(10, 190), Color.Black);
            spriteBatch.DrawString(font, "UpdateTime: " + requiredUpdateTime.ToString(), new Vector2(10, 210), Color.Black);
            spriteBatch.DrawString(font, "CollisionTime: " + requiredCollisionTime.ToString(), new Vector2(10, 230), Color.Black);

            if (collisionCheck.UseThreading)
            {
                spriteBatch.DrawString(font, "SkipTime: " + skips.Average().ToString(), new Vector2(10, 250), Color.Black);
            }

            spriteBatch.End();
            base.Draw(gameTime);

            drawStopwatch.Stop();
            requiredDrawTime = drawStopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Draws a model with defaultLightning and highlights it on collisions
        /// </summary>
        /// <param name="model">loaded model</param>
        /// <param name="worldMatrix">this matrix contains the rotation, scalation and translation information</param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="collision"></param>
        private void DrawModel(Model model, Matrix worldMatrix, Matrix view, Matrix projection, bool collision)
        {
            //Draw meshes
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = Matrix.CreateScale(modelScale) * worldMatrix;
                    effect.View = view;
                    effect.Projection = projection;

                    //if object collides highlight object (red color)
                    effect.DiffuseColor = (collision ? collisionColor : defaultColor);
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// Calculates radius of a model
        /// </summary>
        /// <param name="model">loaded model</param>
        /// <returns>returns radius in float (radius depends on BoundingSphere size)</returns>
        private static float getModelSize(Model model)
        {
            BoundingSphere sphere = new BoundingSphere();

            foreach (ModelMesh mesh in model.Meshes)
            {
                if (sphere.Radius == 0)
                    sphere = mesh.BoundingSphere;
                else
                    sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
            }

            return sphere.Radius;
        }

        /// <summary>
        /// Does a simple collision detection by compareing each object with all remaining objects
        /// </summary>
        private void UnoptimizedCollisionDetection()
        {
            UnoptimizedCollisionChecks = 0;
            UnoptimizedCollisions = 0;
            for (int i = 0; i < collideableObjects.Count; i++)
            {
                for (int j = i + 1; j < collideableObjects.Count; j++)
                {
                    if (collideableObjects[i].Sphere.Contains(collideableObjects[j].Sphere) != ContainmentType.Disjoint)
                    {
                        UnoptimizedCollisions++;
                        if (!collisionIds.Contains(collideableObjects[i].Id))
                            collisionIds.Add(collideableObjects[i].Id);
                        if (!collisionIds.Contains(collideableObjects[j].Id))
                            collisionIds.Add(collideableObjects[j].Id);
                    }
                    UnoptimizedCollisionChecks++;
                }
            }
        }

        /// <summary>
        /// Handles keyboard input to toggle views and functions
        /// </summary>
        private void KeyboardInput()
        {
            KeyboardState currentState = Keyboard.GetState();

            if (currentState.IsKeyUp(Keys.Escape) && lastKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (currentState.IsKeyUp(Keys.T) && lastKeyboardState.IsKeyDown(Keys.T))
                drawObjects = !drawObjects;

            if (currentState.IsKeyUp(Keys.H) && lastKeyboardState.IsKeyDown(Keys.H))
                collisionDebugging.ToggleHighlightFieldsWithObjects(Color.Green);

            if (currentState.IsKeyUp(Keys.C) && lastKeyboardState.IsKeyDown(Keys.C))
            {
                UnoptimizedCollisionDetection();
            }

            lastKeyboardState = currentState;
        }

        /// <summary>
        /// Handles keyboard input to update and move the camera
        /// </summary>
        /// <param name="gameTime">contains the elapsed time since last update</param>
        private void UpdateCamera(GameTime gameTime)
        {
            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentKeyboardState.IsKeyDown(Keys.W))
            {
                camPos.Y += time * camSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentKeyboardState.IsKeyDown(Keys.S))
            {
                camPos.Y -= time * camSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentKeyboardState.IsKeyDown(Keys.D))
            {
                camPos.X += time * camSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentKeyboardState.IsKeyDown(Keys.A))
            {
                camPos.X -= time * camSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Z))
            {
                camPos.Z += time * camSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.X))
            {
                camPos.Z -= time * camSpeed;
            }
        }
    }
}
