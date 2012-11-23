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

    /// <summary>
    /// This is the main type for your game
    /// </summary> 



    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // To tekstura, któr¹ mo¿emy wyrenderowaæ
        Texture2D spriteTexture;
        public Rectangle spriteRectangle;

        //Centralna czêœæ obrazka
        Vector2 spriteOrigin;

        public Vector2 spritePosition;
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

            backgroundTexture = Content.Load<Texture2D>("Background");
            //Po³o¿enie wyœwietlanej trasy
            backgroundPosition = new Vector2(-400, 0);

        }



        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>




        protected override void UnloadContent()
        {
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

            spriteRectangle = new Rectangle((int)spritePosition.X, (int)spritePosition.Y,
                spriteTexture.Width, spriteTexture.Height);
            spritePosition = spriteVelocity + spritePosition;
            spriteOrigin = new Vector2(spriteRectangle.Width / 2, spriteRectangle.Height / 2);



            //Poruszanie do przodu uwzglêdniaj¹c rotacjê
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                //Obroty samochodu
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) rotation += 0.1f;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) rotation -= 0.1f;

                spriteVelocity.X = (float)Math.Cos(rotation) * tangentialVelocity;
                spriteVelocity.Y = (float)Math.Sin(rotation) * tangentialVelocity;
            }
            else if (spriteVelocity != Vector2.Zero)
            {
                float i = spriteVelocity.X;
                float j = spriteVelocity.Y;

                if (spriteVelocity != Vector2.Zero) spriteVelocity *= 1 - friction;

            }


            camera.Update(gameTime, this);
            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null, null, null, null,
                camera.transform);

            spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

            //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna czêœæ obrazka, bez efektów
            spriteBatch.Draw(spriteTexture, spritePosition, null, Color.White, rotation, spriteOrigin, 1f, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}