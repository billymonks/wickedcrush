using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using WickedCrush.GameEntities;
using WickedCrush.Utility;
using WickedCrush.GameStates;

namespace WickedCrush
{
    public struct texLookup
    {
        public String texName;
        public Point topLeft, topRight, bottomLeft, bottomRight;
    }
    public class Level
    {
        #region fields
        public String name, author;
        public List<Character> characterList;
        public Color bgColor;

        public List<PointLight> lightList;

        public List<DamageNumber> floatingNumList;

        public Texture2D solidGeomTex; //solid geometry supertexture (composed of textures from the used blocks)
        public Texture2D solidGeomNorm; //solid geometry normal supertexture (composed of textures from the used blocks)
        public List<texLookup> texLookupList;
        public List<texLookup> normLookupList;

        public Platform[,] sGCD; //solid geometry collision data
        //public List<Platform> solidGeom;

        public String[,] levelBackgrounds;
        public String[,] levelForegrounds;
        public VertexPositionNormalTextureTangentBinormal[] solidGeomVertices;

        public VertexBuffer levelVertexBuffer;

        public Vector4 ambientLightColor = new Vector4(1f, 1f, 1f, 1f);
        public Vector4 diffuseLightColor = new Vector4(0.6f, 0.8f, 1f, 1f);
        public Vector4 specularLightColor = new Vector4(0.6f, 0.6f, 0.8f, 0.5f);
        public float ambientIntensity = 0.175f;
        public float diffuseIntensity = 0.5f;

        public Vector4 baseColor = new Vector4(0.05f, 0.05f, 0.1f, 1f);
        public Vector4 hurtColor = new Vector4(0.55f, 0.05f, 0.1f, 1f);
        public Vector4 blockColor = new Vector4(0.25f, 0.25f, 0.4f, 1f);
        
        public int gridSize;
        private int scrollType = 0; //0=horizontal, 1=vertical, 2=lockon; currently unused

        public Hero hero;

        public Camera levelCam;

        public CharacterFactory cf;
        public Overlord _overlord;
        #endregion

        public Level(Overlord overlord) //gd needs to be removed when this stuff is loaded in the level loader like it should
        {
            name = "";
            characterList = new List<Character>();
            lightList = new List<PointLight>();
            floatingNumList = new List<DamageNumber>();
            cf = new CharacterFactory(characterList, lightList, floatingNumList, overlord);
            bgColor = new Color(0.05f, 0.05f, 0.1f);
            gridSize = 64;
            scrollType = 0;

            sGCD = new Platform[0, 0];
            levelBackgrounds = new String[0, 0];
            levelForegrounds = new String[0, 0];
            solidGeomVertices = new VertexPositionNormalTextureTangentBinormal[0];

            _overlord = overlord;

            hero = new Hero(overlord._cm, overlord._gd, new Vector2(-256f, 800f), cf, overlord._sound);

            levelCam = new Camera();
            setupCamera();
        }

        private void setupCamera()
        {
            levelCam.cameraPosition.Z = 880f;
            //levelCam.cameraPosition.Z = 720f;
            levelCam.cameraTarget.Z = 0f;
            levelCam.SetTarget(hero);
        }

