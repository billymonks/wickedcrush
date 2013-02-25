using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WickedCrush
{
    //handles the user's input, including whether they are playing with keyboard or gamepad
    public class ControlsManager
    {
        public ControlDevice userDevice;

        private KeyboardState keyState, prevKeyState;
        private MouseState mouseState, prevMouseState;
        private GamePadState padState, prevPadState;

        private PlayerIndex playerIndex = PlayerIndex.One;

        private bool usingPad;

        public Keys jumpKey = Keys.Z;
        public Keys actionKey = Keys.X;
        public Keys blockKey = Keys.C;
        public Keys startKey = Keys.Enter;
        public Keys backKey = Keys.Escape;
        public Keys leftTrigger = Keys.A;
        public Keys rightTrigger = Keys.S;
        public Keys leftBumper = Keys.D;
        public Keys rightBumper = Keys.F;
        public Keys flipKey = Keys.G;

        public ControlsManager()
        {
            usingPad = false;

            padState = new GamePadState();
            prevPadState = padState;

            keyState = new KeyboardState();
            prevKeyState = keyState;

            mouseState = new MouseState();
            prevMouseState = mouseState;
        }
        public void Update(GameTime gameTime)
        {
            if (usingPad)
            {
                prevPadState = padState;
                padState = GamePad.GetState(playerIndex);
            }

            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            mouseState = new MouseState();
            prevMouseState = mouseState;
        }
        public bool checkForGamepad()
        {
            for (PlayerIndex index = PlayerIndex.One; index <= PlayerIndex.Four; index++) //thanks microsoft
            {
                if (GamePad.GetState(index).Buttons.Start == ButtonState.Pressed)
                {
                    playerIndex = index;
                    usingPad = true;
                    userDevice = ControlDevice.Gamepad;
                    return true;
                }
            }
            return false;
        }
        public float XAxis()
        {
            if (usingPad)
            {
                if (Math.Abs(padState.ThumbSticks.Left.X) < 0.2f)
                    return 0f;
                else
                    return padState.ThumbSticks.Left.X;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Left) && !keyState.IsKeyDown(Keys.Right))
                    return -1f;
                else if (keyState.IsKeyDown(Keys.Right) && !keyState.IsKeyDown(Keys.Left))
                    return 1f;
                else
                    return 0f;
            }
        }

        public float YAxis()
        {
            if (usingPad)
            {
                if (Math.Abs(padState.ThumbSticks.Left.Y) < 0.2f)
                    return 0f;
                else
                    return padState.ThumbSticks.Left.Y;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Down) && !keyState.IsKeyDown(Keys.Up))
                    return -1f;
                else if (keyState.IsKeyDown(Keys.Up) && !keyState.IsKeyDown(Keys.Down))
                    return 1f;
                else
                    return 0f;
            }
        }
        public float RStickXAxis()
        {
            if (usingPad)
            {
                if (Math.Abs(padState.ThumbSticks.Right.X) < 0.2f)
                    return 0f;
                else
                    return padState.ThumbSticks.Right.X;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.NumPad4) && !keyState.IsKeyDown(Keys.NumPad6))
                    return -1f;
                else if (keyState.IsKeyDown(Keys.NumPad6) && !keyState.IsKeyDown(Keys.NumPad4))
                    return 1f;
                else
                    return 0f;
            }
        }
        public float RStickYAxis()
        {
            if (usingPad)
            {
                if (Math.Abs(padState.ThumbSticks.Right.Y) < 0.2f)
                    return 0f;
                else
                    return padState.ThumbSticks.Right.Y;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.NumPad2) && !keyState.IsKeyDown(Keys.NumPad8))
                    return -1f;
                else if (keyState.IsKeyDown(Keys.NumPad8) && !keyState.IsKeyDown(Keys.NumPad2))
                    return 1f;
                else
                    return 0f;
            }
        }
        public bool DownPressed()
        {
            if (usingPad)
            {
                if (padState.ThumbSticks.Left.Y < -0.2f && !(prevPadState.ThumbSticks.Left.Y < -0.2f))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))
                    return true;
                else
                    return false;
            }
        }
        public bool UpPressed()
        {
            if (usingPad)
            {
                if (padState.ThumbSticks.Left.Y > 0.2f && !(prevPadState.ThumbSticks.Left.Y > 0.2f))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))
                    return true;
                else
                    return false;
            }
        }
        public bool JumpPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.A) && prevPadState.IsButtonUp(Buttons.A))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(jumpKey) && prevKeyState.IsKeyUp(jumpKey))
                    return true;
                else
                    return false;
            }
        }
        public bool StartPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.Start) && prevPadState.IsButtonUp(Buttons.Start))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(startKey) && prevKeyState.IsKeyUp(startKey))
                    return true;
                else
                    return false;
            }
        }
        public bool BackPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.Back) && prevPadState.IsButtonUp(Buttons.Back))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(backKey) && prevKeyState.IsKeyUp(backKey))
                    return true;
                else
                    return false;
            }
        }
        public bool JumpReleased() //true if jump button not pressed
        {
            if (usingPad)
            {
                if (padState.IsButtonUp(Buttons.A))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyUp(jumpKey))
                    return true;
                else
                    return false;
            }
        }
        public bool BlockHeld()
        {

            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.B))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(blockKey))
                    return true;
                else
                    return false;
            }
        }
        public bool ActionPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.X) && prevPadState.IsButtonUp(Buttons.X))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(actionKey) && prevKeyState.IsKeyUp(actionKey))
                    return true;
                else
                    return false;
            }
        }
        public bool ActionReleased() //true if action button not pressed
        {
            if (usingPad)
            {
                if (padState.IsButtonUp(Buttons.X))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyUp(actionKey))
                    return true;
                else
                    return false;
            }
        }

        public float LeftTrigger()
        {
            if (usingPad)
            {
                return padState.Triggers.Left;
            }
            else
            {
                if (keyState.IsKeyDown(leftTrigger))
                    return 1f;
                else
                    return 0f;
            }
        }
        public float RightTrigger()
        {
            if (usingPad)
            {
                return padState.Triggers.Right;
            }
            else
            {
                if (keyState.IsKeyDown(rightTrigger))
                    return 1f;
                else
                    return 0f;
            }
        }
        public bool LeftBumper()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.LeftShoulder) && prevPadState.IsButtonUp(Buttons.LeftShoulder))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(leftBumper) && prevKeyState.IsKeyUp(leftBumper))
                    return true;
                else
                    return false;
            }
        }
        public bool RightBumper()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.RightShoulder) && prevPadState.IsButtonUp(Buttons.RightShoulder))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(rightBumper) && prevKeyState.IsKeyUp(rightBumper))
                    return true;
                else
                    return false;
            }
        }

        public bool DPadDown()
        {
            if (usingPad)
            {
                if (padState.DPad.Down == ButtonState.Pressed && prevPadState.DPad.Down == ButtonState.Released)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DPadUp()
        {
            if (usingPad)
            {
                if (padState.DPad.Up == ButtonState.Pressed && prevPadState.DPad.Up == ButtonState.Released)
                {
                    return true;
                }
            }
            return false;
        }

        public bool EditorDrawPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.A))
                    return true;
                else
                    return false;
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                    return true;
                else
                    return false;
            }
        }

        public bool ConfirmPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.A) && prevPadState.IsButtonUp(Buttons.A))
                    return true;
                else
                    return false;
            }
            else
            {
                if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    return true;
                else
                    return false;
            }
        }

        public bool FlipPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.Y) && prevPadState.IsButtonUp(Buttons.Y))
                    return true;
                else
                    return false;
            }
            else
            {
                if (keyState.IsKeyDown(flipKey) && prevKeyState.IsKeyUp(flipKey))
                    return true;
                else
                    return false;
            }
        }

        public bool CancelPressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.B) && prevPadState.IsButtonUp(Buttons.B))
                    return true;
                else
                    return false;
            }
            else
            {
                if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
                    return true;
                else
                    return false;
            }
        }

        public Keys[] GetPressedKey()
        {
            List<Keys> temp = new List<Keys>();
            Keys[] pKeys = keyState.GetPressedKeys();
            Keys[] prevKeys = prevKeyState.GetPressedKeys();
            bool ok;

            for (int i = 0; i < pKeys.Length; i++)
            {
                ok = true;
                for (int j = 0; j < prevKeys.Length; j++)
                {
                    if (pKeys[i].Equals(prevKeys[j]))
                        ok = false;
                }
                if (ok)
                    temp.Add(pKeys[i]);
            }

            return (temp.ToArray());
            
        }

        public bool EditorErasePressed()
        {
            if (usingPad)
            {
                if (padState.IsButtonDown(Buttons.B))
                    return true;
                else
                    return false;
            }
            else
            {
                if (mouseState.RightButton == ButtonState.Pressed)
                    return true;
                else
                    return false;
            }
        }
    }
}
