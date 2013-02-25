using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WickedCrush.GameStates
{
    public class SplashScreen
    {
        #region fields
        GraphicsDevice _graphics;
        ContentManager _content;
        Stack<GameState> _cGameState;
        ControlsManager _controls;

        Matrix scaleMatrix;

        SpriteFont font;

        private SpriteBatch sb;

        #endregion

        public SplashScreen(GraphicsDevice gd, ContentManager c, Stack<GameState> cGameState, ControlsManager controls)
        {
            _graphics = gd;
            _content = c;
            _cGameState = cGameState;
            _controls = controls;

            this.Initialize();
        }

        public void Initialize()
        {
            sb = new SpriteBatch(_graphics);
            font = _content.Load<SpriteFont>(@"fonts/PreAlphaFont");
        }

        public void Update(GameTime gameTime)
        {
            if (_controls.checkForGamepad())
                _cGameState.Push(GameState.MainMenu);
            else if (_controls.StartPressed())
                _cGameState.Push(GameState.MainMenu);

            /*if (_controls.DownPressed())
            {
                selectionIndex++;
                if (selectionIndex > modeList.Count - 1)
                    selectionIndex = 0;
            }
            if (_controls.UpPressed())
            {
                selectionIndex--;
                if (selectionIndex < 0)
                    selectionIndex = modeList.Count - 1;
            }*/

            //_controls.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            _graphics.Clear(Color.Azure);
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);
            sb.DrawString(font, "Wicked Crush\nPress Enter or Start", new Vector2(0f, 0f), Color.Black);
            sb.End();
        }

        public void updateScaleMatrix(Matrix sm)
        {
            scaleMatrix = sm;
        }
    }
}
