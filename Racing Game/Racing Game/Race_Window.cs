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


namespace Racing_Game
{
    class Race_Window : Microsoft.Xna.Framework.Game
    {
        ClockTimer clock = new ClockTimer();

        // To tekstura, którą możemy wyrenderować
        private Texture2D playerTexture;
        public Rectangle playerRectangle;

        private enum Dest { North, East, South, West };
        private Dest playerDestination;

        private AI firstAi = new AI(new Vector2(350, 211), 6f);
        private Color[] firstAiTextureData;
        private Vector2 firstAiOrigin;
        private Texture2D firstAiTexture;
        private Rectangle firstAiRectangle;

        private AI secondAi = new AI(new Vector2(350, 377), 5f);
        private Color[] secondAiTextureData;
        private Vector2 secondAiOrigin;
        private Texture2D secondAiTexture;
        private Rectangle secondAiRectangle;

        private Rectangle mapRectangle;

        private Color[] playerTextureData;
        private Color[] mapTextureData;

        //Centralna część obrazka
        private Vector2 playerOrigin;
        public Vector2 playerPosition;
        private float playerRotation;
        private Vector2 playerVelocity;

        //Ustalenie prędkości pojazdu
        private const float playerTangentialVelocity = 7f;
        //Ustalenie długości poślizgu do zatrzymania
        private float friction = 0.1f;

        private Camera camera;
        private Texture2D backgroundTexture;
        private Vector2 backgroundPosition;
        private Texture2D mapTexture;

        private Boolean playerCollision = false;

