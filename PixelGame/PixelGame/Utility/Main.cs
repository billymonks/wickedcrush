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
using WickedCrush.Utility;
using WickedCrush.GameStates;

namespace WickedCrush
{
    public enum GameState
    {
        OpeningLogos = 1,
        SplashScreen = 2,
        MainMenu = 3,
        InGame = 4,
        PauseMenu = 5,
        Editor = 6,
        TempMenu = 7
    }

    public enum ControlDevice
    {
        Keyboard = 0,
        Gamepad = 1
    }

    public class Main : Microsoft.Xna.Framework.Game
    {
        #region fields
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Stack<GameState> cGameState = new Stack<GameState>();

        Matrix scaleMatrix;

        Vector2 baseSize;
        Vector3 scale;

        SplashScreen splashScreen;
        Menu mainMenu;
        Gameplay gameEngine;
        Editor editor;

        LevelMenu tempMenu;

        Texture2D scaleTest;

        ControlsManager controls;
        SoundManager sound;

        Overlord overlord;

        bool paused = false;

        bool fullscreen = false;
        #endregion

        public Main()
        {
            IsFixedTimeStep = false;

            graphics = new GraphicsDeviceManager(this);
            Content = new SynchronizedContentManager(this.Services);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            //IsMouseVisible = true;

            base.Window.Title = "Wicked Crush";

            baseSize = new Vector2(1920, 1080);

            graphics.SynchronizeWithVerticalRetrace = true; //use to test performance somewhat
            graphics.PreferMultiSampling = true;

            if (fullscreen)
            {
                graphics.IsFullScreen = true;
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.ApplyChanges();
            }
            else
            {
                graphics.IsFullScreen = false;
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
                graphics.ApplyChanges();
            }

            
            //graphics.

            scale = new Vector3();
            scale.X = (float)GraphicsDevice.Viewport.Width / baseSize.X;
            scale.Y = (float)GraphicsDevice.Viewport.Height / baseSize.Y;
            scale.Z = 1;

            scaleMatrix = Matrix.CreateScale(scale);

            controls = new ControlsManager();
            sound = new SoundManager(Content);

            overlord = new Overlord(Content, GraphicsDevice, sound, cGameState);

            splashScreen = new SplashScreen(GraphicsDevice, Content, cGameState, controls);
            mainMenu = new Menu(GraphicsDevice, Content, cGameState, controls);
            gameEngine = new Gameplay(overlord, controls);
            editor = new Editor(overlord, controls);

            overlord._gameplay = gameEngine;
            overlord._editor = editor;

            tempMenu = new LevelMenu(overlord, controls);

            updateScaleMatrices();

            cGameState.Push(GameState.SplashScreen);
            //cGameState.Push(GameState.InGame);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            scaleTest = Content.Load<Texture2D>("textures\\null");
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            controls.Update(gameTime);
            sound.Update(gameTime);

            switch (cGameState.Peek())
            {
                case GameState.SplashScreen:
                    splashScreen.Update(gameTime);
                    break;
                case GameState.MainMenu:
                    tempMenu.Update(gameTime);
                    break;
                case GameState.InGame:
                    if(!paused)
                        gameEngine.Update(gameTime);
                    break;
                case GameState.OpeningLogos:
                    break;
                case GameState.Editor:
                    editor.Update(gameTime);
                    break;
                default:
                    break;
            }
            
            base.Update(gameTime);

            if (controls.BackPressed())
            {
                overlord.popGameState();
                paused = false;
            }

            if (cGameState.Count == 0)
                this.Exit();
            else if (cGameState.Peek().Equals(GameState.InGame) && controls.StartPressed())
                paused = !paused;
        }

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);

            //if (gameEngine != null && gameEngine.currentLevel != null)
            //{
                //gameEngine.currentLevel.ResetBuffer(GraphicsDevice);
            //}

            switch (cGameState.Peek())
            {
                case GameState.SplashScreen:
                    splashScreen.Draw(gameTime);
                    break;
                case GameState.MainMenu:
                    tempMenu.Draw(gameTime);
                    break;

                case GameState.InGame:
                    gameEngine.Draw(gameTime);
                    break;

                case GameState.Editor:
                    editor.Draw(gameTime);
                    break;

                case GameState.OpeningLogos:
                    break;
            }
            
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);

            //spriteBatch.Draw(scaleTest, new Rectangle(0, 0,1920,1080), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        /*void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            scale.X = (float)GraphicsDevice.Viewport.Width / baseSize.X;
            scale.Y = (float)GraphicsDevice.Viewport.Height / baseSize.Y;
            scale.Z = 1;

            scaleMatrix = Matrix.CreateScale(scale);

            updateScaleMatrices();
        }*/

        void updateScaleMatrices()
        {
            mainMenu.updateScaleMatrix(scaleMatrix);
            gameEngine.updateScaleMatrix(scaleMatrix);
            tempMenu.updateScaleMatrix(scaleMatrix);
            splashScreen.updateScaleMatrix(scaleMatrix);
            editor.updateScaleMatrix(scaleMatrix);
        }
    }
}
