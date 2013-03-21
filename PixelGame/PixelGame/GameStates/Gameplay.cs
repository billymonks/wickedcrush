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
using System.IO;
using WickedCrush.Utility;

namespace WickedCrush.GameStates
{
    public struct VertexPositionNormalTextureTangentBinormal : IVertexType
    {
        #region fields
        Vector4 vPosition;
        Vector3 vNormal;
        Vector2 vTextureCoordinate;
        Vector2 vNormalCoordinate;
        Vector3 vTangent;
        Vector3 vBinormal;
        #endregion

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 7, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 9, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 11, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
            new VertexElement(sizeof(float) * 14, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0)
        );
        public VertexPositionNormalTextureTangentBinormal(Vector4 position, Vector3 normal, Vector2 textureCoordinate, Vector2 normalCoordinate, Vector3 tangent, Vector3 binormal)
        {
            vPosition = position;
            vNormal = normal;
            vTextureCoordinate = textureCoordinate;
            vNormalCoordinate = normalCoordinate;
            vTangent = tangent;
            vBinormal = binormal;
        }
        public Vector4 Position
        {
            get { return vPosition; }
            set { vPosition = value; }
        }
        public Vector3 Normal
        {
            get { return vNormal; }
            set { vNormal = value; }
        }
        public Vector2 TextureCoordinate
        {
            get { return vTextureCoordinate; }
            set { vTextureCoordinate = value; }
        }
        public Vector2 NormalCoordinate
        {
            get { return vNormalCoordinate; }
            set { vNormalCoordinate = value; }
        }
        public Vector3 Tangent
        {
            get { return vTangent; }
            set { vTangent = value; }
        }
        public Vector3 Binormal
        {
            get { return vBinormal; }
            set { vBinormal = value; }
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    

    public class Gameplay
    {
        #region fields
        private SpriteBatch sb;

        //GraphicsDevice _graphics;
        //ContentManager _content;
        //SoundManager _sound;
        //Stack<GameState> _cGameState;
        ControlsManager _controls;

        CharacterFactory _cf;

        Matrix scaleMatrix;

        public Level currentLevel;

        Texture2D scaleTest;

        Effect normalMappingEffect;
        Matrix viewMatrix, projectionMatrix;
        //DynamicVertexBuffer vertexBuffer;
        //Vector3 lightDir;

        DepthStencilState depthStencilState;

        SpriteFont preAlphaFont;
        SpriteFont damageFont;

        Overlord _overlord;

        //hud stuff
        Texture2D health_bar;
        #endregion

        public Gameplay(Overlord overlord, ControlsManager controls)
        {

            //_graphics = gd;
            //_content = c;
            //_cGameState = cGameState;
            _controls = controls;
            //_sound = sound;
            _overlord = overlord;

            currentLevel = new Level(_overlord);

            this.Initialize();
        }

        public void Initialize()
        {
            sb = new SpriteBatch(_overlord._gd);

            depthStencilState = new DepthStencilState();
            depthStencilState.DepthBufferFunction = CompareFunction.LessEqual;
            depthStencilState.DepthBufferEnable = true;
            _overlord._gd.DepthStencilState = depthStencilState;

            scaleTest = _overlord._cm.Load<Texture2D>(@"textures/null");

            checkForCollisions();

            normalMappingEffect = _overlord._cm.Load<Effect>(@"effects/NormalMappingMultiLights");

            viewMatrix = Matrix.CreateLookAt(new Vector3(256f, 256f, 1125.0f), new Vector3(256f, 256f, 0f), Vector3.Up);
            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)_overlord._gd.Viewport.Width / (float)_overlord._gd.Viewport.Height, 0.2f, 1536f);
            projectionMatrix = Matrix.CreateOrthographic(1067f, 600f, 0.2f, 1536f);
            //lightDir = new Vector3(0.1f, -0.6f, -0.6f);

            preAlphaFont = _overlord._cm.Load<SpriteFont>(@"fonts/PreAlphaFont");
            damageFont = _overlord._cm.Load<SpriteFont>(@"fonts/DamageFont");

            health_bar = _overlord._cm.Load<Texture2D>(@"hud/health_bar");
        }

        public void InitializeLevelLoad(String path)
        {
            currentLevel = new Level(_overlord);
            currentLevel.hero._controls = _controls;
            currentLevel.levelCam._controls = _controls;

            currentLevel.loadLevel(path);

            _cf = currentLevel.cf;

            SetEffectParameters();

            //currentLevel.hero._controls = _controls;
            //currentLevel.levelCam._controls = _controls;
        }

