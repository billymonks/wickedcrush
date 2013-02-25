using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace WickedCrush
{
    public class Menu
    {
        private List<MenuOption> menuList = new List<MenuOption>();
        private SpriteBatch sb;

        GraphicsDevice _graphics;
        ContentManager _content;
        Stack<GameState> _cGameState;
        ControlsManager _controls;

        Matrix scaleMatrix;


        public Menu(GraphicsDevice gd, ContentManager c, Stack<GameState> cGameState, ControlsManager controls)
        {
            _graphics = gd;
            _content = c;
            _cGameState = cGameState;
            _controls = controls;

            menuList = new List<MenuOption>();
            this.Initialize();
        }

        public void Initialize()
        {
            sb = new SpriteBatch(_graphics);
        }

        public void Update(GameTime gameTime)
        {
            if(_controls.checkForGamepad())
                _cGameState.Push(GameState.InGame);

            if(_controls.JumpPressed())
                _cGameState.Push(GameState.InGame);

            //_controls.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);
            
            foreach (MenuOption o in menuList)
            {
                sb.Draw(o.getTexture(), o.getPos(), Color.White);
            }

            sb.End();
        }
        public void AddToMenu(MenuOption option)
        {
            menuList.Add(option);
        }
        public void updateScaleMatrix(Matrix sm)
        {
            scaleMatrix = sm;
        }
    }
}