        private SpriteFont font;
        private SpriteFont font2;
        private int lap = 0;
        private int maxlap = 3;
        private bool lapChange = false;
        private DateTime lastLapChange = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));

        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        public void LoadContent(ContentManager Content,Texture2D tex1, Texture2D tex2)
        {
            playerTexture = Content.Load<Texture2D>("auto");
            //Pozycja samochodu na trasie
            playerPosition = new Vector2(360, 294);

            firstAiTexture = Content.Load<Texture2D>("przeciwnik");
            secondAiTexture = Content.Load<Texture2D>("przeciwnik2");
           
            //Położenie wyświetlanej trasy
            backgroundTexture = tex1;
            mapTexture = tex2;
            backgroundPosition = new Vector2(-400, 0);

            //mapTexture = Content.Load<Texture2D>("map");

            mapTextureData = new Color[backgroundTexture.Width * backgroundTexture.Height];
            backgroundTexture.GetData(mapTextureData);

            playerTextureData = new Color[playerTexture.Width * playerTexture.Height];
            playerTexture.GetData(playerTextureData);

            firstAiTextureData = new Color[firstAiTexture.Width * firstAiTexture.Height];
            firstAiTexture.GetData(firstAiTextureData);

            secondAiTextureData = new Color[secondAiTexture.Width * secondAiTexture.Height];
            secondAiTexture.GetData(secondAiTextureData);

            font = Content.Load<SpriteFont>("SpriteFont");
            font2 = Content.Load<SpriteFont>("SpriteFont2");
        }

        public void Update(GameTime gameTime, Camera camera)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            if (clock.isRunning == false)
            {
                //count 3 seconds down 
                clock.start(3);
                //Gdy zegar odlicza porusznie zablokowane
                Matrix mapTransform = Matrix.CreateTranslation(new Vector3(backgroundPosition, 0.0f));

                Matrix firstAiTransform = Matrix.CreateTranslation(new Vector3(-firstAiOrigin, 0.0f)) *
                                          Matrix.CreateRotationZ(firstAi.rotation) *
                                          Matrix.CreateTranslation(new Vector3(firstAi.position, 0.0f));

                Matrix secondAiTransform = Matrix.CreateTranslation(new Vector3(-secondAiOrigin, 0.0f)) *
                                           Matrix.CreateRotationZ(secondAi.rotation) *
                                           Matrix.CreateTranslation(new Vector3(secondAi.position, 0.0f));

                Matrix playerTransform = Matrix.CreateTranslation(new Vector3(-playerOrigin, 0.0f)) *
                                         Matrix.CreateRotationZ(playerRotation) *
                                         Matrix.CreateTranslation(new Vector3(playerPosition, 0.0f));

                playerRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, playerTexture.Width, playerTexture.Height), playerTransform);
                playerOrigin = new Vector2(playerRectangle.Width / 2, playerRectangle.Height / 2);

                firstAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, firstAiTexture.Width, firstAiTexture.Height), firstAiTransform);
                firstAiOrigin = new Vector2(firstAiRectangle.Width / 2, firstAiRectangle.Height / 2);

                secondAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, secondAiTexture.Width, secondAiTexture.Height), secondAiTransform);
                secondAiOrigin = new Vector2(secondAiRectangle.Width / 2, secondAiRectangle.Height / 2);

                mapRectangle = new Rectangle((int)backgroundPosition.X, (int)backgroundPosition.Y, backgroundTexture.Width, backgroundTexture.Height);

                firstAi.CalculatePosition(backgroundTexture);
                secondAi.CalculatePosition(backgroundTexture);
                camera.Update(gameTime, playerPosition, playerRectangle);
                
            }
            else if (clock.isFinished == true)
            {
                //Gdy zegar skończy odliczać start wyścigu
                Matrix mapTransform = Matrix.CreateTranslation(new Vector3(backgroundPosition, 0.0f));

                Matrix firstAiTransform = Matrix.CreateTranslation(new Vector3(-firstAiOrigin, 0.0f)) *
                                          Matrix.CreateRotationZ(firstAi.rotation) *
                                          Matrix.CreateTranslation(new Vector3(firstAi.position, 0.0f));

                Matrix secondAiTransform = Matrix.CreateTranslation(new Vector3(-secondAiOrigin, 0.0f)) *
                                           Matrix.CreateRotationZ(secondAi.rotation) *
                                           Matrix.CreateTranslation(new Vector3(secondAi.position, 0.0f));

                Matrix playerTransform = Matrix.CreateTranslation(new Vector3(-playerOrigin, 0.0f)) *
                                         Matrix.CreateRotationZ(playerRotation) *
                                         Matrix.CreateTranslation(new Vector3(playerPosition, 0.0f));

                playerRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, playerTexture.Width, playerTexture.Height), playerTransform);
                playerOrigin = new Vector2(playerRectangle.Width / 2, playerRectangle.Height / 2);

                firstAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, firstAiTexture.Width, firstAiTexture.Height), firstAiTransform);
                firstAiOrigin = new Vector2(firstAiRectangle.Width / 2, firstAiRectangle.Height / 2);

                secondAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, secondAiTexture.Width, secondAiTexture.Height), secondAiTransform);
                secondAiOrigin = new Vector2(secondAiRectangle.Width / 2, secondAiRectangle.Height / 2);

                mapRectangle = new Rectangle((int)backgroundPosition.X, (int)backgroundPosition.Y, backgroundTexture.Width, backgroundTexture.Height);

                //Poruszanie do przodu uwzględniając rotację
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
                if (secondAiRectangle.Intersects(playerRectangle))
                {
                    if (Collisions.IntersectPixels(secondAiTransform, secondAiTexture.Width, secondAiTexture.Height, secondAiTextureData, playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData))
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

                float round = (float)Math.Round(playerRotation, 4);
                float sround = (float)(Math.Round(Math.Sin(round), 4));
                float cround = (float)(Math.Round(Math.Cos(round), 4));
                if ((sround >= 0.5 && sround <= 1) && (cround >= -0.5 && cround <= 0.5))
                {
                    playerDestination = Dest.South;
                }
                else if ((sround <= -0.5 && sround >= -1) && (cround >= -0.5 && cround <= 0.5))
                {
                    playerDestination = Dest.North;
                }
                else if ((sround >= -0.5 && sround <= 0.5) && (cround >= 0.5 && cround <= 1))
                {
                    playerDestination = Dest.East;
                }
                else if ((sround >= -0.5 && sround <= 0.5) && (cround <= -0.5 && cround >= -1))
                {
                    playerDestination = Dest.West;
                }

                playerPosition = (playerVelocity + playerPosition);

                Color[] c = new Color[1];
                backgroundTexture.GetData(0, new Rectangle((int)playerPosition.X + 400, (int)playerPosition.Y, 1, 1), c, 0, c.Length);
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
                secondAi.CalculatePosition(backgroundTexture);
                camera.Update(gameTime, playerPosition, playerRectangle);
            }
            else
            {
                clock.checkTime(gameTime);
            }
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch,GraphicsDeviceManager graphics)
        {
            if (!clock.isFinished)
            {
                spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

                spriteBatch.Draw(firstAiTexture, firstAi.position, null, Color.White, firstAi.rotation, firstAiOrigin, 1f, SpriteEffects.None, 0);

                spriteBatch.Draw(secondAiTexture, secondAi.position, null, Color.White, secondAi.rotation, secondAiOrigin, 1f, SpriteEffects.None, 0);

                //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna część obrazka, bez efektów
                spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, playerRotation, playerOrigin, 1f, SpriteEffects.None, 0);

                spriteBatch.DrawString(font2, clock.displayClock, new Vector2(300, 200), Color.Gold);
            }
            else
            {

                spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

                spriteBatch.Draw(firstAiTexture, firstAi.position, null, Color.White, firstAi.rotation, firstAiOrigin, 1f, SpriteEffects.None, 0);

                spriteBatch.Draw(secondAiTexture, secondAi.position, null, Color.White, secondAi.rotation, secondAiOrigin, 1f, SpriteEffects.None, 0);

                //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna część obrazka, bez efektów
                spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, playerRotation, playerOrigin, 1f, SpriteEffects.None, 0);

                //Informacje o przebiegu gry
                Vector2 mapPos = new Vector2(playerPosition.X + 300, playerPosition.Y - 180);

                spriteBatch.Draw(mapTexture, mapPos, Color.White);

                int markX = (mapTexture.Width * (int)playerPosition.X) / backgroundTexture.Width;
                int markY = (mapTexture.Height * (int)playerPosition.Y) / backgroundTexture.Height;
                Vector2 markPos = new Vector2(mapPos.X + markX + 9, mapPos.Y + markY - 1);
                spriteBatch.Draw(mapMark(Color.Blue, graphics), markPos, Color.White);

                markX = (mapTexture.Width * (int)firstAi.position.X) / backgroundTexture.Width;
                markY = (mapTexture.Height * (int)firstAi.position.Y) / backgroundTexture.Height;
                markPos = new Vector2(mapPos.X + markX + 9, mapPos.Y + markY - 1);
                spriteBatch.Draw(mapMark(Color.Red, graphics), markPos, Color.White);

                markX = (mapTexture.Width * (int)secondAi.position.X) / backgroundTexture.Width;
                markY = (mapTexture.Height * (int)secondAi.position.Y) / backgroundTexture.Height;
                markPos = new Vector2(mapPos.X + markX + 9, mapPos.Y + markY - 1);
                spriteBatch.Draw(mapMark(Color.Green, graphics), markPos, Color.White);

                int internalLap = lap;
                if (internalLap < 1)
                {
                    internalLap = 1;
                }
                spriteBatch.DrawString(font, "Okążenie: " + internalLap.ToString() + " z " + maxlap.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 100), Color.White);

                spriteBatch.DrawString(font, "Czas: " + gameTime.TotalGameTime.Minutes.ToString() + ":" + (gameTime.TotalGameTime.Seconds-3).ToString() + "." + gameTime.TotalGameTime.Milliseconds.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 80), Color.White);

                spriteBatch.DrawString(font, "Kierunek: " + playerDestination.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 60), Color.White);

            }
            base.Draw(gameTime);

        }
        private Texture2D mapMark(Color c, GraphicsDeviceManager graphics)
        {
            Texture2D rect = new Texture2D(graphics.GraphicsDevice, 5, 5);

            Color[] data = new Color[25];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = c;
            }
            rect.SetData(data);

            return rect;
        }

    }

}