        /*public void saveLevel(String FILE_NAME, String LEVEL_NAME, String LEVEL_AUTHOR)
        {
            XDocument doc = new XDocument();
            XElement rootElement = new XElement("level");
            XElement attributes = new XElement("attributes");
            XElement grid = new XElement("grid");
            XElement background = new XElement("background");
            XElement foreground = new XElement("foreground");
            XElement entities = new XElement("entities");

            rootElement.Add(new XAttribute("name", LEVEL_NAME));
            rootElement.Add(new XAttribute("author", LEVEL_AUTHOR));

            attributes.Add(new XAttribute("scroll", scrollType));

            grid.Add(new XAttribute("cols", sGCD.GetLength(1)),
                new XAttribute("rows", sGCD.GetLength(0)));

            for (int i = 0; i < sGCD.GetLength(0); i++)
            {
                for (int j = 0; j < sGCD.GetLength(1); j++)
                {
                    if (sGCD[i, j] != null)
                        grid.Add(new XElement("coord",
                            new XAttribute("x", j),
                            new XAttribute("y", i),
                            new XAttribute("block", sGCD[i, j].blockName)));

                    if (levelBackgrounds[i, j] != null)
                        background.Add(new XElement("coord",
                            new XAttribute("x", j),
                            new XAttribute("y", i),
                            new XAttribute("tile", levelBackgrounds[i, j])));

                    if (levelForegrounds[i, j] != null)
                        foreground.Add(new XElement("coord",
                            new XAttribute("x", j),
                            new XAttribute("y", i),
                            new XAttribute("tile", levelForegrounds[i, j])));
                }
            }

            foreach (Character c in characterList)
            {
                if (!c.name.Equals("Hero"))
                    entities.Add(new XElement("entity",
                        new XAttribute("name", c.name),
                        new XAttribute("xPos", c.pos.X),
                        new XAttribute("yPos", c.pos.Y)));
            }

            rootElement.Add(attributes);
            rootElement.Add(grid);
            rootElement.Add(background);
            rootElement.Add(foreground);
            rootElement.Add(entities);
            doc.Add(rootElement);

            doc.Save(FILE_NAME);
        }*/

        public void saveLevel(String FILE_NAME, String LEVEL_NAME, String LEVEL_AUTHOR)
        {
            XDocument doc = new XDocument();
            XElement rootElement = new XElement("level");
            XElement attributes = new XElement("attributes");
            XElement grid = new XElement("grid");
            XElement entities = new XElement("entities");

            rootElement.Add(new XAttribute("name", LEVEL_NAME));
            rootElement.Add(new XAttribute("author", LEVEL_AUTHOR));

            //attributes.Add(new XAttribute("scroll", scrollType));

            grid.Add(new XAttribute("cols", sGCD.GetLength(1)),
                new XAttribute("rows", sGCD.GetLength(0)));

            for (int i = 0; i < sGCD.GetLength(0); i++)
            {
                for (int j = 0; j < sGCD.GetLength(1); j++)
                {
                    if (sGCD[i, j] != null)
                        grid.Add(new XElement("coord",
                            new XAttribute("x", j),
                            new XAttribute("y", i),
                            new XAttribute("mat", sGCD[i, j].matName)));
                }
            }

            foreach (Character c in characterList)
            {
                if (!c.name.Equals("Hero"))
                    entities.Add(new XElement("entity",
                        new XAttribute("name", c.name),
                        new XAttribute("xPos", c.pos.X),
                        new XAttribute("yPos", c.pos.Y)));
            }

            rootElement.Add(attributes);
            rootElement.Add(grid);
            rootElement.Add(entities);
            doc.Add(rootElement);

            doc.Save(FILE_NAME);
        }

