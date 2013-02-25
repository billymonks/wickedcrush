using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush.Utility
{
    public class DamageNumber
    {
        public int number;
        public Vector2 inGamePos;
        private TimeSpan interval, currentTime;

        public bool readyForRemoval = false;

        public DamageNumber(int number, Vector2 inGamePos)
        {
            this.number = number;
            this.inGamePos = inGamePos;

            interval = new TimeSpan(0, 0, 0, 1);
            currentTime = TimeSpan.Zero;
        }

        public void Update(GameTime gameTime)
        {
            TimeSpan elapsed = gameTime.ElapsedGameTime;

            currentTime+=elapsed;

            inGamePos.Y += ((float)elapsed.TotalMilliseconds / 1000.0f) * 60.0f;

            if (currentTime.CompareTo(interval) >= 0)
                readyForRemoval = true;
        }
    }
}
