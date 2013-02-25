using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WickedCrush;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;

namespace BlockGenerator
{
    public partial class Form1 : Form
    {

        Block editorBlock;

        public Form1()
        {
            editorBlock = new Block();

            InitializeComponent();
        }

        private void genTemplateButton_Click(object sender, EventArgs e) //screw this just put that stuff in manually
        {
            genDefaultBlock();
        }

        private void genDefaultBlock()
        {
            int numOfSegments = 1;

            XDocument doc = new XDocument();
            XElement rootElement = new XElement("block");
            XElement heightElement = new XElement("height");
            XElement textureElement = new XElement("textures");
            XElement normalElement = new XElement("normals");

            rootElement.Add(new XAttribute("name", "BLOCK_NAME"));
            rootElement.Add(new XAttribute("size_x", 256));
            rootElement.Add(new XAttribute("size_y", 256));

            heightElement.Add(new XAttribute("numberOfSegments", numOfSegments));
            heightElement.Add(new XAttribute("ceiling", false));

            /*for (int i = 0; i < numOfSegments; i++)
            {

            }*/

            heightElement.Add(new XElement("segment", 1f));

            textureElement.Add(new XElement("top", "null"),
                new XElement("bottom", "null"),
                new XElement("front", "null"),
                new XElement("back", "null"),
                new XElement("left", "null"),
                new XElement("right", "null"));

            normalElement.Add(new XElement("top", "null"),
                new XElement("bottom", "null"),
                new XElement("front", "null"),
                new XElement("back", "null"),
                new XElement("left", "null"),
                new XElement("right", "null"));

            rootElement.Add(heightElement);
            rootElement.Add(textureElement);
            rootElement.Add(normalElement);

            doc.Add(rootElement);

            doc.Save("default_block.xml");
        }
    }
}
