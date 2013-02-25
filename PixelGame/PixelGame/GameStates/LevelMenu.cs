using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.IO;
using WickedCrush.Utility;

namespace WickedCrush.GameStates
{
    class LevelMenu
    {
        private SpriteBatch sb;

        GraphicsDevice _graphics;
        ContentManager _content;
        Stack<GameState> _cGameState;

        Overlord _overlord;
        ControlsManager _controls;
        Gameplay _game;

        Matrix scaleMatrix;

        List<String> levelList;
        int selectionIndex = 0;

        SpriteFont font;


        public LevelMenu(Overlord o, ControlsManager controls)
        {
            //_graphics = gd;
            //_content = c;
            //_cGameState = cGameState;
            _controls = controls;
            _overlord = o;
            //_game = game;

            this.Initialize();
        }

        public void Initialize()
        {
            sb = new SpriteBatch(_overlord._gd);
            font = _overlord._cm.Load<SpriteFont>(@"fonts/PreAlphaFont");
            LoadLevelList();
        }

        public void Update(GameTime gameTime)
        {

            if (_controls.DownPressed())
            {
                selectionIndex++;
                if (selectionIndex > levelList.Count-1)
                    selectionIndex = 0;
            }

            if (_controls.UpPressed())
            {
                selectionIndex--;
                if (selectionIndex < 0)
                    selectionIndex = levelList.Count-1;
            }

            if (_controls.JumpPressed() && levelList!=null && levelList[selectionIndex]!=null)
            {
                //if (_game.currentLevel != null)
                //{
                    //_game.currentLevel = new Level(_content, _graphics);
                    //_game.currentLevel.hero._controls = _controls;
                    //_game.currentLevel.levelCam._controls = _controls;
                //}
                _overlord.LoadLevel(levelList[selectionIndex]);
                //_game.currentLevel.loadLevel(levelList[selectionIndex], _content, _graphics, _cGameState);
            }

            if (_controls.ActionPressed())
            {
                _overlord.LaunchEditor();
            }

            if (_controls.LeftBumper())
                LoadLevelList();

            //_controls.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            _overlord._gd.Clear(Color.Black);

            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);
            if(levelList!=null)
                for (int i = 0; i < levelList.Count; i++)
                {
                    if(i==selectionIndex)
                        sb.DrawString(font, levelList[i], new Vector2(15f, 10f + i * 30f), Color.Yellow);
                    else
                        sb.DrawString(font, levelList[i], new Vector2(15f, 10f + i * 30f), Color.White);
                }
            sb.End();
        }
        public void LoadLevelList()
        {

            string[] files = System.IO.Directory.GetFiles(@"Content/levels", "*", SearchOption.AllDirectories);
            selectionIndex = 0;
            levelList = new List<String>();

            for (int i = 0; i < files.Length; i++)
            {
                levelList.Add(files[i]);
            }
        }
        public void updateScaleMatrix(Matrix sm)
        {
            scaleMatrix = sm;
        }
    }
}
