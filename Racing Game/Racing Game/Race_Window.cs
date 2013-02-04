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
    //Klasa odpowiedzialna za wyścig
    class Race_Window : Microsoft.Xna.Framework.Game
    {
        ClockTimer clock = new ClockTimer();

        // To tekstura, którą możemy wyrenderować
        private Texture2D playerTexture;
        public Rectangle playerRectangle;
        //Możliwe kierunki ruchu 
        private enum Dest { North, East, South, West };
        private Dest playerDestination;

        //Obiekt typu AI i jego położenie na trasie
        private AI firstAi = new AI(new Vector2(350, 211), 5f);
        //Colory dla tekstury pierwszego AI
        private Color[] firstAiTextureData;
        //Centralna część tekstury pierwszego AI
        private Vector2 firstAiOrigin;
        //Tekstura pierwszego AI
        private Texture2D firstAiTexture;
        //Prostokąt w którym jest umieszczona tekstura auta pierwszego AI
        private Rectangle firstAiRectangle;

        //Obiekt typu AI i jego położenie na trasie
        private AI secondAi = new AI(new Vector2(350, 377), 4f);
        //Colory dla tekstury drugiego AI
        private Color[] secondAiTextureData;
        //Centralna część tekstury drugiego AI
        private Vector2 secondAiOrigin;
        //Tekstura drugiego AI
        private Texture2D secondAiTexture;
        //Prostokąt w którym jest umieszczona tekstura auta drugiego AI
        private Rectangle secondAiRectangle;

        //Prostokąt w którym jest mapa
        private Rectangle mapRectangle;
        //Colory tekstury gracza
        private Color[] playerTextureData;
        //Colory tekstury mapy
        private Color[] mapTextureData;

        //Centralna część obrazka
        private Vector2 playerOrigin;
        //Pozycja gracza na trasie
        public Vector2 playerPosition;
        //Kąt obrotu samochodu gracza
        private float playerRotation;
        //Prędkość gracza
        private Vector2 playerVelocity;

        //Ustalenie prędkości pojazdu
        private const float playerTangentialVelocity = 6f;
        //Ustalenie długości poślizgu do zatrzymania
        private float friction = 0.1f;

        //Obiekt typu kamera śledzący samochód gracza
        private Camera camera;
        //Tekstura trasy
        private Texture2D backgroundTexture;
        //Położenie trasy na ekranie
        private Vector2 backgroundPosition;
        //Tekstura mapy
        private Texture2D mapTexture;

        private Boolean playerCollision = false;

        //Czcionki wyświetlające odliczanie,czas,ilość okrążeń
        private SpriteFont font;
        private SpriteFont font2;
        private SpriteFont font3;

        //Ilość okrążeń przejechanych i maksymalna ich liczba
        private int lap = 0;
        private int maxlap = 3;
        private bool lapChange = false;
        private DateTime lastLapChange = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));

        Game1 _parent;

        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }
        //Ładowanie odpowiednich grafik
        public void LoadContent(ContentManager Content, Texture2D tex1, Texture2D tex2, Game1 parent)
        {
            this._parent = parent;

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

            //Pobranie kolorów mapy
            mapTextureData = new Color[backgroundTexture.Width * backgroundTexture.Height];
            backgroundTexture.GetData(mapTextureData);

            //Pobranie kolorów auta gracza
            playerTextureData = new Color[playerTexture.Width * playerTexture.Height];
            playerTexture.GetData(playerTextureData);

            //Pobranie kolorów auta pierwszego AI
            firstAiTextureData = new Color[firstAiTexture.Width * firstAiTexture.Height];
            firstAiTexture.GetData(firstAiTextureData);

            //Pobranie kolorów auta drugiego AI
            secondAiTextureData = new Color[secondAiTexture.Width * secondAiTexture.Height];
            secondAiTexture.GetData(secondAiTextureData);

            //Przypisanie odpowiednich czcionek
            font = Content.Load<SpriteFont>("SpriteFont");
            font2 = Content.Load<SpriteFont>("SpriteFont2");
            font3 = Content.Load<SpriteFont>("SpriteFont3");
        }

        //Metoda odpowiedzialna za kolizję, obroty i sam wyścig
        public void Update(GameTime gameTime, Camera camera)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            //Odliczanie rozpoczęte
            if (clock.isRunning == false)
            {
                clock.start(3);
                //Gdy zegar odlicza porusznie zablokowane

                //Odpowiednie położenie mapy
                Matrix mapTransform = Matrix.CreateTranslation(new Vector3(backgroundPosition, 0.0f));

                //Odpowiednie ustawienie pierwsego AI
                Matrix firstAiTransform = Matrix.CreateTranslation(new Vector3(-firstAiOrigin, 0.0f)) *
                                          Matrix.CreateRotationZ(firstAi.rotation) *
                                          Matrix.CreateTranslation(new Vector3(firstAi.position, 0.0f));
                //Odpowiednie ustawienie drugiego AI
                Matrix secondAiTransform = Matrix.CreateTranslation(new Vector3(-secondAiOrigin, 0.0f)) *
                                           Matrix.CreateRotationZ(secondAi.rotation) *
                                           Matrix.CreateTranslation(new Vector3(secondAi.position, 0.0f));
                //Odpowiednie ustawienie gracza
                Matrix playerTransform = Matrix.CreateTranslation(new Vector3(-playerOrigin, 0.0f)) *
                                         Matrix.CreateRotationZ(playerRotation) *
                                         Matrix.CreateTranslation(new Vector3(playerPosition, 0.0f));
                //Wyliczenie odpowiednie położenie prostokąta w którym jest umieszczona tekstura
                playerRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, playerTexture.Width, playerTexture.Height), playerTransform);

                //Centralna część tekstury gracza
                playerOrigin = new Vector2(playerRectangle.Width / 2, playerRectangle.Height / 2);

                //Wyliczenie odpowiednie położenie prostokąta w którym jest umieszczona tekstura dla pierszego AI
                firstAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, firstAiTexture.Width, firstAiTexture.Height), firstAiTransform);

                //Centralna część tekstury pierwszego AI
                firstAiOrigin = new Vector2(firstAiRectangle.Width / 2, firstAiRectangle.Height / 2);

                //Wyliczenie odpowiednie położenie prostokąta w którym jest umieszczona tekstura dla drugiego AI
                secondAiRectangle = Collisions.CalculateBoundingRectangle(new Rectangle(0, 0, secondAiTexture.Width, secondAiTexture.Height), secondAiTransform);

                //Centralna część tekstury pierwszego AI
                secondAiOrigin = new Vector2(secondAiRectangle.Width / 2, secondAiRectangle.Height / 2);

                //Prostokąt dla mapy
                mapRectangle = new Rectangle((int)backgroundPosition.X, (int)backgroundPosition.Y, backgroundTexture.Width, backgroundTexture.Height);

                firstAi.CalculatePosition(backgroundTexture);
                secondAi.CalculatePosition(backgroundTexture);
                camera.Update(gameTime, playerPosition, playerRectangle);

            }
            else if (lap > maxlap || firstAi.lap > maxlap || secondAi.lap > maxlap)
            {
                ; // Nie robimy nic, gdy wyścig został wygrany
            }
            else if (clock.isFinished == true)
            {
                //Gdy zegar skończy odliczać start wyścigu
                //Cześć zmiennych jest taka sama jak powyżej ponieważ odpowiada tylko za przedstawienie mapy, gracza, AI podczas odliczania do rozpoczęcia
                // wyścigu
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

                //Dalsza część 
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

                // Kolizja z przeciwnikiem za pomocą metody Per Piksel Collision
                if (firstAiRectangle.Intersects(playerRectangle))
                {
                    //Jeśli zaszła kolizja
                    if (Collisions.IntersectPixels(firstAiTransform, firstAiTexture.Width, firstAiTexture.Height, firstAiTextureData, playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData))
                    {
                        //Odbicie od przeciwnika po współrzędnej X i Y pod odpowiednim kątem
                        playerVelocity.X = -playerVelocity.X;
                        playerVelocity.Y = -playerVelocity.Y;
                        this.playerCollision = true;
                    }
                }
                if (secondAiRectangle.Intersects(playerRectangle))
                {
                    //Jeśli zaszła kolizja
                    if (Collisions.IntersectPixels(secondAiTransform, secondAiTexture.Width, secondAiTexture.Height, secondAiTextureData, playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData))
                    {
                        //Odbicie od przeciwnika po współrzędnej X i Y pod odpowiednim kątem
                        playerVelocity.X = -playerVelocity.X;
                        playerVelocity.Y = -playerVelocity.Y;
                        this.playerCollision = true;
                    }
                }
                //Kolizja przeciwnika pierwszego z autem gracza 
                if (playerRectangle.Intersects(firstAiRectangle))
                {
                    //Jeśli zaszła kolizja
                    if (Collisions.IntersectPixels(playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData, firstAiTransform, firstAiTexture.Width, firstAiTexture.Height, firstAiTextureData))
                    {
                        //Wykrywanie kierunku w jakim porusza się gracz i wzależności od tego odbicie od drugiego pojazdu po odpowiedniej współrzędnej
                        if (playerDestination == Dest.East)
                        {
                            //Jak jedziemy w kierunku wchodnim zmiana następuje po współrzędnej X
                            float tmp = (playerVelocity.X + (float)1);
                            playerVelocity.X = (playerVelocity.X + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.South)
                        {
                            //Jak jedziemy w kierunku południowym zmiana nastepuje po współrzędnej Y
                            float tmp = (playerVelocity.Y - (float)1);
                            playerVelocity.Y = (playerVelocity.Y + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.West)
                        {
                            //Jak jedziemy w kierunku zachodnim zmiana nastepuje po współrzędnej X
                            float tmp = (playerVelocity.X - (float)1);
                            playerVelocity.X = (playerVelocity.X + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.North)
                        {
                            //Jak jedziemy w kierunku północnym zmiana nastepuje w po współrzędnej Y
                            float tmp = (playerVelocity.Y + (float)1);
                            playerVelocity.Y = (playerVelocity.Y + tmp);
                            this.playerCollision = true;
                        }
                    }
                }

                //Kolizja przeciwnika drugiego z autem gracza 
                if (playerRectangle.Intersects(secondAiRectangle))
                {
                    //Jeśli kolizja jest wykryta
                    if (Collisions.IntersectPixels(playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData, secondAiTransform, secondAiTexture.Width, secondAiTexture.Height, secondAiTextureData))
                    {
                        //Wykrywanie kierunku w jakim porusza się gracz i wzależności od tego odbicie od drugiego pojazdu po odpowiedniej współrzędnej
                        if (playerDestination == Dest.East)
                        {
                            //Jak jedziemy w kierunku wchodnim zmiana następuje po współrzędnej X
                            float tmp = (playerVelocity.X + (float)1);
                            playerVelocity.X = (playerVelocity.X + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.South)
                        {
                            //Jak jedziemy w kierunku południowym zmiana nastepuje po współrzędnej Y
                            float tmp = (playerVelocity.Y - (float)1);
                            playerVelocity.Y = (playerVelocity.Y + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.West)
                        {
                            //Jak jedziemy w kierunku zachodnim zmiana nastepuje po współrzędnej X
                            float tmp = (playerVelocity.X - (float)1);
                            playerVelocity.X = (playerVelocity.X + tmp);
                            this.playerCollision = true;
                        }
                        if (playerDestination == Dest.North)
                        {
                            //Jak jedziemy w kierunku północnym zmiana nastepuje w po współrzędnej Y
                            float tmp = (playerVelocity.Y + (float)1);
                            playerVelocity.Y = (playerVelocity.Y + tmp);
                            this.playerCollision = true;
                        }
                    }
                }

                //Kolizja auta gracza z bandami
                if (Collisions.Intersect(playerTransform, playerTexture.Width, playerTexture.Height, playerTextureData, mapTransform, backgroundTexture.Width, backgroundTexture.Height, mapTextureData))
                {
                    //Odbicie pod odpowiednim kątem po współrzędnych X i Y
                    playerVelocity.X = -playerVelocity.X;
                    playerVelocity.Y = -playerVelocity.Y;
                    this.playerCollision = true;
                }
                //Wykrywanie kierunku poruszania sięsamochodu
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

        //wyświetlanie wyścigu
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            //Gdy odlicznianie się nie skończyło wyświetla tylko auta i trasę
            if (!clock.isFinished || lap == maxlap || firstAi.lap == maxlap || secondAi.lap == maxlap)
            {
                //trasa
                spriteBatch.Draw(backgroundTexture, backgroundPosition, Color.White);

                //Pierwszy AI
                spriteBatch.Draw(firstAiTexture, firstAi.position, null, Color.White, firstAi.rotation, firstAiOrigin, 1f, SpriteEffects.None, 0);

                //Drugi AI
                spriteBatch.Draw(secondAiTexture, secondAi.position, null, Color.White, secondAi.rotation, secondAiOrigin, 1f, SpriteEffects.None, 0);

                //Rysowanie tekstury i ustawianie koloru na transparentny, rotacji, centralna część obrazka, bez efektów
                spriteBatch.Draw(playerTexture, playerPosition, null, Color.White, playerRotation, playerOrigin, 1f, SpriteEffects.None, 0);

                if (!clock.isFinished)
                {
                    spriteBatch.DrawString(font2, clock.displayClock, new Vector2(300, 200), Color.Gold);
                }
                else if (lap > maxlap)
                {
                    spriteBatch.DrawString(font3, "Wygrałeś!", new Vector2(250, 320), Color.Gold);
                    if (DateTime.Now.Subtract(lastLapChange) > TimeSpan.FromSeconds(5))
                    {
                        this._parent.ReturnToMenu();
                    }
                }
                else
                {
                    spriteBatch.DrawString(font3, "Przegrałeś.", new Vector2(250, 320), Color.Gold);
                    if (DateTime.Now.Subtract(lastLapChange) > TimeSpan.FromSeconds(5))
                    {
                        this._parent.ReturnToMenu();
                    }
                }
            }
            else
            {
                //Odliczanie się skończyło wyświetlanie aut, trasy, minimapy, czasu
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

                spriteBatch.DrawString(font, "Czas: " + gameTime.TotalGameTime.Minutes.ToString() + ":" + (gameTime.TotalGameTime.Seconds - 3).ToString() + "." + gameTime.TotalGameTime.Milliseconds.ToString(), new Vector2(playerPosition.X + 300, playerPosition.Y - 80), Color.White);

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