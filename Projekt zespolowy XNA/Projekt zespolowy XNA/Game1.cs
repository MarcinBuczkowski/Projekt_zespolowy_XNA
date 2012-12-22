using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Projekt_zespolowy_XNA
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // To tekstura, któr¹ mo¿emy wyrenderowaæ
        Texture2D spriteTexture;
        public Rectangle spriteRectangle;

        Texture2D autoTexture;
        Rectangle autoRectangle;
        Rectangle MapRectangle;

        Color[] spriteTextureData;
        Color[] autoTextureData;
        Color[] MapTexturedata;

        //Centralna czêœæ obrazka
        Vector2 spriteOrigin;

        public Vector2 spritePosition;
        Vector2 AiCarPosition;
        float rotation;

        Vector2 spriteVelocity;
        //Ustalenie prêdkoœci pojazdu
        const float tangentialVelocity = 7f;
        //Ustalenie d³ugoœci poœlizgu do zatrzymania
        float friction = 0.1f;

        Camera camera;
        Texture2D backgroundTexture;
        Vector2 backgroundPosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }



        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>



        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);


            base.Initialize();
        }



        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>



        protected override void LoadContent()
        {

            // Tworzymy nowego SpriteBatch, który jest u¿yty do narysowania tekstur
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Wczytywanie auta i t³a
            spriteTexture = Content.Load<Texture2D>("auto");
            //Pozycja samochodu na trasie
            spritePosition = new Vector2(300, 295);

            autoTexture = Content.Load<Texture2D>("auto2");
            AiCarPosition = new Vector2(600, 295);
            //Pozycja samochodu na trasie
           

            backgroundTexture = Content.Load<Texture2D>("trasa");
            //Po³o¿enie wyœwietlanej trasy
            backgroundPosition = new Vector2(-400, 0);
            MapTexturedata = new Color[backgroundTexture.Width*backgroundTexture.Height];
            backgroundTexture.GetData(MapTexturedata);

            spriteTextureData = new Color[spriteTexture.Width * spriteTexture.Height];
            spriteTexture.GetData(spriteTextureData);

            autoTextureData = new Color[autoTexture.Width * autoTexture.Height];
            autoTexture.GetData(autoTextureData);
        }



        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>

        protected override void UnloadContent()
        {
            autoTexture.Dispose();
            spriteTexture.Dispose();
            backgroundTexture.Dispose();
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>



        protected override void Update(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            Matrix MapTransform = Matrix.CreateTranslation(new Vector3(backgroundPosition, 0.0f));

            Matrix AiCarTransform =
                Matrix.CreateTranslation(new Vector3(AiCarPosition, 0.0f));

            Matrix UserCarTransform =
                    Matrix.CreateTranslation(new Vector3(-spriteOrigin, 0.0f)) *
                // Matrix.CreateScale(block.Scale) *  would go here
                    Matrix.CreateRotationZ(rotation) *
                    Matrix.CreateTranslation(new Vector3(spritePosition, 0.0f));

            spriteRectangle = Collisions.CalculateBoundingRectangle(
                         new Rectangle(0, 0, spriteTexture.Width, spriteTexture.Height),
                         UserCarTransform);

            autoRectangle = new Rectangle((int)AiCarPosition.X, (int)AiCarPosition.Y, autoTexture.Width, autoTexture.Height);

            MapRectangle = new Rectangle((int)backgroundPosition.X, (int)backgroundPosition.Y, backgroundTexture.Width, backgroundTexture.Height);

            spriteOrigin = new Vector2(spriteRectangle.Width / 2, spriteRectangle.Height / 2);

            //Poruszanie do przodu uwzglêdniaj¹c rotacjê
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                //Obroty samochodu
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) rotation += 0.05f;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) rotation -= 0.05f;
                spriteVelocity.X = (float)Math.Cos(rotation) * tangentialVelocity;
                spriteVelocity.Y = (float)Math.Sin(rotation) * tangentialVelocity;
            }
            else if (spriteVelocity != Vector2.Zero)
            {
                float i = spriteVelocity.X;
                float j = spriteVelocity.Y;

                if (spriteVelocity != Vector2.Zero) spriteVelocity *= 1 - friction;
            }
            if (autoRectangle.Intersects(spriteRectangle))
            {
                if (Collisions.IntersectPixels(AiCarTransform, autoTexture.Width, autoTexture.Height, autoTextureData, UserCarTransform, spriteTexture.Width, spriteTexture.Height, spriteTextureData))
                {
                    spriteVelocity.X = -spriteVelocity.X;
                    spriteVelocity.Y = -spriteVelocity.Y;
                }
            }
           
            if (Collisions.Intersect(UserCarTransform, spriteTexture.Width, spriteTexture.Height, spriteTextureData, MapTransform, backgroundTexture.Width, backgroundTexture.Height, MapTexturedata))
            {
                spriteVelocity.X = -spriteVelocity.X;
                spriteVelocity.Y = -spriteVelocity.Y;
            }

            spritePosition = (spriteVelocity + spritePosition);
            camera.Update(gameTime, this);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null, null, null, null,
                camera.transform);

            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);
            spriteBatch.Draw(autoTexture, autoRectangle, Color.White);
            //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna czêœæ obrazka, bez efektów
            spriteBatch.Draw(spriteTexture, spritePosition, null, Color.White, rotation, spriteOrigin, 1f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}