        public void Update(GameTime gameTime)
        {
            checkForCollisions();

            foreach (Character c in currentLevel.characterList)
            {
                c.Update(gameTime);
            }

            foreach (PointLight l in currentLevel.lightList)
            {
                l.Update(gameTime);
            }

            foreach (DamageNumber d in currentLevel.floatingNumList)
            {
                d.Update(gameTime);
            }

            reapCharacters();
            reapLights();
            reapNumbers();

            _cf.ProcessQueue();
            
            //update camera
            currentLevel.levelCam.Update();

            viewMatrix = Matrix.CreateLookAt(
                currentLevel.levelCam.cameraPosition,
                currentLevel.levelCam.cameraTarget,
                currentLevel.levelCam.upVector);

            //_controls.Update(gameTime);
        }

        public void SetEffectParameters()
        {
            normalMappingEffect.Parameters["World"].SetValue(Matrix.Identity);
            
            normalMappingEffect.Parameters["Projection"].SetValue(projectionMatrix);

            normalMappingEffect.Parameters["AmbientColor"].SetValue(currentLevel.ambientLightColor);
            normalMappingEffect.Parameters["AmbientIntensity"].SetValue(currentLevel.ambientIntensity);

            //normalMappingEffect.Parameters["DiffuseColor"].SetValue(currentLevel.diffuseLightColor);
            //normalMappingEffect.Parameters["DiffuseIntensity"].SetValue(currentLevel.diffuseIntensity);

            //normalMappingEffect.Parameters["SpecularColor"].SetValue(currentLevel.specularLightColor);
            

            
        }

        public void Draw(GameTime gameTime)
        {
            _overlord._gd.Clear(currentLevel.bgColor);

            _overlord._gd.RasterizerState = RasterizerState.CullClockwise;
            //_graphics.DepthStencilState = depthStencilState;
            _overlord._gd.DepthStencilState = DepthStencilState.Default;

            normalMappingEffect.Parameters["View"].SetValue(viewMatrix);
            normalMappingEffect.Parameters["EyePosition"].SetValue(currentLevel.levelCam.cameraPosition);
            //normalMappingEffect.Parameters["PointLightPosition"].SetValue(new Vector3(currentLevel.hero.pos.X + 40f, currentLevel.hero.pos.Y + 50f, 300f));
            //normalMappingEffect.Parameters["PointLightRange"].SetValue(400f);

            if (currentLevel.solidGeomVertices.Length > 0)
            {
                normalMappingEffect.Parameters["ColorMap"].SetValue(currentLevel.solidGeomTex);
                normalMappingEffect.Parameters["NormalMap"].SetValue(currentLevel.solidGeomNorm);

                normalMappingEffect.Parameters["SpecularIntensity"].SetValue(1f);
                normalMappingEffect.Parameters["baseColor"].SetValue(currentLevel.baseColor);

                normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["MultiPassLight"]; //geom has normal map (some sprites do not)

                _overlord._gd.SetVertexBuffer(currentLevel.levelVertexBuffer);

                _overlord._gd.BlendState = BlendState.Opaque;

                normalMappingEffect.CurrentTechnique.Passes["Ambient"].Apply();
                _overlord._gd.DrawPrimitives(PrimitiveType.TriangleList, 0, currentLevel.solidGeomVertices.Length / 3);


                _overlord._gd.BlendState = BlendState.Additive;
                //normalMappingEffect.CurrentTechnique.Passes["Point"].Apply();
                foreach (PointLight p in currentLevel.lightList)
                {
                    normalMappingEffect.Parameters["DiffuseColor"].SetValue(p.diffuseColor);
                    normalMappingEffect.Parameters["DiffuseIntensity"].SetValue(p.intensity);
                    normalMappingEffect.Parameters["SpecularColor"].SetValue(p.specularColor);
                    normalMappingEffect.Parameters["PointLightPosition"].SetValue(p.pos);
                    normalMappingEffect.Parameters["PointLightRange"].SetValue(p.range);
                    normalMappingEffect.CurrentTechnique.Passes["Point"].Apply();
                    _overlord._gd.DrawPrimitives(PrimitiveType.TriangleList, 0, currentLevel.solidGeomVertices.Length / 3);
                    
                }

            }

            foreach (Character c in currentLevel.characterList)
            {
                if (c.currentAnimation != null)
                {
                    normalMappingEffect.Parameters["ColorMap"].SetValue(c.currentAnimation.animationSheet);

                    if (c.currentAnimationNorm != null)
                    {
                        normalMappingEffect.Parameters["NormalMap"].SetValue(c.currentAnimationNorm.animationSheet);
                        normalMappingEffect.Parameters["SpecularIntensity"].SetValue(c.specular);
                    }

                    //vertexBuffer = new DynamicVertexBuffer(_graphics, typeof(VertexPositionNormalTextureTangentBinormal), c.vertices.Length, BufferUsage.None); // needs improvement. d- see me after class
                    //vertexBuffer.SetData(c.vertices);

                    if(c.hurtFlash)
                        normalMappingEffect.Parameters["baseColor"].SetValue(currentLevel.hurtColor);
                    else if(c.blockFlash)
                        normalMappingEffect.Parameters["baseColor"].SetValue(currentLevel.blockColor);
                    else
                        normalMappingEffect.Parameters["baseColor"].SetValue(currentLevel.baseColor);

                    if (c.currentAnimationNorm != null)
                        normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["MultiPassLight"];
                    else
                        if(c.variableSize)
                            normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["MultiPassLightColorOnlyVariable"];
                        else if(c.bright)
                            normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["MultiPassLightColorOnly"]; //implement fullbright
                        else
                            normalMappingEffect.CurrentTechnique = normalMappingEffect.Techniques["MultiPassLightColorOnly"];

                    //_graphics.SetVertexBuffer(vertexBuffer);
                    _overlord._gd.SetVertexBuffer(c.vb);

                    _overlord._gd.BlendState = BlendState.Opaque;

                    normalMappingEffect.CurrentTechnique.Passes["Ambient"].Apply();

                    _overlord._gd.DrawPrimitives(PrimitiveType.TriangleList, 0, c.primCount);

                    //normalMappingEffect.CurrentTechnique.Passes["Point"].Apply();

                    _overlord._gd.BlendState = BlendState.Additive;
                    foreach (PointLight p in currentLevel.lightList)
                    {
                        normalMappingEffect.Parameters["DiffuseColor"].SetValue(p.diffuseColor);
                        normalMappingEffect.Parameters["DiffuseIntensity"].SetValue(p.intensity);
                        normalMappingEffect.Parameters["SpecularColor"].SetValue(p.specularColor);
                        normalMappingEffect.Parameters["PointLightPosition"].SetValue(p.pos);
                        normalMappingEffect.Parameters["PointLightRange"].SetValue(p.range);
                        normalMappingEffect.CurrentTechnique.Passes["Point"].Apply();
                        _overlord._gd.DrawPrimitives(PrimitiveType.TriangleList, 0, c.primCount);
                    }
                }
            }

            //vertexBuffer.
            
            
            sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);

