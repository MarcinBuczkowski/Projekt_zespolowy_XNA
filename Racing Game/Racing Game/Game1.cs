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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        ClockTimer clock1 = new ClockTimer();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum GameState{MainMenu,Options,Playing,}

        GameState CurrentGameState = GameState.MainMenu;

        cButton btnPlay;
        cButton btntor2;
        cButton btntor3;

        Race_Window win = new Race_Window();

        Camera camera;

        Texture2D back1, back2, back3;
        Texture2D mini1, mini2, mini3;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;


        }

       
        protected override void Initialize()
        {
            camera = new Camera(GraphicsDevice.Viewport);
            base.Initialize();
        }

        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            graphics.ApplyChanges();
            IsMouseVisible = true;

            btnPlay = new cButton(Content.Load<Texture2D>("tor1mini"), graphics.GraphicsDevice);
            btnPlay.setPosition(new Vector2(60, 60));

            btntor2 = new cButton(Content.Load<Texture2D>("tor2mini"), graphics.GraphicsDevice);
            btntor2.setPosition(new Vector2(60, 220));

            btntor3 = new cButton(Content.Load<Texture2D>("tor3mini"), graphics.GraphicsDevice);
            btntor3.setPosition(new Vector2(60, 380));

            back1 = Content.Load<Texture2D>("background");

            back2 = Content.Load<Texture2D>("tor2");

            back3 = Content.Load<Texture2D>("tor3");

            mini1 = Content.Load<Texture2D>("map");

            mini2 = Content.Load<Texture2D>("map2");

            mini3 = Content.Load<Texture2D>("map3");
        }

       
        protected override void UnloadContent()
        {
            
        }
        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            //Logo US si�  wy�wietla
            if (clock1.isRunning == false)
            {
                clock1.start(3);
            }
            //Koniec wy�wietlania loga mo�na wybra� odpowiedni� tras�  i gra�
            else if (clock1.isFinished==true)
            {
                switch (CurrentGameState)
                {
                    case GameState.MainMenu:
                        if (btnPlay.isClicked == true)
                        {
                            CurrentGameState = GameState.Playing;

                            win.LoadContent(Content, back1, mini1, this);

                            btnPlay.isClicked = false;
                        }
                        btnPlay.Update(mouse);

                        if (btntor2.isClicked == true)
                        {
                            CurrentGameState = GameState.Playing;

                            win.LoadContent(Content, back2, mini2, this);

                            btntor2.isClicked = false;
                        }
                        btntor2.Update(mouse);

                        if (btntor3.isClicked == true)
                        {
                            CurrentGameState = GameState.Playing;

                            win.LoadContent(Content, back3, mini3, this);

                            btntor3.isClicked = false;
                        }
                        btntor3.Update(mouse);

                        break;
                    case GameState.Playing:
                        win.Update(gameTime, camera);
                        break;
                }
            }
            else
            {
                clock1.checkTime(gameTime);
            }
            base.Update(gameTime);
        }

       
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //Wy�wietlanie menu gry przez par� sekund (logo US)
            if (!clock1.isFinished)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Content.Load<Texture2D>("menugry"), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                spriteBatch.End();
            }
            //W�a�ciwe menu gry
            else 
            {
                switch (CurrentGameState)
                {
                    case GameState.MainMenu:
                        spriteBatch.Begin();
                        spriteBatch.Draw(Content.Load<Texture2D>("wyscig"), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);
                        btnPlay.Draw(spriteBatch);
                        btntor2.Draw(spriteBatch);
                        btntor3.Draw(spriteBatch);
                        spriteBatch.End();
                        break;
                    case GameState.Playing:
                        spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    null, null, null, null,
                    camera.transform);
                        win.Draw(gameTime, spriteBatch, graphics);
                        spriteBatch.End();
                        break;
                }
            }
            base.Draw(gameTime);
        }

        public void ReturnToMenu()
        {
            CurrentGameState = GameState.MainMenu;
            win.Exit();
            win.Dispose();
            win = new Race_Window();
        }
    }
}
