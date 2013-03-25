using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using WickedCrush.GameStates;

namespace WickedCrush.Utility
{
    public enum Corner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomRight = 2,
        BottomLeft = 3
    }
    public struct MatTex
    {
        public byte index;
        public byte key; //for matching textures to normals if multiple textures or normals
        public String texName;
    }
    public class Material
    {
        public String name;
        //public List<MatTex> textures;
        public String[][][] textures;

        public Material()
        {
            this.name = "";
            //textures = new List<MatTex>();
            textures = new String[96][][];
        }

        public Material(String name)
        {
            this.name = name;
            //textures = new List<MatTex>();
            textures = new String[96][][];
        }

        public void loadMat(string path)
        {
            MatTex tempMatTex;
            List<String> tempTexList;
            byte prevIndex = 255; //impossible
            byte prevKey = 255; //don't have this many keys but seriously

            XDocument doc = XDocument.Load(path);
            XElement rootElement = doc.Element("material");
            XElement[] textureElements = rootElement.Elements().ToArray<XElement>();

            name = rootElement.Attribute("name").Value;
            //textures = new List<MatTex>();
            textures = new String[96][][];
            tempTexList = new List<String>();

            for (int i = 0; i < textureElements.Length; i++)
            {
                tempMatTex = new MatTex();
                tempMatTex.index = byte.Parse(textureElements[i].Attribute("index").Value);
                tempMatTex.key = byte.Parse(textureElements[i].Attribute("key").Value);

                if (tempMatTex.index != prevIndex)
                {
                    textures[tempMatTex.index] = new String[tempMatTex.key + 1][];
                    if (prevIndex != 255)
                    {
                        textures[prevIndex][prevKey] = tempTexList.ToArray();
                        tempTexList = new List<String>();
                    }
                }
                else if (tempMatTex.key != prevKey)
                {
                    if (prevKey != 255)
                    {
                        textures[prevIndex][prevKey] = tempTexList.ToArray();
                        tempTexList = new List<String>();
                    }
                }

                //tempMatTex.texName = textureElements[i].Value;
                tempTexList.Add(textureElements[i].Value);
                //textures.Add(tempMatTex);

                prevIndex = tempMatTex.index;
                prevKey = tempMatTex.key;
            }
            textures[prevIndex][prevKey] = tempTexList.ToArray();

        }

        public VertexPositionNormalTextureTangentBinormal[] getFrontFace(float depth)
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];

            //top front left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 1f, depth, 1f),
                normal,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                tangent,
                binormal);
            //bottom front left
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 0f, depth, 1f),
                normal,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                tangent,
                binormal);
            //bottom front right
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 0f, depth, 1f),
                normal,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                tangent,
                binormal);
            //top front right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 1f, depth, 1f),
                normal,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                tangent,
                binormal);
            vertices[4] = vertices[0];
            vertices[5] = vertices[2];

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getFrontFace(float depth, Corner corner)
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[3];

            switch (corner)
            {
                case Corner.BottomLeft:
                    //top front left
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 1f, depth, 1f),
                        normal,
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f),
                        tangent,
                        binormal);
                    //bottom front left
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 0f, depth, 1f),
                        normal,
                        new Vector2(0f, 1f),
                        new Vector2(0f, 1f),
                        tangent,
                        binormal);
                    //bottom front right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 0f, depth, 1f),
                        normal,
                        new Vector2(1f, 1f),
                        new Vector2(1f, 1f),
                        tangent,
                        binormal);
                    break;

                case Corner.BottomRight:
                    //bottom front left
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 0f, depth, 1f),
                        normal,
                        new Vector2(0f, 1f),
                        new Vector2(0f, 1f),
                        tangent,
                        binormal);
                    //bottom front right
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 0f, depth, 1f),
                        normal,
                        new Vector2(1f, 1f),
                        new Vector2(1f, 1f),
                        tangent,
                        binormal);
                    //top front right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 1f, depth, 1f),
                        normal,
                        new Vector2(1f, 0f),
                        new Vector2(1f, 0f),
                        tangent,
                        binormal);
                    break;

                case Corner.TopLeft:
                    //top front left
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 1f, depth, 1f),
                        normal,
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f),
                        tangent,
                        binormal);
                    //bottom front left
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 0f, depth, 1f),
                        normal,
                        new Vector2(0f, 1f),
                        new Vector2(0f, 1f),
                        tangent,
                        binormal);
                    //top front right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 1f, depth, 1f),
                        normal,
                        new Vector2(1f, 0f),
                        new Vector2(1f, 0f),
                        tangent,
                        binormal);
                    break;

                case Corner.TopRight:
                    //top front right
                    vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 1f, depth, 1f),
                        normal,
                        new Vector2(1f, 0f),
                        new Vector2(1f, 0f),
                        tangent,
                        binormal);
                    //top front left
                    vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(0f, 1f, depth, 1f),
                        normal,
                        new Vector2(0f, 0f),
                        new Vector2(0f, 0f),
                        tangent,
                        binormal);
                    //bottom front right
                    vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                        new Vector4(1f, 0f, depth, 1f),
                        normal,
                        new Vector2(1f, 1f),
                        new Vector2(1f, 1f),
                        tangent,
                        binormal);
                    break;
            }
            

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getTopFace(float depth) //8 segments from rear to front
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 1f, 0f);
            tangent = new Vector3(0f, 0f, -1f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];

            //back top left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 1f, depth + 1f, 1f),
                normal,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                tangent,
                binormal);
            //front top right
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 1f, depth, 1f),
                normal,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                tangent,
                binormal);
            //front top left
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 1f, depth, 1f),
                normal,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                tangent,
                binormal);
            //back top right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 1f, depth + 1f, 1f),
                normal,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                tangent,
                binormal);
            //front top right
            vertices[4] = vertices[1];
            //back top left
            vertices[5] = vertices[0];

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getBottomFace(float depth)
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, -1f, 0f);
            tangent = new Vector3(0f, 0f, 1f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];

            //back bottom left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 0f, depth + 1f, 1f),
                normal,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                tangent,
                binormal);
            //front bottom left
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 0f, depth, 1f),
                normal,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                tangent,
                binormal);
            //front bottom right
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 0f, depth, 1f),
                normal,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                tangent,
                binormal);
            //back bottom right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 0f, depth + 1f, 1f),
                normal,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                tangent,
                binormal);
            vertices[4] = vertices[0];
            vertices[5] = vertices[2];

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getLeftFace(float depth)
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(-1f, 0f, 0f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];

            //top back left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                    new Vector4(0f, 1f, depth, 1f),
                    normal,
                    new Vector2(0f, 0f),
                    new Vector2(0f, 0f),
                    tangent,
                    binormal);
            //bottom back left
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                    new Vector4(0f, 0f, depth, 1f),
                    normal,
                    new Vector2(0f, 1f),
                    new Vector2(0f, 1f),
                    tangent,
                    binormal);
            //bottom front left
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                    new Vector4(0f, 0f, depth + 1f, 1f),
                    normal,
                    new Vector2(1f, 1f),
                    new Vector2(1f, 1f),
                    tangent,
                    binormal);
            //top front left
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                    new Vector4(0f, 1f, depth + 1f, 1f),
                    normal,
                    new Vector2(1f, 0f),
                    new Vector2(1f, 0f),
                    tangent,
                    binormal);
            vertices[4] = vertices[0];
            vertices[5] = vertices[2];

            return vertices;
        }

        public VertexPositionNormalTextureTangentBinormal[] getRightFace(float depth)
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(1f, 0f, 0f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];


            //top front right
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                   new Vector4(1f, 1f, depth + 1f, 1f),
                   normal,
                   new Vector2(0f, 0f),
                   new Vector2(0f, 0f),
                   tangent,
                   binormal);
            //bottom front right
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                   new Vector4(1f, 0f, depth + 1f, 1f),
                   normal,
                   new Vector2(0f, 1f),
                   new Vector2(0f, 1f),
                   tangent,
                   binormal);
            //bottom back right
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                   new Vector4(1f, 0f, depth, 1f),
                   normal,
                   new Vector2(1f, 1f),
                   new Vector2(1f, 1f),
                   tangent,
                   binormal);
            //top back right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                   new Vector4(1f, 1f, depth, 1f),
                   normal,
                   new Vector2(1f, 0f),
                   new Vector2(1f, 0f),
                   tangent,
                   binormal);
            vertices[4] = vertices[0];
            vertices[5] = vertices[2];

            return vertices;
        }
    }
}