            Draw2DElements(gameTime);
            //sb.DrawString(preAlphaFont, "Pre-Alpha Footage. All assets subject to change.", Vector2.Zero, Color.White);
            //gameplay interface go here
            //foreach (Entity e in currentLevel.entityList)
            //{
                //sb.Draw(e.getTexture(), e.getGraphicBox(), Color.White);
            //}

            //sb.Draw(scaleTest, new Rectangle(0, 0, 1920, 1080), Color.White);

            sb.End();
        }

        private void Draw2DElements(GameTime gameTime)
        {
            DrawDamageNumbers(gameTime);
            DrawHUD(gameTime);
        }

        private void DrawHUD(GameTime gameTime)
        {
            sb.Draw(health_bar, new Rectangle(25, 25, (int) (500 * ((float)currentLevel.hero.hp / 3f)), 35), Color.White * 0.5f);
        }

        private void DrawDamageNumbers(GameTime gameTime)
        {
            foreach (DamageNumber d in currentLevel.floatingNumList)
            {
                sb.DrawString(damageFont, d.number.ToString(), new Vector2(d.inGamePos.X - currentLevel.levelCam.cameraPosition.X + 1000f, -d.inGamePos.Y + currentLevel.levelCam.cameraPosition.Y + 500f), Color.White);
            }
        }

        public void updateScaleMatrix(Matrix sm)
        {
            scaleMatrix = sm;
        }
        
        public void checkForCollisions()
        {
            int leftSpot, rightSpot, topSpot, bottomSpot;

            for (int i = 0; i < currentLevel.characterList.Count; i++)
            {
                for (int j = i + 1; j < currentLevel.characterList.Count; j++) // check list with itself
                {
                    if (currentLevel.characterList[i].checkCharacterCollision(currentLevel.characterList[j]))
                    {
                        currentLevel.characterList[i].collisionList.Add(currentLevel.characterList[j]);
                        currentLevel.characterList[j].collisionList.Add(currentLevel.characterList[i]);
                    }
                    //if (currentLevel.characterList[i].checkCollision(currentLevel.characterList[j]))
                    //{
                        //currentLevel.characterList[i].collisionList.Add(currentLevel.characterList[j]);
                        //currentLevel.characterList[j].collisionList.Add(currentLevel.characterList[i]);
                    //}
                }

                if (!currentLevel.characterList[i].ignorePlatforms
                    && currentLevel.characterList[i].hitBox.Right > 0
                    && currentLevel.characterList[i].hitBox.Top > 0)
                {
                    leftSpot = (currentLevel.characterList[i].hitBox.Left / currentLevel.gridSize)-1;
                    rightSpot = (currentLevel.characterList[i].hitBox.Right / currentLevel.gridSize)+1;
                    topSpot = (currentLevel.characterList[i].hitBox.Bottom / currentLevel.gridSize)+1; //up is down
                    bottomSpot = (currentLevel.characterList[i].hitBox.Top / currentLevel.gridSize)-1;

                    if (leftSpot < 0)
                        leftSpot = 0;
                    if (rightSpot < 0)
                        rightSpot = 0;
                    if (topSpot < 0)
                        topSpot = 0;
                    if (bottomSpot < 0)
                        bottomSpot = 0;

                    if (leftSpot >= currentLevel.sGCD.GetLength(1))
                        leftSpot = currentLevel.sGCD.GetLength(1) - 1;
                    if (rightSpot >= currentLevel.sGCD.GetLength(1))
                        rightSpot = currentLevel.sGCD.GetLength(1) - 1;
                    if (topSpot >= currentLevel.sGCD.GetLength(0))
                        topSpot = currentLevel.sGCD.GetLength(0) - 1;
                    if (bottomSpot >= currentLevel.sGCD.GetLength(0))
                        bottomSpot = currentLevel.sGCD.GetLength(0) - 1;

                    /*for (int k = 0; k < currentLevel.solidGeom.Count; k++)
                    {
                        if (currentLevel.characterList[i].checkCollision(currentLevel.solidGeom[k])
                                || currentLevel.characterList[i].checkFeetCollision(currentLevel.solidGeom[k]))
                            if (currentLevel.solidGeom[k].checkDetailedCollision(currentLevel.characterList[i]))
                                currentLevel.characterList[i].collisionList.Add(currentLevel.solidGeom[k]);
                    }*/
                    /*for (int k = 0; k < currentLevel.sGCD.GetLength(0); k++) // check for collisions with solid geom (OBVIOUSLY DEBUG, could be linear)
                    {
                        for (int l = 0; l < currentLevel.sGCD.GetLength(1); l++)
                        {
                            if (currentLevel.characterList[i].checkCollision(currentLevel.sGCD[k, l])
                                || currentLevel.characterList[i].checkFeetCollision(currentLevel.sGCD[k, l]))
                                if (currentLevel.sGCD[k, l].checkDetailedCollision(currentLevel.characterList[i]))
                                    currentLevel.characterList[i].collisionList.Add(currentLevel.sGCD[k, l]);

                        }
                    }*/
                    for (int k = bottomSpot; k < topSpot; k++) // check for collisions with solid geom (OBVIOUSLY DEBUG, could be linear)
                    {
                        for (int l = leftSpot; l < rightSpot; l++)
                        {
                            if (currentLevel.characterList[i].checkCollision(currentLevel.sGCD[k, l])
                                || currentLevel.characterList[i].checkFeetCollision(currentLevel.sGCD[k, l]))
                                //if (currentLevel.sGCD[k, l].checkDetailedCollision(currentLevel.characterList[i]))
                                if (currentLevel.sGCD[k, l].checkDetailedCollision(currentLevel.characterList[i]))
                                    currentLevel.characterList[i].collisionList.Add(currentLevel.sGCD[k, l]);

                        }
                    }
                }
            }
        }

        public void reapCharacters()
        {
            for (int i = currentLevel.characterList.Count-1; i >= 0; i--)
            {
                if (currentLevel.characterList[i].readyForRemoval)
                    currentLevel.characterList.Remove(currentLevel.characterList[i]);
            }
        }

        public void reapLights()
        {
            for (int i = currentLevel.lightList.Count - 1; i >= 0; i--)
            {
                if (currentLevel.lightList[i].readyForRemoval)
                    currentLevel.lightList.Remove(currentLevel.lightList[i]);
            }
        }

        public void reapNumbers()
        {
            for (int i = currentLevel.floatingNumList.Count - 1; i >= 0; i--)
            {
                if (currentLevel.floatingNumList[i].readyForRemoval)
                    currentLevel.floatingNumList.Remove(currentLevel.floatingNumList[i]);
            }
        }

        public void StopCharacterSoundInstances()
        {
            foreach (Character c in currentLevel.characterList)
            {
                c.StopSoundInstances();
            }
        }
    }
}
