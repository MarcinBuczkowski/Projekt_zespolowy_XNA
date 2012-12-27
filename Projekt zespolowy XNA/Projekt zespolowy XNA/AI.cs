using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projekt_zespolowy_XNA
{
    class AI
    {
        private Vector2 _position;

        private enum Dest { North, East, South, West };
        private Dest destination;

        private bool rotatefirstAiLeft = false;
        private bool rotatefirstAiRight = false;

        private Color reference;

        private Vector2 _velocity;
        private const float tangentialVelocity = 3f;

        private float _rotation;

        public Vector2 position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
            }
        }

        public Vector2 velocity
        {
            get
            {
                return this._velocity;
            }
            set
            {
                this._velocity = value;
            }
        }

        public float rotation
        {
            get
            {
                return this._rotation;
            }
            set
            {
                this._rotation = value;
            }
        }

        public AI()
        {
            this._position = new Vector2(600, 225);
            this.reference = new Color(114, 114, 113, 255);
            this._velocity = new Vector2(3f, 0f);
            this.destination = Dest.East;
        }

        public void CalculatePosition(Texture2D background)
        {
            Color[] east_n = new Color[] { Color.Transparent };
            Color[] east_f = new Color[] { Color.Transparent };
            Color[] west_n = new Color[] { Color.Transparent };
            Color[] west_f = new Color[] { Color.Transparent };
            Color[] north_n = new Color[] { Color.Transparent };
            Color[] north_f = new Color[] { Color.Transparent };
            Color[] south_n = new Color[] { Color.Transparent };
            Color[] south_f = new Color[] { Color.Transparent };
            if (!rotatefirstAiLeft && !rotatefirstAiRight)
            {
                east_n = getBackgroundColor(550, 0, position, background);
                east_f = getBackgroundColor(610, 0, position, background);
                west_n = getBackgroundColor(250, 0, position, background);
                west_f = getBackgroundColor(190, 0, position, background);
                north_n = getBackgroundColor(400, -150, position, background);
                north_f = getBackgroundColor(400, -210, position, background);
                south_n = getBackgroundColor(400, 150, position, background);
                south_f = getBackgroundColor(400, 210, position, background);
            }

            if (!rotatefirstAiLeft && !rotatefirstAiRight)
            {
                Color[] check1_n = new Color[] { Color.Transparent };
                Color[] check1_f = new Color[] { Color.Transparent };
                Color[] check2_n = new Color[] { Color.Transparent };
                Color[] check2_f = new Color[] { Color.Transparent };
                if (destination == Dest.East && east_n[0] != reference)
                {
                    check1_n = north_n;
                    check1_f = north_f;
                    check2_n = south_n;
                    check2_f = south_f;
                }
                else if (destination == Dest.West && west_n[0] != reference)
                {
                    check1_n = south_n;
                    check1_f = south_f;
                    check2_n = north_n;
                    check2_f = north_f;
                }
                else if (destination == Dest.North && north_n[0] != reference)
                {
                    check1_n = west_n;
                    check1_f = west_f;
                    check2_n = east_n;
                    check2_f = east_f;
                }
                else if (destination == Dest.South && south_n[0] != reference)
                {
                    check1_n = east_n;
                    check1_f = east_f;
                    check2_n = west_n;
                    check2_f = west_f;
                }

                if (check1_n[0] != Color.Transparent)
                {
                    if (check1_n[0] == check2_n[0])
                    {
                        if (check1_f[0] == reference)
                        {
                            rotatefirstAiLeft = true;
                        }
                        else if (check2_f[0] == reference)
                        {
                            rotatefirstAiRight = true;
                        }
                    }
                    else
                    {
                        if (check1_n[0] == reference)
                        {
                            rotatefirstAiLeft = true;
                        }
                        else if (check2_n[0] == reference)
                        {
                            rotatefirstAiRight = true;
                        }
                    }
                }
            }

            if (rotatefirstAiLeft || rotatefirstAiRight)
            {
                if (rotatefirstAiLeft)
                {
                    rotation -= (float)(0.01 * 3.14);
                }
                else
                {
                    rotation += (float)(0.01 * 3.14);
                }
                float round = (float)Math.Round(rotation, 4);
                float sround = (float)(Math.Round(Math.Sin(round), 4));
                float cround = (float)(Math.Round(Math.Cos(round), 4));
                if (((destination == Dest.East || destination == Dest.West) && (sround == 0 || sround == 1 || sround == -1))
                    ||
                    ((destination == Dest.North || destination == Dest.South) && (cround == 0 || cround == 1 || cround == -1)))
                {
                    if (sround == 1)
                    {
                        destination = Dest.South;
                        _velocity.X = 0;
                        _velocity.Y = 3;
                        rotation = 1.57f;
                    }
                    else if (sround == -1)
                    {
                        destination = Dest.North;
                        _velocity.X = 0;
                        _velocity.Y = -3;
                        rotation = 4.71f;
                    }
                    else
                    {
                        if (cround == 1)
                        {
                            destination = Dest.East;
                            _velocity.X = 3;
                            _velocity.Y = 0;
                            rotation = 0f;
                        }
                        else if (cround == -1)
                        {
                            destination = Dest.West;
                            _velocity.X = -3;
                            _velocity.Y = 0;
                            rotation = 3.14f;
                        }
                    }

                    rotatefirstAiLeft = false;
                    rotatefirstAiRight = false;
                }
                else
                {
                    _velocity.X = (float)Math.Cos(rotation) * tangentialVelocity;
                    _velocity.Y = (float)Math.Sin(rotation) * tangentialVelocity;
                }
            }

            this._position += this._velocity;
        }

        private Color[] getBackgroundColor(int x, int y, Vector2 position, Texture2D background)
        {
            Rectangle rect = new Rectangle((int)position.X + x, (int)position.Y + y, 1, 1);
            Color[] c = new Color[1];
            if (rect.X > background.Width || rect.X < 0 || rect.Y > background.Height || rect.Y < 0)
            {
                c[0] = Color.Transparent;
            }
            else
            {
                try
                {
                    background.GetData(0, rect, c, 0, c.Length);
                }
                catch (Exception ex)
                {
                    c[0] = Color.Transparent;
                }
            }
            return c;
        }
    }
}
