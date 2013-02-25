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
using WickedCrush;
using System.IO;
using WickedCrush.GameEntities;
using WickedCrush.Utility;

namespace Raditor
{
    public enum EditorViewMode
    {
        Dimension2 = 1,
        Dimension3 = 2
    }
    public class Editor : Microsoft.Xna.Framework.Game
    {
        #region fields
        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        private IntPtr drawSurface;

        Level currentLevel;

        int mouseX = 0;
        int mouseY = 0;

        SpriteFont ef1;

        EditorViewMode viewMode = EditorViewMode.Dimension3;

        Effect normalMappingEffect;
        Matrix viewMatrix, projectionMatrix;
        DepthStencilState depthStencilState;

        public Raditor.EditorForm.EditorTool currentTool;

        public Block currentBlock;
        public Tile currentTile;
        List<Block> completeBlockList;

        public Character currentCharacter;
        private bool placementOK = false;

        public Camera editorCamera;

        KeyboardState keyState;
        KeyboardState oldKeyState;

        public bool eraseMode = false;

        public String levelName, authorName;

        Overlord overlord;
        #endregion

        public Editor(IntPtr drawSurface)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.drawSurface = drawSurface;

            levelName = "Untitled";
            authorName = "Noname";

            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);

            System.Windows.Forms.Control.FromHandle(this.Window.Handle).VisibleChanged +=
                new EventHandler(Editor_VisibleChanged);

        }

        public List<Block> getCompleteBlockList()
        {
            Block tempBlock;
            List<Block> cBL = new List<Block>();
            string[] files = System.IO.Directory.GetFiles(@"Content/blocks", "*", SearchOption.AllDirectories);
            foreach (String s in files)
            {
                tempBlock = new Block();
                tempBlock.loadBlock(s);
                cBL.Add(tempBlock);
            }

            return cBL;
        }

        public void ReplaceViewMode(EditorViewMode m)
        {
            viewMode = m;
            if (viewMode.Equals(EditorViewMode.Dimension2))
            {
                projectionMatrix = Matrix.CreateOrthographic(1280f, 720f, 0.2f, 5000f);
            }
            if (viewMode.Equals(EditorViewMode.Dimension3))
            {
                projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1280f / 720f, 0.2f, 5000f);
            }
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.ApplyChanges();

            overlord = new Overlord(Content, GraphicsDevice);

            //editorCamera = new Camera();
            resetCamera();

            currentLevel = new Level(overlord);
            currentLevel.diffuseIntensity = 0.7f;
            currentLevel.ambientIntensity = 0.1f;

            depthStencilState = new DepthStencilState();
            depthStencilState.DepthBufferFunction = CompareFunction.LessEqual;
            depthStencilState.DepthBufferEnable = true;
            graphics.GraphicsDevice.DepthStencilState = depthStencilState;

            //normalMappingEffect = Content.Load<Effect>(@"effects/normEffect");
            normalMappingEffect = Content.Load<Effect>(@"effects/NormalMapping");

            viewMatrix = Matrix.CreateLookAt(editorCamera.cameraPosition, editorCamera.cameraTarget, editorCamera.upVector);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1280f / 720f, 0.2f, 5000f);

            //testVector = new Vector4(256f, 256f, 0f, 0f);
            //lightDir = new Vector3(0.1f, -0.4f, -0.2f);

            completeBlockList = getCompleteBlockList();
            currentBlock = new Block();
            currentBlock.loadBlock(@"Content/blocks/default_block.xml");

            currentTile = new Tile();
            currentTile.loadTile(@"Content/tiles/basic_tile.xml");

            keyState = Keyboard.GetState();

            base.Initialize();
        }

        private void resetCamera()
        {
            editorCamera = new Camera();
            editorCamera.cameraPosition = new Vector3(640f, 360f, 1125.0f);
            editorCamera.cameraTarget = new Vector3(640f, 360f, 0f);
            editorCamera.minCamPos.X = 640f;
            editorCamera.minCamPos.Y = 360f;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ef1 = Content.Load<SpriteFont>(@"fonts/editorFont1");
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            CheckForSimpleCollisions();
            KeyboardInputHandler();

            if(currentCharacter!=null)
                currentCharacter.EditorUpdate();

            currentLevel.cf.ProcessQueue();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(currentLevel.bgColor);

            normalMappingEffect.Parameters["World"].SetValue(Matrix.Identity);
            normalMappingEffect.Parameters["View"].SetValue(viewMatrix);
            normalMappingEffect.Parameters["Projection"].SetValue(projectionMatrix);

            normalMappingEffect.Parameters["AmbientColor"].SetValue(currentLevel.ambientLightColor);
            normalMappingEffect.Parameters["AmbientIntensity"].SetValue(currentLevel.ambientIntensity);

            //normalMappingEffect.Parameters["LightDirection"].SetValue(lightDir);
            normalMappingEffect.Parameters["DiffuseColor"].SetValue(currentLevel.diffuseLightColor);
            normalMappingEffect.Parameters["DiffuseIntensity"].SetValue(currentLevel.diffuseIntensity);

            normalMappingEffect.Parameters["SpecularColor"].SetValue(currentLevel.specularLightColor);
            normalMappingEffect.Parameters["EyePosition"].SetValue(currentLevel.levelCam.cameraPosition);



            normalMappingEffect.Parameters["PointLightPosition"].SetValue(new Vector3(editorCamera.cameraPosition.X, editorCamera.cameraPosition.Y, 300f));
            normalMappingEffect.Parameters["PointLightRange"].SetValue(600f);

            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            graphics.GraphicsDevice.DepthStencilState = depthStencilState;
            

            if (currentLevel.solidGeomVertices.Length > 0)
            {
                //vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionNormalTextureTangentBinormal), currentLevel.solidGeomVertices.Length, BufferUsage.None);
                //vertexBuffer.SetData(currentLevel.solidGeomVertices);

                normalMappingEffect.Parameters["ColorMap"].SetValue(currentLevel.solidGeomTex);
                normalMappingEffect.Parameters["NormalMap"].SetValue(currentLevel.solidGeomNorm);

                normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["PerPixelNormalMappingTechnique"]; //geom has normal map (sprites do not)

                graphics.GraphicsDevice.SetVertexBuffer(currentLevel.levelVertexBuffer);

                foreach (EffectPass pass in normalMappingEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    //graphics.GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, currentLevel.solidGeomVertices.Length/3);

                }
            }

            foreach (Character c in currentLevel.characterList)
            {
                //if (c != null) //fuck it
                //{
                    if (c.defaultNormal != null)
                    {
                        normalMappingEffect.Parameters["NormalMap"].SetValue(c.defaultNormal);
                        normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["PerPixelNormalMappingTechnique"];
                    }
                    else
                        normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["PerPixelNoNormalTechnique"];

                    if (c.defaultTexture != null)
                    {
                        normalMappingEffect.Parameters["ColorMap"].SetValue(c.defaultTexture);

                        graphics.GraphicsDevice.SetVertexBuffer(c.vb);

                        foreach (EffectPass pass in normalMappingEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, c.primCount);
                        }
                    }
                //}
            }

            if (currentCharacter != null)
            {
                if (currentCharacter.defaultNormal != null)
                {
                    normalMappingEffect.Parameters["NormalMap"].SetValue(currentCharacter.defaultNormal);
                    normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["PerPixelNormalMappingTechnique"];
                }
                else
                    normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["PerPixelNoNormalTechnique"];

                if (currentCharacter.defaultTexture != null)
                {
                    normalMappingEffect.Parameters["ColorMap"].SetValue(currentCharacter.defaultTexture);

                    graphics.GraphicsDevice.SetVertexBuffer(currentCharacter.vb);

                    foreach (EffectPass pass in normalMappingEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, currentCharacter.primCount);
                    }
                }
            }

            spriteBatch.Begin();
            //spriteBatch.DrawString(ef1, "Camera position: " + editorCamera.cameraPosition.X + ", " + editorCamera.cameraPosition.Y, new Vector2(0, 0), Color.White);
            //spriteBatch.DrawString(ef1, "Mouse position: " + x + ", " + y, new Vector2(0, 0), Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void mouseOverPosition(int x, int y)
        {
            mouseX = x;
            mouseY = y;

            if (currentCharacter != null)
            {
                currentCharacter.pos.X = (float)(mouseX + editorCamera.cameraPosition.X - 640);
                currentCharacter.pos.Y = (float)((editorCamera.cameraPosition.Y + 360 - mouseY));
            }
        }

        public void clickHandler(int x, int y) //click on the game render area
        {
            this.mouseX = x;
            this.mouseY = y;
            if(currentTool.Equals(Raditor.EditorForm.EditorTool.Block))
            {
                if (eraseMode == false)
                {
                    if (currentBlock != null)
                        addBlock(currentBlock, (int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
                else
                {
                    removeBlock((int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
            }
            else if (currentTool.Equals(Raditor.EditorForm.EditorTool.Background))
            {
                if (eraseMode == false)
                {
                    if (currentTile != null)
                        addBackground(currentTile, (int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
                else
                {
                    removeBackground((int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
            }
            else if (currentTool.Equals(Raditor.EditorForm.EditorTool.Foreground))
            {
                if (eraseMode == false)
                {
                    if (currentTile != null)
                        addForeground(currentTile, (int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
                else
                {
                    removeForeground((int)((editorCamera.cameraPosition.Y + 360 - y) / currentLevel.gridSize), (int)((x + editorCamera.cameraPosition.X - 640) / currentLevel.gridSize));
                }
            }
            else if (currentTool.Equals(Raditor.EditorForm.EditorTool.Entity))
            {
                if (eraseMode == false)
                {
                    if (currentCharacter != null && placementOK)
                    {
                        if(!currentCharacter.airborne)
                            currentCharacter.pos.Y = currentCharacter.GetHighestSensorPoint();
                        currentCharacter.EditorUpdate();
                        currentLevel.characterList.Add(currentCharacter);
                        loadCurrentCharacter(currentCharacter.name);
                    }
                }
                else
                {
                    removeCharacter(
                        (int)(mouseX + editorCamera.cameraPosition.X - 640),
                        (int)(editorCamera.cameraPosition.Y + 360 - mouseY));
                }
            }
        }
        public void removeBlock(int row, int col)
        {
            if (row < currentLevel.sGCD.GetLength(0) && col < currentLevel.sGCD.GetLength(1) && currentLevel.sGCD[row, col] != null)
            {
                currentLevel.sGCD[row, col] = null;
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }
        public void removeBackground(int row, int col)
        {
            if (row < currentLevel.sGCD.GetLength(0) && col < currentLevel.sGCD.GetLength(1) && currentLevel.levelBackgrounds[row, col] != null)
            {
                currentLevel.levelBackgrounds[row, col] = null;
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }
        public void removeForeground(int row, int col)
        {
            if (row < currentLevel.sGCD.GetLength(0) && col < currentLevel.sGCD.GetLength(1) && currentLevel.levelForegrounds[row, col] != null)
            {
                currentLevel.levelForegrounds[row, col] = null;
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }
        public void removeCharacter(int x, int y)
        {
            for (int i = currentLevel.characterList.Count-1; i >= 0; i--)
            {
                if (currentLevel.characterList[i].hitBox.Contains(x, y))
                    currentLevel.characterList.Remove(currentLevel.characterList[i]);
                
            }
        }
        public void addBlock(Block b, int row, int col)
        {
            if (row >= 0 && col >= 0)
            {
                currentLevel.extendSGCD(row, col); //extend grid if not large enough (does nothing & is memory waste if it is but whateva we major)
                currentLevel.sGCD[row, col] = new Platform(col * currentLevel.gridSize, row * currentLevel.gridSize, currentLevel.gridSize, currentLevel.gridSize, b); //obviously debug
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }
        public void addBackground(Tile t, int row, int col)
        {
            if (row >= 0 && col >= 0)
            {
                currentLevel.extendSGCD(row, col);
                currentLevel.levelBackgrounds[row, col] = t.name;
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }
        public void addForeground(Tile t, int row, int col)
        {
            if (row >= 0 && col >= 0)
            {
                currentLevel.extendSGCD(row, col);
                currentLevel.levelForegrounds[row, col] = t.name;
                currentLevel.levelChanged(Content, graphics.GraphicsDevice);
            }
        }

        public void levelLoader(String name)
        {
            currentLevel.loadLevel(name);
            resetCamera();
        }

        public void levelSaver(String name)
        {
            currentLevel.saveLevel(name, levelName, authorName);
        }

        public void loadCurrentCharacter(String name)
        {
            switch(name)
            {
                case "Level_Entrance":
                    currentCharacter = new LevelEntrance(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null);
                    break;
                case "Spike_Trap":
                    currentCharacter = new SpikeTrap(Content, graphics.GraphicsDevice, new Vector2(0f, 0f));
                    break;
                case "Level_Exit":
                    currentCharacter = new LevelExit(new Vector2(0f, 0f), overlord);
                    break;
                case "TreeMob":
                    currentCharacter = new TreeMob(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null, null);
                    break;
                case "CaveCrystal1":
                    currentCharacter = new CaveCrystal1(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null);
                    break;
                case "CaveCrystal2":
                    currentCharacter = new CaveCrystal2(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null);
                    break;
                case "CaveCrystal3":
                    currentCharacter = new CaveCrystal3(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null);
                    break;
                case "Birdy":
                    currentCharacter = new Birdy(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null);
                    break;
                case "Rhino":
                    currentCharacter = new Rhino(Content, graphics.GraphicsDevice, new Vector2(0f, 0f), null, null);
                    break;
                default:
                    currentCharacter = null;
                    break;
            }
        }

        public void newLevel()
        {
            currentLevel = new Level(overlord);
            resetCamera();
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.DeviceWindowHandle = drawSurface;
        }

        private void Editor_VisibleChanged(object sender, EventArgs e)
        {
            if (System.Windows.Forms.Control.FromHandle(this.Window.Handle).Visible == true)
                System.Windows.Forms.Control.FromHandle(this.Window.Handle).Visible = false;
        }

        private void KeyboardInputHandler()
        {
            oldKeyState = keyState;
            keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.A))
                editorCamera.MoveCamLeft(5f);
            if (keyState.IsKeyDown(Keys.D))
                editorCamera.MoveCamRight(5f);
            if (keyState.IsKeyDown(Keys.W))
                editorCamera.MoveCamUp(5f);
            if (keyState.IsKeyDown(Keys.S))
                editorCamera.MoveCamDown(5f);

            viewMatrix = Matrix.CreateLookAt(editorCamera.cameraPosition, editorCamera.cameraTarget, editorCamera.upVector);
        }

        private void CheckForSimpleCollisions()
        {
            if (currentCharacter != null)
            {
                currentCharacter.collisionList.Clear();
                currentCharacter.underFeetCollisionList.Clear();

                for (int i = 0; i < currentLevel.characterList.Count; i++)
                {
                    if (currentLevel.characterList[i].checkCollision(currentCharacter))
                    {
                        currentCharacter.collisionList.Add(currentLevel.characterList[i]);
                    }
                }

                for (int k = 0; k < currentLevel.sGCD.GetLength(0); k++) // check for collisions with solid geom (OBVIOUSLY DEBUG, could be linear)
                {
                    for (int l = 0; l < currentLevel.sGCD.GetLength(1); l++)
                    {
                        if (currentLevel.sGCD[k, l] != null)
                            currentLevel.sGCD[k, l].editorCheckDetailedCollision(currentCharacter);
                    }
                }
                if (currentCharacter.underFeetCollisionList.Count > 0 && !currentCharacter.airborne)
                    //&& currentCharacter.collisionList.Count == 0)
                {
                    placementOK = true;
                    
                }
                else if (currentCharacter.underFeetCollisionList.Count == 0 && currentCharacter.airborne)
                {
                    placementOK = true;
                } else
                    placementOK = false;
            }
        }
    }
}
