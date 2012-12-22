using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projekt_zespolowy_XNA
{
    class Camera
    {
        public Matrix transform;
        Viewport view;
        Vector2 centre;

        public Camera(Viewport newView)
        {
            view = newView;
        }

        public void Update(GameTime gameTime, Game1 auto)
        {
            //Powiązanie kamery z samochodem
            centre = new Vector2(auto.spritePosition.X + (auto.spriteRectangle.Width / 2) - 400, 0);
            centre = new Vector2(auto.spritePosition.X + (auto.spriteRectangle.Width / 2) - 400, auto.spritePosition.Y + (auto.spriteRectangle.Height / 2 - 250));
            transform = Matrix.CreateScale(new Vector3(1, 1, 0)) *
                Matrix.CreateTranslation(new Vector3(-centre.X, -centre.Y, 0));
        }
    }
}