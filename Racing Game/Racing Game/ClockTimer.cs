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

namespace Racing_Game
{
    class ClockTimer
    {
        private int endTimer;
        private int countTimerRef;
        public String displayClock { get; private set; }
        //Zwraca prawdę lub fałsz czy rozpoczęło się odliczanie
        public bool isRunning { get; private set; }
        //Zwraca prawdę lub fałsz czy zakończyło się odliczanie
        public bool isFinished { get; private set; }

        //Ustalenie wyświetlania odliczania
        public ClockTimer()
        {

            displayClock = "";
            endTimer = 0;
            countTimerRef = 0;
            isRunning = false;
            isFinished = false;

        }
        //Start odliczania
        public void start(int seconds)
        {
            //Odliczamy w sekundach
            endTimer = seconds;
            isRunning = true;
            displayClock = endTimer.ToString();
        }

        //Sprawdzenie odliczanego czasu
        public Boolean checkTime(GameTime gameTime)
        {
            countTimerRef += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (!isFinished)
            {
                if (countTimerRef >= 1000.0f)
                {
                    endTimer = endTimer - 1;
                    displayClock = endTimer.ToString();
                    countTimerRef = 0;

                    //Gdy timer odliczy do zera to koniec pracy timera
                    if (endTimer <= 0)
                    {
                        endTimer = 0;
                        isFinished = true;
                        displayClock = "Game Over";
                    }
                }
            }
            else
            {

                displayClock = "Game Over";
            }
            return isFinished;
        }
        //Reset timera
        public void reset()
        {
            isRunning = false;
            isFinished = false;
            displayClock = "None";
            countTimerRef = 0;
            endTimer = 0;
        }

    }
}
