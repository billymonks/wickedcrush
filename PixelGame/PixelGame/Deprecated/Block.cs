using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using WickedCrush.GameStates;

//blocks are like blueprints for the tiles that the world is made up of. they are only used at the time of level load.

namespace WickedCrush
{
    public class Block : IComparable<Block> //the blocks that the levels are made of. these are turned into one collection of vectors at level load that draws from a single compiled texture (possibly at level export)
    {
        #region fields
        public string name;
        public bool cTop, cBottom, cLeft, cRight; //full gridwall contacts (for culling hidden surfaces)
        public String tTop, tBottom, tFront, tBack, tLeft, tRight; //texture names
        public String nTop, nBottom, nFront, nBack, nLeft, nRight; //normal names
        public float[] hMap;
        public bool ceiling;
        public Vector2 size = new Vector2(256f, 256f);
        public int numOfBlanks = 0;
        #endregion

        #region init
        public Block()
        {
            hMap = new float[1];
            hMap[0] = 1f;
            ceiling = false;
            cTop = true;
            cBottom = true;
            cLeft = true;
            cRight = true;
            //detailedHitBox = new Rectangle[1];
            //detailedHitBox[0] = new Rectangle(0, 0, (int)size.X, (int)size.Y);
        }

        public Block(float[] h, bool c)
        {
            hMap = h;
            ceiling = c;

            setContacts();
            calcBlanks();
            //detailedHitBox = new Rectangle[hMap.Length];
        }

        private void calcBlanks()
        {
            numOfBlanks = 0;
            for (int i = 0; i < hMap.Length; i++)
            {
                if (hMap[i] == 0f)
                {
                    numOfBlanks++;
                }
            }
        }

        public void loadBlock(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement rootElement = doc.Element("block");
            XElement heightElement = rootElement.Element("height");
            XElement[] segmentElements = heightElement.Elements().ToArray<XElement>();
            XElement textureElement = rootElement.Element("textures");
            XElement normalElement = rootElement.Element("normals");
            int numberOfSegments;

            name = rootElement.Attribute("name").Value;
            size.X = int.Parse(rootElement.Attribute("size_x").Value);
            size.Y = int.Parse(rootElement.Attribute("size_y").Value);

            numberOfSegments = int.Parse(heightElement.Attribute("numberOfSegments").Value);
            ceiling = bool.Parse(heightElement.Attribute("ceiling").Value);

            hMap = new float[numberOfSegments];
            for (int i = 0; i < numberOfSegments; i++)
            {
                hMap[i] = float.Parse(segmentElements[i].Value);
            }

            tTop = textureElement.Element("top").Value;
            tBottom = textureElement.Element("bottom").Value;
            tFront = textureElement.Element("front").Value;
            tBack = textureElement.Element("back").Value;
            tLeft = textureElement.Element("left").Value;
            tRight = textureElement.Element("right").Value;

            nTop = normalElement.Element("top").Value;
            nBottom = normalElement.Element("bottom").Value;
            nFront = normalElement.Element("front").Value;
            nBack = normalElement.Element("back").Value;
            nLeft = normalElement.Element("left").Value;
            nRight = normalElement.Element("right").Value;

            setContacts();
        }

        private void setContacts()
        {
            if (hMap.Length == 1 && hMap[0] == 1f)//quick and sloppy check
            {
                cTop = true;
                cBottom = true;
                cLeft = true;
                cRight = true;
            }
            else
            {
                if (ceiling)
                {
                    cTop = true;
                }
                else
                {
                    cBottom = true;
                }
                if (hMap[0] == 1f)
                    cLeft = true;
                if (hMap[hMap.Length - 1] == 1f)
                    cRight = true;
            }

            foreach (float f in hMap)
            {
                if (f != 1f)
                {
                    if (ceiling)
                        cBottom = false;
                    else
                        cTop = false;
                }
                if (f == 0f)
                {
                    cBottom = false;
                    cTop = false;
                }
            }
        } // contacts let us cull surfaces that will never be seen. if collision detection changes, it could also let us cull collision surfaces
        #endregion

        public VertexPositionNormalTextureTangentBinormal[] getTopFace() 
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f,1f,0f);
            tangent = new Vector3(0f, 0f, -1f);
            binormal = Vector3.Cross(tangent, normal);

