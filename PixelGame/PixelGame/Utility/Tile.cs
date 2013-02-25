using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using WickedCrush.GameStates;

namespace WickedCrush
{
    //Tiles are a lot like blocks, but they are only for background or foreground, and only for decoration
    // (no collision data, always same size)
    public class Tile : IComparable<Tile>
    {
        #region fields
        public string name;
        public string tex, norm;
        public Vector2 size = new Vector2(256f, 256f);
        public float depth = 0f;
        #endregion

        #region init
        public Tile()
        {
            name = "";
            tex = "null";
            norm = "null";
        }

        public void loadTile(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement rootElement = doc.Element("tile");
            XElement textureElement = rootElement.Element("texture");
            XElement normalElement = rootElement.Element("normal");
            XElement depthElement = rootElement.Element("depth");

            name = rootElement.Attribute("name").Value;
            size.X = int.Parse(rootElement.Attribute("size_x").Value);
            size.Y = int.Parse(rootElement.Attribute("size_y").Value);
            depth = int.Parse(rootElement.Attribute("depth").Value);

            tex = textureElement.Value;
            norm = normalElement.Value;
        }
        #endregion

        public VertexPositionNormalTextureTangentBinormal[] getFace()
        {
            VertexPositionNormalTextureTangentBinormal[] vertices;
            Vector3 normal, tangent, binormal;

            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            vertices = new VertexPositionNormalTextureTangentBinormal[6];

            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 1f, depth, 1f),
                normal,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                tangent,
                binormal);

            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(0f, 0f, depth, 1f),
                normal,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                tangent,
                binormal);

            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                new Vector4(1f, 0f, depth, 1f),
                normal,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                tangent,
                binormal);
            
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

        public int CompareTo(Tile t)
        {
            return name.CompareTo(t.name);
        }
    }
}