        public void loadLevel(String LEVEL_NAME)
        {
            int row, col;
            Corner cor = Corner.TopLeft;
            //int x, y;
            String elementName;
            List<Texture2D> texList, normList;

            Direction tempDir;

            XDocument doc = XDocument.Load(LEVEL_NAME);

            List<Material> matList = new List<Material>();
            List<Material> completeMatList = new List<Material>();

            if (_overlord._sound != null)
            {
                _overlord._sound.setCam(levelCam);

                _overlord._sound.addSound("cave_ambient", "17729__royal__cavern-wind");
                _overlord._sound.playAmbientLoop("cave_ambient");
                // not while i'm testing please stop
            }

            getCompleteMatList(completeMatList);

            XElement rootElement = new XElement(doc.Element("level"));
            XElement attributes = rootElement.Element("attributes");
            XElement grid = rootElement.Element("grid");
            //XElement background = rootElement.Element("background");
            //XElement foreground = rootElement.Element("foreground");
            XElement entities = rootElement.Element("entities");

            this.name = rootElement.Attribute("name").Value;
            this.author = rootElement.Attribute("author").Value;

            //this.scrollType = int.Parse(attributes.Attribute("scroll").Value);
            row = int.Parse(grid.Attribute("rows").Value);
            col = int.Parse(grid.Attribute("cols").Value);

            sGCD = new Platform[row, col];
            //levelBackgrounds = new String[row, col];
            //levelForegrounds = new String[row, col];

            foreach (XElement e in grid.Elements("coord"))
            {
                col = int.Parse(e.Attribute("x").Value);
                row = int.Parse(e.Attribute("y").Value);
                elementName = e.Attribute("mat").Value;
                sGCD[row, col] = new Platform(col * gridSize, row * gridSize, gridSize+1, gridSize, matSelection(elementName, completeMatList));
                matList.Add(matSelection(elementName, completeMatList));
            }

            foreach (XElement e in grid.Elements("ramp"))
            {
                col = int.Parse(e.Attribute("x").Value);
                row = int.Parse(e.Attribute("y").Value);
                elementName = e.Attribute("mat").Value;

                switch (e.Attribute("corner").Value)
                {
                    case "top-left":
                        cor = Corner.TopLeft;
                        break;
                    case "top-right":
                        cor = Corner.TopRight;
                        break;
                    case "bottom-left":
                        cor = Corner.BottomLeft;
                        break;
                    case "bottom-right":
                        cor = Corner.BottomRight;
                        break;
                }

                sGCD[row, col] = new Ramp(col * gridSize, row * gridSize, gridSize + 1, gridSize, cor, matSelection(elementName, completeMatList));
                matList.Add(matSelection(elementName, completeMatList));
            }

/*            sGCD[53, 12] = new Ramp(12 * gridSize, 53 * gridSize, gridSize + 1, gridSize, Corner.BottomLeft, matSelection("stone", completeMatList));

            sGCD[56, 12] = new Ramp(12 * gridSize, 56 * gridSize, gridSize + 1, gridSize, Corner.TopLeft, matSelection("stone", completeMatList));

            sGCD[53, 18] = new Ramp(18 * gridSize, 53 * gridSize, gridSize + 1, gridSize, Corner.BottomRight, matSelection("stone", completeMatList));

            sGCD[56, 18] = new Ramp(18 * gridSize, 56 * gridSize, gridSize + 1, gridSize, Corner.TopRight, matSelection("stone", completeMatList));*/

            matList = removeDupesFromMatList(matList);

            texList = getTextureList(matList, _overlord._cm);
            normList = getNormalList(matList, _overlord._cm);

            solidGeomTex = compileTex(_overlord._cm, _overlord._gd, texList, out texLookupList, 64);
            solidGeomNorm = compileTex(_overlord._cm, _overlord._gd, normList, out normLookupList, 64);

            createSolidGeomVertices(matList);

            if (solidGeomVertices.Length > 0)
            {
                levelVertexBuffer = new VertexBuffer(_overlord._gd, typeof(VertexPositionNormalTextureTangentBinormal), solidGeomVertices.Length, BufferUsage.None);
                levelVertexBuffer.SetData(solidGeomVertices);
            }

            //characterList = new List<Character>();

            characterList.Clear();
            lightList.Clear();
            floatingNumList.Clear();

            cf.AddCharacterToList(hero);

            cf.AddLightToList(new GlowingPointLight(hero,
                850f,
                new Vector4(0.7f, 0.75f, 0.9f, 1f),
                new Vector4(0.4f, 0.9f, 0.6f, 0.7f),
                1300f,
                0.6f,
                new Vector2(40f, 50f),
                0.035f,
                285));


            foreach (XElement e in entities.Elements("entity"))
            {
                elementName = e.Attribute("name").Value;
                if (int.Parse(e.Attribute("direction").Value) == 0)
                    tempDir = Direction.Left;
                else
                    tempDir = Direction.Right;
                

                switch (elementName) //this is as good as it's going to get, sadly
                {
                    case "Level_Entrance":
                        cf.AddCharacterToList(new LevelEntrance(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            hero));
                        break;
                    case "Level_Exit":
                        cf.AddCharacterToList(new LevelExit(
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                                _overlord));
                        break;
                    case "Spike_Trap":
                        cf.AddCharacterToList(new SpikeTrap(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value))));
                        break;
                    case "TreeMob":
                        cf.AddCharacterToList(new TreeMob(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf,
                            hero,
                            tempDir,
                            _overlord._sound));
                        break;
                    case "Flametosser":
                        cf.AddCharacterToList(new Flametosser(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf,
                            tempDir));
                        break;
                    case "CaveCrystal1":
                        cf.AddCharacterToList(new CaveCrystal1(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf));
                        break;
                    case "CaveCrystal2":
                        cf.AddCharacterToList(new CaveCrystal2(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf));
                        break;
                    case "CaveCrystal3":
                        cf.AddCharacterToList(new CaveCrystal3(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf));
                        break;

                    case "Birdy":
                        cf.AddCharacterToList(new Birdy(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf,
                            tempDir));
                        break;
                    case "Rhino":
                        cf.AddCharacterToList(new Rhino(
                            _overlord._cm,
                            _overlord._gd,
                            new Vector2(
                                float.Parse(e.Attribute("xPos").Value),
                                float.Parse(e.Attribute("yPos").Value)),
                            cf,
                            hero,
                            tempDir));
                        break;
                }
            }

