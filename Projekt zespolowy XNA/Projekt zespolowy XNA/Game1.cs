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
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // To tekstura, któr¹ mo¿emy wyrenderowaæ
        private Texture2D playerTexture;
        public  Rectangle playerRectangle;

        private AI firstAi = new AI();
        private Color[] firstAiTextureData;
        private Vector2 firstAiOrigin;
        private Texture2D firstAiTexture;
        private Rectangle firstAiRectangle;

        private Rectangle mapRectangle;

        private Color[] playerTextureData;
        private Color[] mapTextureData;

        //Centralna czêœæ obrazka
        private Vector2 playerOrigin;
        public  Vector2 playerPosition;
        private float   playerRotation;
        private Vector2 playerVelocity;

        //Ustalenie prêdkoœci pojazdu
        private const float playerTangentialVelocity = 7f;
        //Ustalenie d³ugoœci poœlizgu do zatrzymania
        private float friction = 0.1f;

        private Camera camera;
        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;

        private Boolean playerCollision = false;

        private SpriteFont font;
        private int lap = 0;
        private int maxlap = 3;
        private bool lapChange = false;
        private DateTime lastLapChange = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));

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
            playerTexture = Content.Load<Texture2D>("auto");
            //Pozycja samochodu na trasie
            playerPosition = new Vector2(300, 295);

            firstAiTexture = Content.Load<Texture2D>("przeciwnik");

            backgroundTexture = Content.Load<Texture2D>("trasa");
            //Po³o¿enie wyœwietlanej trasy
            backgroundPosition = new Vector2(-400, 0);

            mapTextureData = new Color[backgroundTexture.Width * backgroundTexture.Height];
            backgroundTexture.GetData(mapTextureData);

            playerTextureData = new Color[playerTexture.Width * playerTexture.Height];
            playerTexture.GetData(playerTextureData);

            firstAiTextureData = new Color[firstAiTexture.Width * firstAiTexture.Height];
            firstAiTexture.GetData(firstAiTextureData);

            font = Content.Load<SpriteFont> ("SpriteFont");
        }



        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            firstAiTexture.Dispose();
            playerTexture.Dispose();
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
            {
                this.Exit();
            }

            Matrix mapTransform = Matrix.CreateTranslation(new Vector3(backgroundPosition, 0.0f));

            Matrix firstAiTransform = Matrix.CreateTranslation(new Vector3(-firstAiOrigin, 0.0f)) *
                                      Matrix.CreateRotationZ(firstAi.rotation) *
                                      Matrix.CreateTranslation(new Vector3(firstAi.position, 0.0f));

            Matrix playerTransform = Matrix.CreateTranslation(new Vector3(-playerOrigin, 0.0f)) *
                                     Matrix.CreateRotationZ(playerRotation) *
                                     Matrix.CreateTranslation(new Vector3(playerPosition, 0.0f));

            playerRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, playerTexture.Width, playerTexture.Height), playerTransform);
            playerOrigin = new Vector2(playerRectangle.Width / 2, playerRectangle.Height / 2);

            firstAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, firstAiTexture.Width, firstAiTexture.Height), firstAiTransform);
            firstAiOrigin = new Vector2(firstAiRectangle.Width / 2, firstAiRectangle.Height / 2);

            mapRectangle = new Rectangle((int)backgroundPosition.X, (int)backgroundPosition.Y, backgroundTexture.Width, backgroundTexture.Height);

            //Poruszanie do przodu uwzglêdniaj¹c rotacjê
            if (this.playerCollision)
            {
                if (playerVelocity.X > -0.5 && playerVelocity.X < 0.5 && playerVelocity.Y > -0.5 && playerVelocity.Y < 0.5)
                {
                    this.playerCollision = false;
                }
                else
                {
                    playerVelocity *= 1 - friction;
                }
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                //Obroty samochodu
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) playerRotation += 0.05f;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) playerRotation -= 0.05f;
                playerVelocity.X = (float)Math.Cos(playerRotation) * playerTangentialVelocity;
                playerVelocity.Y = (float)Math.Sin(playerRotation) * playerTangentialVelocity;
            }
            else if (playerVelocity != Vector2.Zero)
            {
                playerVelocity *= 1 - friction;
            }

            // Kolizja z przeciwnikiem
            if (firstAiRectangle.Intersects(playerRectangle))
            {
                if (Collisions.IntersectPixels(firstAiTransform, firstAiTexture.Width, firstAiTexture.Height, firstAiTextureData, playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData))
                {
                    playerVelocity.X = -playerVelocity.X;
                    playerVelocity.Y = -playerVelocity.Y;
                    this.playerCollision = true;
                }
            }

            if (Collisions.Intersect(playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData, mapTransform, backgroundTexture.Width, backgroundTexture.Height, mapTextureData))
            {
                playerVelocity.X = -playerVelocity.X;
                playerVelocity.Y = -playerVelocity.Y;
                this.playerCollision = true;
            }

            playerPosition = (playerVelocity + playerPosition);

            Color[] c = new Color[1];
            backgroundTexture.GetData(0, new Rectangle((int)playerPosition.X+400, (int)playerPosition.Y, 1, 1), c, 0, c.Length);
            Color black = new Color(43, 42, 41);
            Color white = new Color(254, 254, 254);
            if (c[0] == black || c[0] == white)
            {
                lapChange = true;
            }
            else
            {
                if (lapChange && (DateTime.Now.Subtract(lastLapChange) > TimeSpan.FromSeconds(1)))
                {
                    lastLapChange = DateTime.Now;
                    lap++;
                }
                lapChange = false;
            }

            firstAi.CalculatePosition(backgroundTexture);

            camera.Update(gameTime, playerPosition, playerRectangle);
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
            spriteBatch.Draw(firstAiTexture, firstAi.position, null, Color.White, firstAi.rotation, firstAiOrigin, 1f, SpriteEffects.None, 0);
            //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna czêœæ obrazka, bez efektów
            spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, playerRotation, playerOrigin, 1f, SpriteEffects.None, 0);
            //Informacje o przebiegu gry
            int internalLap = lap;
            if (internalLap < 1)
            {
                internalLap = 1;
            }
            spriteBatch.DrawString(font, "Okr¹¿enie: " + internalLap.ToString() + " z " + maxlap.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 100), Color.White);
            spriteBatch.DrawString(font, "Czas: " + gameTime.TotalGameTime.Minutes.ToString() + ":" + gameTime.TotalGameTime.Seconds.ToString() + "." + gameTime.TotalGameTime.Milliseconds.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 80), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}