            if (ceiling)
            {
                vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];
                //back top left
                for (int i = 0; i < hMap.Length; i++)
                {
                    if (hMap[i] != 0f)
                    {
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f, 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //front top right
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i + 1) / ((float)hMap.Length), 1f, 0f, 1f),
                            normal,
                            new Vector2(((float)i + 1) / ((float)hMap.Length), 0f),
                            new Vector2(((float)i + 1) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //front top left
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f, 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //back top right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i + 1) / ((float)hMap.Length), 1f, 1f, 1f),
                            normal,
                            new Vector2(((float)i + 1) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i + 1) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        vertices[i * 6 + 4] = vertices[i * 6 + 1];
                        vertices[i * 6 + 5] = vertices[i * 6 + 0];
                    }
                }


            }
            else
            {
                vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];
                for (int i = 0; i < hMap.Length; i++)
                {
                    if (hMap[i] != 0f)
                    {
                        //back top left
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //front top right
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //front top left
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //back top right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //front top right
                        vertices[i * 6 + 4] = vertices[i * 6 + 1];
                        //back top left
                        vertices[i * 6 + 5] = vertices[i * 6 + 0];
                    }
                }
            }

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getBottomFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, -1f, 0f);
            tangent = new Vector3(0f, 0f, 1f);
            binormal = Vector3.Cross(tangent, normal);

            if (ceiling)
            {
                vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];
                for (int i = 0; i < hMap.Length; i++)
                {
                    if (hMap[i] != 0f)
                    {
                        //back bottom left
                        vertices[(i) * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i)) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)(i)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)(i)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //front bottom left
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //front bottom right
                        vertices[(i) * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4((float)(i + 1) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                            normal,
                            new Vector2((float)(i + 1) / ((float)hMap.Length), 1f),
                            new Vector2((float)(i + 1) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //back bottom right
                        vertices[(i) * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4((float)(i + 1) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                            normal,
                            new Vector2((float)(i + 1) / ((float)hMap.Length), 0f),
                            new Vector2((float)(i + 1) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        vertices[(i) * 6 + 4] = vertices[(i) * 6 + 0];
                        vertices[(i) * 6 + 5] = vertices[(i) * 6 + 2];
                    }
                }
            }
            else
            {
                vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];
                for (int i = 0; i < hMap.Length; i++)
                {
                    if (hMap[i] != 0f)
                    {
                        //back bottom left
                        vertices[(i) * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i)) / ((float)hMap.Length), 0f, 1f, 1f),
                            normal,
                            new Vector2(((float)(i)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)(i)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //front bottom left
                        vertices[(i) * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i)) / ((float)hMap.Length), 0f, 0f, 1f),
                            normal,
                            new Vector2(((float)(i)) / ((float)hMap.Length), 1f),
                            new Vector2(((float)(i)) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //front bottom right
                        vertices[(i) * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)((i) + 1)) / ((float)hMap.Length), 0f, 0f, 1f),
                            normal,
                            new Vector2(((float)((i) + 1)) / ((float)hMap.Length), 1f),
                            new Vector2(((float)((i) + 1)) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //back bottom right
                        vertices[(i) * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)((i) + 1)) / ((float)hMap.Length), 0f, 1f, 1f),
                            normal,
                            new Vector2(((float)((i) + 1)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)((i) + 1)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        vertices[(i) * 6 + 4] = vertices[(i) * 6 + 0];
                        vertices[(i) * 6 + 5] = vertices[(i) * 6 + 2];
                    }
                }
            }

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getFrontFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];

            for (int i = 0; i < hMap.Length; i++)
            {
                if (hMap[i] != 0f)
                {
                    if (ceiling)
                    {
                        //top front left
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f, 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //bottom front left
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), hMap[i]),
                            new Vector2(((float)i) / ((float)hMap.Length), hMap[i]),
                            tangent,
                            binormal);
                        //bottom front right
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), hMap[i]),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), hMap[i]),
                            tangent,
                            binormal);
                        //top front right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 1f, 1f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        vertices[i * 6 + 4] = vertices[i * 6 + 0];
                        vertices[i * 6 + 5] = vertices[i * 6 + 2];
                    }
                    else
                    {
                        //top front left
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f - hMap[i]),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f - hMap[i]),
                            tangent,
                            binormal);
                        //bottom front right
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 0f, 1f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //bottom front left
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 0f, 1f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //top front right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), hMap[i], 1f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i]),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i]),
                            tangent,
                            binormal);
                        vertices[i * 6 + 4] = vertices[i * 6 + 0];
                        vertices[i * 6 + 5] = vertices[i * 6 + 2];
                    }
                }
            }

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getBackFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];

            for (int i = 0; i < hMap.Length; i++)
            {
                if (hMap[i] != 0f)
                {
                    if (ceiling)
                    {
                        //top back left
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f, 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            new Vector2(((float)i) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        //bottom back left
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), hMap[i]),
                            new Vector2(((float)i) / ((float)hMap.Length), hMap[i]),
                            tangent,
                            binormal);
                        //bottom back right
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), hMap[i]),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), hMap[i]),
                            tangent,
                            binormal);
                        //top back right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 1f, 0f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 0f),
                            tangent,
                            binormal);
                        vertices[i * 6 + 4] = vertices[i * 6 + 1];
                        vertices[i * 6 + 5] = vertices[i * 6 + 0];
                    }
                    else
                    {
                        //top back left
                        vertices[i * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f - hMap[i]),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f - hMap[i]),
                            tangent,
                            binormal);
                        //bottom back right
                        vertices[i * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), 0f, 0f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //bottom back left
                        vertices[i * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)i) / ((float)hMap.Length), 0f, 0f, 1f),
                            normal,
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            new Vector2(((float)i) / ((float)hMap.Length), 1f),
                            tangent,
                            binormal);
                        //top back right
                        vertices[i * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                            new Vector4(((float)(i + 1)) / ((float)hMap.Length), hMap[i], 0f, 1f),
                            normal,
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i]),
                            new Vector2(((float)(i + 1)) / ((float)hMap.Length), 1f - hMap[i]),
                            tangent,
                            binormal);
                        vertices[i * 6 + 4] = vertices[i * 6 + 1];
                        vertices[i * 6 + 5] = vertices[i * 6 + 0];
                    }
                }
            }

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getLeftFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;
            int numFaces = 0;
            int k = 1;

            normal = new Vector3(-1f, 0f, 0f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);
            
            //count faces
            for (int j = 0; j < hMap.Length; j++)
            {
                if (j == 0)
                {
                    numFaces++;
                }
                else
                {
                    if (hMap[j] > hMap[j - 1])
                        numFaces++;
                }
            }

            vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];

            //left-most wall
            if (hMap[0] != 0f)
            {
                if (ceiling)
                {
                    //top back left
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 1f, 0f, 1f),
                           normal,
                           new Vector2(0f, 1f - hMap[0]),
                           new Vector2(0f, 1f - hMap[0]),
                           tangent,
                           binormal);
                    //bottom back left
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 1f - hMap[0], 0f, 1f),
                           normal,
                           new Vector2(0f, 1f),
                           new Vector2(0f, 1f),
                           tangent,
                           binormal);
                    //bottom front left
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 1f - hMap[0], 1f, 1f),
                           normal,
                           new Vector2(1f, 1f),
                           new Vector2(1f, 1f),
                           tangent,
                           binormal);
                    //top front left
                    vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 1f, 1f, 1f),
                           normal,
                           new Vector2(1f, 1f - hMap[0]),
                           new Vector2(1f, 1f - hMap[0]),
                           tangent,
                           binormal);
                    vertices[4] = vertices[0];
                    vertices[5] = vertices[2];
                }
                else
                {
                    //top back left
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, hMap[0], 0f, 1f),
                           normal,
                           new Vector2(0f, 0f),
                           new Vector2(0f, 0f),
                           tangent,
                           binormal);
                    //bottom back left
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 0f, 0f, 1f),
                           normal,
                           new Vector2(0f, hMap[0]),
                           new Vector2(0f, hMap[0]),
                           tangent,
                           binormal);
                    //bottom front left
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, 0f, 1f, 1f),
                           normal,
                           new Vector2(1f, hMap[0]),
                           new Vector2(1f, hMap[0]),
                           tangent,
                           binormal);
                    //top front left
                    vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(0f, hMap[0], 1f, 1f),
                           normal,
                           new Vector2(1f, 0f),
                           new Vector2(1f, 0f),
                           tangent,
                           binormal);
                    vertices[4] = vertices[0];
                    vertices[5] = vertices[2];
                }
            }

            //rest of left faces
            for (int i = 1; i < hMap.Length; i++)
            {
                if ((hMap[i] > hMap[i - 1]) && (hMap[i]>0f))
                {
                    if (ceiling)
                    {
                        //top back left
                        vertices[k * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i - 1], 0f, 1f),
                           normal,
                           new Vector2(0f, 1f - hMap[i] + hMap[i - 1]),
                           new Vector2(0f, 1f - hMap[i] + hMap[i - 1]),
                           tangent,
                           binormal);
                        //bottom back left
                        vertices[k * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                           normal,
                           new Vector2(0f, 1f),
                           new Vector2(0f, 1f),
                           tangent,
                           binormal);
                        //bottom front left
                        vertices[k * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                           normal,
                           new Vector2(1f, 1f),
                           new Vector2(1f, 1f),
                           tangent,
                           binormal);
                        //top front left
                        vertices[k * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), 1f - hMap[i - 1], 1f, 1f),
                           normal,
                           new Vector2(1f, 1f - hMap[i] + hMap[i - 1]),
                           new Vector2(1f, 1f - hMap[i] + hMap[i - 1]),
                           tangent,
                           binormal);

                        vertices[k * 6 + 4] = vertices[k * 6 + 0];
                        vertices[k * 6 + 5] = vertices[k * 6 + 2];

                    }
                    else
                    {
                        //top back left
                        vertices[k * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 0f, 1f),
                           normal,
                           new Vector2(0f, 0f),
                           new Vector2(0f, 0f),
                           tangent,
                           binormal);
                        //bottom back left
                        vertices[k * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), hMap[i-1], 0f, 1f),
                           normal,
                           new Vector2(0f, hMap[i]-hMap[i-1]),
                           new Vector2(0f, hMap[i] - hMap[i - 1]),
                           tangent,
                           binormal);
                        //bottom front left
                        vertices[k * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), hMap[i-1], 1f, 1f),
                           normal,
                           new Vector2(1f, hMap[i] - hMap[i - 1]),
                           new Vector2(1f, hMap[i] - hMap[i - 1]),
                           tangent,
                           binormal);
                        //top front left
                        vertices[k * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(((float)i) / ((float)hMap.Length), hMap[i], 1f, 1f),
                           normal,
                           new Vector2(1f, 0f),
                           new Vector2(1f, 0f),
                           tangent,
                           binormal);

                        vertices[k * 6 + 4] = vertices[k * 6 + 0];
                        vertices[k * 6 + 5] = vertices[k * 6 + 2];
                    }
                    k++;
                }
            }

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getRightFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;
            int numFaces = 0;
            int k = 1;

            normal = new Vector3(1f, 0f, 0f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            //count faces
            for (int j = 0; j < hMap.Length; j++)
            {
                if (j == 0)
                {
                    numFaces++;
                }
                else
                {
                    if (hMap[j] < hMap[j - 1])
                        numFaces++;
                }
            }

            vertices = new VertexPositionNormalTextureTangentBinormal[(hMap.Length - numOfBlanks) * 6];

            //right-most wall
            if (hMap[hMap.Length - 1] != 0f)
            {
                if (ceiling)
                {
                    //top back right
                    vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 1f, 0f, 1f),
                           normal,
                           new Vector2(1f, 1f - hMap[hMap.Length - 1]),
                           new Vector2(1f, 1f - hMap[hMap.Length - 1]),
                           tangent,
                           binormal);
                    //bottom back right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 1f - hMap[hMap.Length - 1], 0f, 1f),
                           normal,
                           new Vector2(1f, 1f),
                           new Vector2(1f, 1f),
                           tangent,
                           binormal);
                    //bottom front right
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 1f - hMap[hMap.Length - 1], 1f, 1f),
                           normal,
                           new Vector2(0f, 1f),
                           new Vector2(0f, 1f),
                           tangent,
                           binormal);
                    //top front right
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 1f, 1f, 1f),
                           normal,
                           new Vector2(0f, 1f - hMap[hMap.Length - 1]),
                           new Vector2(0f, 1f - hMap[hMap.Length - 1]),
                           tangent,
                           binormal);
                    vertices[4] = vertices[0];
                    vertices[5] = vertices[2];
                }
                else
                {
                    //top back right
                    vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, hMap[hMap.Length - 1], 0f, 1f),
                           normal,
                           new Vector2(1f, 0f),
                           new Vector2(1f, 0f),
                           tangent,
                           binormal);
                    //bottom back right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 0f, 0f, 1f),
                           normal,
                           new Vector2(1f, hMap[hMap.Length - 1]),
                           new Vector2(1f, hMap[hMap.Length - 1]),
                           tangent,
                           binormal);
                    //bottom front right
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, 0f, 1f, 1f),
                           normal,
                           new Vector2(0f, hMap[hMap.Length - 1]),
                           new Vector2(0f, hMap[hMap.Length - 1]),
                           tangent,
                           binormal);
                    //top front right
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                           new Vector4(1f, hMap[hMap.Length - 1], 1f, 1f),
                           normal,
                           new Vector2(0f, 0f),
                           new Vector2(0f, 0f),
                           tangent,
                           binormal);
                    vertices[4] = vertices[0];
                    vertices[5] = vertices[2];
                }
            }

            //rest of right faces
            for (int i = 1; i < hMap.Length; i++)
            {
                if (hMap[i-1] != 0f)
                {
                    if (hMap[i] < hMap[i - 1])
                    {
                        if (ceiling)
                        {
                            //top back right
                            vertices[k * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), 1f - hMap[i], 0f, 1f),
                               normal,
                               new Vector2(1f, 1f + hMap[i] - hMap[i - 1]),
                               new Vector2(1f, 1f + hMap[i] - hMap[i - 1]),
                               tangent,
                               binormal);
                            //bottom back right
                            vertices[k * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), 1f - hMap[i - 1], 0f, 1f),
                               normal,
                               new Vector2(1f, 1f),
                               new Vector2(1f, 1f),
                               tangent,
                               binormal);
                            //bottom front right
                            vertices[k * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), 1f - hMap[i - 1], 1f, 1f),
                               normal,
                               new Vector2(0f, 1f),
                               new Vector2(0f, 1f),
                               tangent,
                               binormal);
                            //top front right
                            vertices[k * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), 1f - hMap[i], 1f, 1f),
                               normal,
                               new Vector2(0f, 1f + hMap[i] - hMap[i - 1]),
                               new Vector2(0f, 1f + hMap[i] - hMap[i - 1]),
                               tangent,
                               binormal);

                            vertices[k * 6 + 4] = vertices[k * 6 + 0];
                            vertices[k * 6 + 5] = vertices[k * 6 + 2];

                        }
                        else
                        {
                            //top back right
                            vertices[k * 6 + 3] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), hMap[i - 1], 0f, 1f),
                               normal,
                               new Vector2(1f, 0f),
                               new Vector2(1f, 0f),
                               tangent,
                               binormal);
                            //bottom back right
                            vertices[k * 6 + 2] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), hMap[i], 0f, 1f),
                               normal,
                               new Vector2(1f, hMap[i - 1] - hMap[i]),
                               new Vector2(1f, hMap[i - 1] - hMap[i]),
                               tangent,
                               binormal);
                            //bottom front right
                            vertices[k * 6 + 1] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), hMap[i], 1f, 1f),
                               normal,
                               new Vector2(0f, hMap[i - 1] - hMap[i]),
                               new Vector2(0f, hMap[i - 1] - hMap[i]),
                               tangent,
                               binormal);
                            //top front right
                            vertices[k * 6 + 0] = new VertexPositionNormalTextureTangentBinormal(
                               new Vector4(((float)(i)) / ((float)hMap.Length), hMap[i - 1], 1f, 1f),
                               normal,
                               new Vector2(0f, 0f),
                               new Vector2(0f, 0f),
                               tangent,
                               binormal);

                            vertices[k * 6 + 4] = vertices[k * 6 + 0];
                            vertices[k * 6 + 5] = vertices[k * 6 + 2];
                        }
                        k++;
                    }
                }
            }

            return vertices;
        }

        public int CompareTo(Block b)
        {
            return name.CompareTo(b.name);
        }
    }
}
