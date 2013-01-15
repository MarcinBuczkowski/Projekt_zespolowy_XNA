using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Racing_Game
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

        public void Update(GameTime gameTime, Vector2 playerPosition, Rectangle playerRectangle)
        {
            //Powiązanie kamery z samochodem
            centre = new Vector2(playerPosition.X + (playerRectangle.Width / 2) - 400, 0);
            centre = new Vector2(playerPosition.X + (playerRectangle.Width / 2) - 400, playerPosition.Y + (playerRectangle.Height / 2 - 250));
            transform = Matrix.CreateScale(new Vector3(1, 1, 0)) *
                Matrix.CreateTranslation(new Vector3(-centre.X, -centre.Y, 0));
        }

    }
}
