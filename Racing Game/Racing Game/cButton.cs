using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Racing_Game
{
    class cButton
    {
        Texture2D texture;
        Vector2 position;
        Rectangle rectangle;

        //Ustawienie białego koloru
        Color colour = new Color(255, 255, 255, 255);

        public Vector2 size;

        public cButton(Texture2D newTexture, GraphicsDevice graphics)
        {
            texture = newTexture;
            //ScreenW = 800, ScreenH = 600
            // ImgW = 100, IngH = 20
            //Ustalenie wielkości stworzonego przycisku
            size = new Vector2(graphics.Viewport.Width / 4, graphics.Viewport.Height / 4);
        }

        bool down;
        public bool isClicked;
        //Obsługa zdarzeń myszy
        public void Update(MouseState mouse)
        {
            //Umożliwienie przesówaia kursora względem osi x i y
            rectangle = new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            Rectangle mouseRectangle = new Rectangle(mouse.X, mouse.Y, 1, 1);

            //Event, który tyczy się momentu gdy użytkownik najedzie kursorem na przycisk
            if (mouseRectangle.Intersects(rectangle))
            {
                if (colour.A == 255) down = false;
                if (colour.A == 0) down = true;
                //Przedział zmiany intensywności koloru po najechaniu kursorem na przycisk
                if (down) colour.A += 3; else colour.A -= 3;
                if (mouse.LeftButton == ButtonState.Pressed) isClicked = true;
            }
            //Przejście kolorów przycisku gdy nie jest wciśnięty
            else if (colour.A < 255)
            {
                colour.A += 3;
                isClicked = false;
            }
        }

        //Pozycja
        public void setPosition(Vector2 newPosition)
        {
            position = newPosition;
        }

        //Rysowanie
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, colour);
        }

    }
}