            //cf.AddCharacterToList(enterDoor);
            //cf.AddCharacterToList(spikeTrap);

            setupCamera();

        }

        public void clearLevel(ContentManager cm, GraphicsDevice gd, SoundManager sound)
        {
            name = "";
            characterList = new List<Character>();
            lightList = new List<PointLight>();
            floatingNumList = new List<DamageNumber>();
            cf = new CharacterFactory(characterList, lightList, floatingNumList, _overlord);

            bgColor = new Color(0.05f, 0.05f, 0.1f);
            gridSize = 64;
            scrollType = 0;

            sGCD = new Platform[0, 0];
            levelBackgrounds = new String[0, 0];
            levelForegrounds = new String[0, 0];
            solidGeomVertices = new VertexPositionNormalTextureTangentBinormal[0];

            hero.RestoreHero();
            //hero = new Hero(cm, gd, new Vector2(-256f, 800f));
            setupCamera();
        }

        public List<Material> removeDupesFromMatList(List<Material> matList)
        {
            List<Material> newMatList = matList.Distinct().ToList<Material>();
            return newMatList;
        }

        public Material matSelection(String matName, List<Material> matList)
        {
            Material tempMat = new Material();
            foreach (Material m in matList)
            {
                if (m.name.Equals(matName))
                {
                    tempMat = m;
                    return tempMat;
                }
            }

            return null;
        }

        public Tile tileSelection(String tileName, List<Tile> tileList)
        {
            Tile tempTile = new Tile();
            foreach (Tile t in tileList)
            {
                if (t.name.Equals(tileName))
                {
                    tempTile = t;
                    return tempTile;
                }
            }

            return null;
        }

        public Texture2D compileTex(ContentManager cm, GraphicsDevice gd, List<Texture2D> texList, out List<texLookup> lookupList, int tSize) //giant texture atlas that contains all the textures for the level, 
        {
            int texSize = tSize;
            int rc = (int)Math.Ceiling(Math.Sqrt((double)texList.Count));
            int width, height;

            RenderTarget2D renderTarget;
            Texture2D compiledTexture;
            Texture2D[] textureList = texList.ToArray<Texture2D>();
            Color[] cData;
            lookupList = new List<texLookup>(); // contains texture coordinates in the large compiled texture

            SpriteBatch sb = new SpriteBatch(gd);

            texLookup tempTexLookup;

            if(rc>0)
                renderTarget = new RenderTarget2D(
                    gd,
                    texSize * rc,
                    texSize * rc,
                    true,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24);
            else //forgive me, for i know not what i do. this is just to keep it from crashing if an empty level is loaded.
                renderTarget = new RenderTarget2D(
                    gd,
                    1,
                    1,
                    true,
                    SurfaceFormat.Color,
                    DepthFormat.Depth24);

            gd.SetRenderTarget(renderTarget);

            gd.Clear(ClearOptions.Target, new Color(0f, 0f, 0f, 0f), 0, 0);

            sb.Begin();

            for (int i = 0; i < textureList.Length; i++) //render all textures in list to renderTarget and place coordinates in lookup list
            {
                tempTexLookup = new texLookup();
                tempTexLookup.texName = textureList[i].Name;
                tempTexLookup.topLeft = new Point((i % rc)*texSize, (i / rc) * texSize);
                tempTexLookup.bottomLeft = new Point((i % rc) * texSize, (i / rc + 1) * texSize);
                tempTexLookup.topRight = new Point(((i % rc)+1) * texSize, (i / rc) * texSize);
                tempTexLookup.bottomRight = new Point(((i % rc) + 1) * texSize, (i / rc + 1) * texSize);
                lookupList.Add(tempTexLookup);
                sb.Draw(textureList[i], new Rectangle(tempTexLookup.topLeft.X, tempTexLookup.topLeft.Y, tempTexLookup.topRight.X - tempTexLookup.topLeft.X, tempTexLookup.bottomLeft.Y - tempTexLookup.topLeft.Y), Color.White);
            }
            sb.End();

            gd.SetRenderTarget(null);

            compiledTexture = new Texture2D(gd,
                renderTarget.Width, renderTarget.Height, true,
                renderTarget.Format);
            
            for (int i = 0; i < renderTarget.LevelCount; i++)
            {
                width = (int)Math.Max((renderTarget.Width / Math.Pow(2, i)), 1);
                height = (int)Math.Max((renderTarget.Width / Math.Pow(2, i)), 1);
                cData = new Color[width * height];

                renderTarget.GetData<Color>(i, null, cData, 0, cData.Length);
                compiledTexture.SetData<Color>(i, null, cData, 0, cData.Length);
            }

            
            //compiledTexture = renderTarget;
            
            return compiledTexture;

            //return renderTarget;
        }

        public List<Texture2D> getTextureList(List<Material> matList, ContentManager cm) //this is crazy
        {
            List<Texture2D> textureList = new List<Texture2D>();
            Texture2D tempTex;
            foreach (Material m in matList)
            {
                for (int i = 0; i < 48; i++)
                {
                    for (int j = 0; j < m.textures[i].Length; j++)
                    {
                        for (int k = 0; k < m.textures[i][j].Length; k++)
                        {
                            tempTex = cm.Load<Texture2D>("textures//" + m.textures[i][j][k]);
                            tempTex.Name = m.textures[i][j][k];
                            textureList.Add(tempTex);
                        }
                    }
                }
            }
            
            textureList = textureList.Distinct<Texture2D>().ToList<Texture2D>();
            return textureList;
        }
        public List<Texture2D> getNormalList(List<Material> matList, ContentManager cm) //combine me maybe?
        {
            List<Texture2D> normalList = new List<Texture2D>();
            Texture2D tempNorm;
            foreach (Material m in matList)
            {
                for (int i = 48; i < 96; i++)
                {
                    for (int j = 0; j < m.textures[i].Length; j++)
                    {
                        for (int k = 0; k < m.textures[i][j].Length; k++)
                        {
                            tempNorm = cm.Load<Texture2D>("normals//" + m.textures[i][j][k]);
                            tempNorm.Name = m.textures[i][j][k];
                            normalList.Add(tempNorm);
                        }
                    }
                }
                
            }

            normalList = normalList.Distinct<Texture2D>().ToList<Texture2D>();
            return normalList;
        }

        public void getCompleteMatList(List<Material> completeMatList)
        {
            Material tempMat;
            string[] files = System.IO.Directory.GetFiles(@"Content/materials", "*", SearchOption.AllDirectories);
            foreach (String s in files)
            {
                tempMat = new Material();
                tempMat.loadMat(s);
                completeMatList.Add(tempMat);
            }
        }
       

        public void createSolidGeomVertices(List<Material> matList) //a doozy
        {
            List<VertexPositionNormalTextureTangentBinormal> tempVertList = new List<VertexPositionNormalTextureTangentBinormal>();

            Material tempMat;
            //Block tempBlock;
            //Tile tempTile;
            VertexPositionNormalTextureTangentBinormal[] tempVerts;

            int k, l;

            for (int i = 0; i < sGCD.GetLength(0); i++)
            {
                for (int j = 0; j < sGCD.GetLength(1); j++)
                {
                    if (sGCD[i, j] != null && sGCD[i, j].type.Equals(EntType.Platform))
                    {
                        tempMat = matSelection(sGCD[i, j].matName, matList);

                        tempVerts = tempMat.getFrontFace(4f); //front face
                        transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                        adjustTextureCoordinates(tempVerts, tempMat.textures[0][0][0]); //needs change to choose correct tex and norm
                        adjustNormalCoordinates(tempVerts, GetFrontTextureName(tempMat, i, j)); //tempMat.textures[48][0][0]);

                        //add tempVerts
                        for (k = 0; k < tempVerts.Length; k++)
                            tempVertList.Add(tempVerts[k]);

                        if (!_overlord.isOrtho)
                        {
                            if (i != sGCD.GetLength(0) - 1 && sGCD[i + 1, j] == null)
                                for (l = 0; l < 4; l++)
                                {
                                    tempVerts = tempMat.getTopFace((float)l); //top face
                                    transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                                    adjustTextureCoordinates(tempVerts, tempMat.textures[16][0][0]);
                                    adjustNormalCoordinates(tempVerts, GetTopTextureName(tempMat, i, j, l));

                                    //add tempVerts
                                    for (k = 0; k < tempVerts.Length; k++)
                                        tempVertList.Add(tempVerts[k]);
                                }

                            if (j != 0 && sGCD[i, j - 1] == null)
                                for (l = 0; l < 4; l++)
                                {
                                    tempVerts = tempMat.getLeftFace((float)l); //left face
                                    transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                                    adjustTextureCoordinates(tempVerts, tempMat.textures[0][0][0]);
                                    adjustNormalCoordinates(tempVerts, tempMat.textures[48][0][0]);

                                    //add tempVerts
                                    for (k = 0; k < tempVerts.Length; k++)
                                        tempVertList.Add(tempVerts[k]);
                                }

                            if (j != sGCD.GetLength(1) - 1 && sGCD[i, j + 1] == null) //note
                                for (l = 0; l < 4; l++)
                                {
                                    tempVerts = tempMat.getRightFace((float)l); //right face
                                    transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                                    adjustTextureCoordinates(tempVerts, tempMat.textures[0][0][0]);
                                    adjustNormalCoordinates(tempVerts, tempMat.textures[48][0][0]);

                                    //add tempVerts
                                    for (k = 0; k < tempVerts.Length; k++)
                                        tempVertList.Add(tempVerts[k]);
                                }

                            if (i != 0 && sGCD[i - 1, j] == null)
                                for (l = 0; l < 4; l++)
                                {
                                    tempVerts = tempMat.getBottomFace((float)l); //bottom face
                                    transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                                    adjustTextureCoordinates(tempVerts, tempMat.textures[32][0][0]);
                                    adjustNormalCoordinates(tempVerts, GetBottomTextureName(tempMat, i, j, l));

                                    //add tempVerts
                                    for (k = 0; k < tempVerts.Length; k++)
                                        tempVertList.Add(tempVerts[k]);
                                }
                        }

                    }
                    else if (sGCD[i, j] != null && sGCD[i, j].type.Equals(EntType.Ramp))
                    {
                        tempMat = matSelection(sGCD[i, j].matName, matList);

                        tempVerts = tempMat.getFrontFace(4f, ((Ramp)sGCD[i, j]).corner); //front face
                        transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                        adjustTextureCoordinates(tempVerts, tempMat.textures[0][0][0]); //needs change to choose correct tex and norm
                        adjustNormalCoordinates(tempVerts, GetFrontTextureName(tempMat, i, j)); //tempMat.textures[48][0][0]);

                        //add tempVerts
                        for (k = 0; k < tempVerts.Length; k++)
                            tempVertList.Add(tempVerts[k]);
                    }
                    else //background
                    {
                        /*tempMat = matList[0];
                        tempVerts = tempMat.getFrontFace(0f); //front face
                        transformSurfaceToPosition(tempVerts, i, j); //transform to proper position
                        adjustTextureCoordinates(tempVerts, tempMat.textures[0][0][0]); //needs change to choose correct tex and norm
                        adjustNormalCoordinates(tempVerts, tempMat.textures[48][0][0]);

                        //add tempVerts
                        for (k = 0; k < tempVerts.Length; k++)
                            tempVertList.Add(tempVerts[k]);*/
                    }
                }
            }

            solidGeomVertices = tempVertList.ToArray();
        }

        private String GetFrontTextureName(Material m, int x, int y) // needs mod and modular
        { //sometimes you write good code and sometimes you write bad code


            if (x < sGCD.GetLength(0) - 1 && sGCD[x + 1, y] == null) // up
            {
                if (y != 0 && sGCD[x, y - 1] == null) // left
                {
                    if (y < sGCD.GetLength(1) - 1 && sGCD[x, y + 1] == null) // right
                    {
                        if (x != 0 && sGCD[x - 1, y] == null) // down
                        {
                            return m.textures[63][0][0];
                        }
                        return m.textures[59][0][0];
                    }

                    if (x != 0 && sGCD[x - 1, y] == null) // down
                    {
                        return m.textures[61][0][0];
                    }

                    return m.textures[54][0][0];
                }

                if (y < sGCD.GetLength(1) - 1 && sGCD[x, y + 1] == null) // right
                {
                    if (x != 0 && sGCD[x - 1, y] == null) // down
                    {
                        return m.textures[60][0][0];
                    }
                    return m.textures[53][0][0];
                }

                if (x != 0 && sGCD[x - 1, y] == null) // down
                {
                    return m.textures[55][0][0];
                }

                return m.textures[49][0][0];
            }

            if (y < sGCD.GetLength(1) - 1 && sGCD[x, y + 1] == null) // right
            {
                if (y != 0 && sGCD[x, y - 1] == null) // left
                {
                    if (x != 0 && sGCD[x - 1, y] == null) // down
                    {
                        return m.textures[62][0][0];
                    }
                    return m.textures[57][0][0];
                }
                if (x != 0 && sGCD[x - 1, y] == null) // down
                {
                    return m.textures[56][0][0];
                }
                return m.textures[50][0][0];
            }

            if (y != 0 && sGCD[x, y - 1] == null) // left
            {
                if (x != 0 && sGCD[x - 1, y] == null) // down
                {
                    return m.textures[58][0][0];
                }
                return m.textures[51][0][0];
            }

            if (x != 0 && sGCD[x - 1, y] == null) // down
            {
                return m.textures[52][0][0];
            }

            

            return m.textures[48][0][0];
        }

        private String GetTopTextureName(Material m, int x, int y, int z)
        {
            if (z == 7)
                return m.textures[64+4][0][0]; //front

            return m.textures[64+0][0][0];
        }

        private String GetBottomTextureName(Material m, int x, int y, int z)
        {
            if (z == 7)
                return m.textures[80 + 1][0][0]; //front

            return m.textures[80 + 0][0][0];
        }

        private void transformSurfaceToPosition(VertexPositionNormalTextureTangentBinormal[] tempVerts, int row, int col)
        {
            for (int i = 0; i < tempVerts.Length; i++)
            {
                tempVerts[i].Position = new Vector4((tempVerts[i].Position.X + (float)col) * gridSize, (tempVerts[i].Position.Y + (float)row) * gridSize, tempVerts[i].Position.Z*gridSize, 1f);
            }
        }
        private void transformForegroundSurfaceToPosition(VertexPositionNormalTextureTangentBinormal[] tempVerts, int row, int col)
        {
            for (int i = 0; i < tempVerts.Length; i++)
            {
                tempVerts[i].Position = new Vector4((tempVerts[i].Position.X + (float)col) * gridSize, (tempVerts[i].Position.Y + (float)row) * gridSize, gridSize+0.5f, 1f);
            }
        }
        private void transformBackgroundSurfaceToPosition(VertexPositionNormalTextureTangentBinormal[] tempVerts, int row, int col)
        {
            for (int i = 0; i < tempVerts.Length; i++)
            {
                tempVerts[i].Position = new Vector4((tempVerts[i].Position.X + (float)col) * gridSize, (tempVerts[i].Position.Y + (float)row) * gridSize, 0f, 1f);
            }
        }
        private void adjustTextureCoordinates(VertexPositionNormalTextureTangentBinormal[] tempVerts, String tex)
        {
            Vector2 imageSize;
            Vector2 topLeft, bottomRight;

            Vector2 pos, scaledSize;

            int i;

            imageSize = new Vector2((float)solidGeomTex.Width, (float)solidGeomTex.Height);

            foreach (texLookup t in texLookupList)
            {
                if (t.texName.Equals(tex))
                {
                    topLeft = new Vector2(((float)t.topLeft.X + 2f) / imageSize.X, ((float)t.topLeft.Y + 2f) / imageSize.Y);
                    bottomRight = new Vector2(((float)t.bottomRight.X - 2f) / imageSize.X, ((float)t.bottomRight.Y - 2f) / imageSize.Y);
                    pos = topLeft;
                    scaledSize = bottomRight - topLeft;

                    for (i = 0; i < tempVerts.Length; i++)
                    {
                        tempVerts[i].TextureCoordinate = new Vector2(pos.X + scaledSize.X * tempVerts[i].TextureCoordinate.X, pos.Y + scaledSize.Y * tempVerts[i].TextureCoordinate.Y);
                    }
                }
            }
        }
        private void adjustNormalCoordinates(VertexPositionNormalTextureTangentBinormal[] tempVerts, String tex)
        {
            Vector2 imageSize;
            Vector2 topLeft, bottomRight;

            Vector2 pos, scaledSize;

            int i;

            imageSize = new Vector2((float)solidGeomNorm.Width, (float)solidGeomNorm.Height);

            foreach (texLookup t in normLookupList)
            {
                if (t.texName.Equals(tex))
                {
                    topLeft = new Vector2(((float)t.topLeft.X + 2f) / imageSize.X, ((float)t.topLeft.Y + 2f) / imageSize.Y);
                    bottomRight = new Vector2(((float)t.bottomRight.X - 2f) / imageSize.X, ((float)t.bottomRight.Y - 2f) / imageSize.Y);
                    pos = topLeft;
                    scaledSize = bottomRight - topLeft;

                    for (i = 0; i < tempVerts.Length; i++)
                    {
                        tempVerts[i].NormalCoordinate = new Vector2(pos.X + scaledSize.X * tempVerts[i].NormalCoordinate.X, pos.Y + scaledSize.Y * tempVerts[i].NormalCoordinate.Y);
                    }
                }
            }
        }
    }
}
