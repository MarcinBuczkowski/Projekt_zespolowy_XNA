using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace Racing_Game
{
    class Collisions
    {
        //Wyznaczenie prostokąta dla samochodów
        public static Rectangle CalculateBoundingRectangle(Rectangle rectangle,
                                                         Matrix transform)
        {
            // Pobranie narożników sprite'a
            Vector2 leftTop = new Vector2(rectangle.Left, rectangle.Top);
            Vector2 rightTop = new Vector2(rectangle.Right, rectangle.Top);
            Vector2 leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
            Vector2 rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

            // Transformacja narożników 
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            // Znalezienie minimum i maksimum 
            Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Zwracanie nowego prostokąta
            return new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        //Algorytm wykrywania kolizji między samochodami
        public static bool IntersectPixels(
                            Matrix transformA, int widthA, int heightA, Color[] dataA,
                            Matrix transformB, int widthB, int heightB, Color[] dataB)
        {
            // Transformowanie sprite'a A w współrzędne sprite'a B
            Matrix transformAToB = transformA * Matrix.Invert(transformB);

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Znalezienie lewego górnego wierzchołka sprite'a
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            // Sprawdzanie pikseli
            for (int yA = 0; yA < heightA; yA++)
            {
                // Punkt startowy 
                Vector2 posInB = yPosInB;

                for (int xA = 0; xA < widthA; xA++)
                {
                    // Szukanie najbliższego piksela
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        // Pobranie kolorów pikseli
                        Color colorA = dataA[xA + yA * widthA];
                        Color colorB = dataB[xB + yB * widthB];

                        // Sprawdzanie warunku zajścia kolizji
                        if (colorA.A != 0 && colorB.A != 0)
                        {
                            
                            return true;
                        }
                    }

                    // Przejscie do następnego kroku
                    posInB += stepX;
                }

                // To samo tylko że dla współrzędnej Y
                yPosInB += stepY;
            }
            return false;
        }

        

        //Sprawdzanie kolizji samochodu z bandą
        //Oparte o tą samą zasadę jak kolizja pomiędzy autami
        public static bool Intersect(
                            Matrix transformA, int widthA, int heightA, Color[] dataA,
                            Matrix transformB, int widthB, int heightB, Color[] dataB)
        {
            
            Matrix transformAToB = transformA * Matrix.Invert(transformB);

            
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

           
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

          
            for (int yA = 0; yA < heightA; yA++)
            {
                
                Vector2 posInB = yPosInB;

                
                for (int xA = 0; xA < widthA; xA++)
                {
                    
                    int xB = (int)Math.Round(posInB.X);
                    int yB = (int)Math.Round(posInB.Y);

                    
                    if (0 <= xB && xB < widthB &&
                        0 <= yB && yB < heightB)
                    {
                        
                        Color colorA = dataA[xA + yA * widthA];
                        Color colorB = dataB[xB + yB * widthB];

                        //Sprawdzanie czy piksele mają inny kolor niż powinny jeśli tak to mamy kolizję
                        if (colorB.R == 0 && colorB.G == 152 && colorB.B == 70)
                        {
                            
                            return true;
                        }
                    }

                    
                    posInB += stepX;

                }

                
                yPosInB += stepY;

            }
            
            return false;
        }


    }
}
