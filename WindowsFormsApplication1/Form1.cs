using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        String directory = "";
        String name = "";
        int spriteSize = 128;
        int rows = 1;
        int columns = 1;
        public ContentManager _content;
        public GraphicsDevice _graphics;

        Texture2D[] spriteList;
        Texture2D spriteSheet;

        public Form1()
        {
            InitializeComponent();
        }

        public Texture2D[] getSprites(String dir)
        {
            Object[] files;
            Texture2D[] spriteArray;
            FileStream fs;
            Texture2D tempTexture;
            files = System.IO.Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories);
            
            spriteArray = new Texture2D[files.Length];
            //this.listBox1.Items.AddRange(files);
            for (int i = 0; i < files.Length; i++)
            {
                fs = new FileStream((String)files[i], FileMode.Open);
                tempTexture = Texture2D.FromStream(_graphics,fs);
                spriteArray[i] = tempTexture;
            }
            return spriteArray;
        }

        public bool createSheet(Texture2D[] textures)
        {
            if (textures.Length == 0)
                return false; //wtf are you doing???

            XDocument doc = new XDocument();
            XElement rootElement = new XElement("animation");
            XElement attributes = new XElement("attributes");

            rootElement.Add(new XAttribute("name", name));

            

            SpriteBatch sb = new SpriteBatch(_graphics);
            RenderTarget2D renderTarget;
            Point frameSize = new Point(textures[0].Width, textures[0].Height); //god have mercy on your soul if textures is empty or the sprites aren't the same size
            Texture2D completeSpriteSheet;

            rows = (int)Math.Ceiling(Math.Sqrt(textures.Length));
            columns = (int)Math.Ceiling((double)textures.Length / (double)rows);

            renderTarget = new RenderTarget2D(
                _graphics,
                columns * frameSize.X,
                rows * frameSize.Y,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24);

            _graphics.SetRenderTarget(renderTarget);
            _graphics.Clear(ClearOptions.Target, new Color(0f, 0f, 0f, 0f), 0, 0);

            sb.Begin();

            for (int i = 0; i < textures.Length; i++)
            {
                sb.Draw(textures[i], new Rectangle((i % columns) * frameSize.X, (i / columns) * frameSize.Y, frameSize.X, frameSize.Y), Color.White);
            }

            sb.End();

            _graphics.SetRenderTarget(null);
            completeSpriteSheet = renderTarget;
            completeSpriteSheet.SaveAsPng(new FileStream(name + ".png", FileMode.Create), completeSpriteSheet.Width, completeSpriteSheet.Height);

            attributes.Add(new XAttribute("frameSize.X", frameSize.X));
            attributes.Add(new XAttribute("frameSize.Y", frameSize.Y));
            attributes.Add(new XAttribute("sheetSize.X", columns));
            attributes.Add(new XAttribute("sheetSize.Y", rows));
            attributes.Add(new XAttribute("totalFrames", textures.Length));
            attributes.Add(new XAttribute("frameInterval", 60)); //default

            rootElement.Add(attributes);
            doc.Add(rootElement);

            doc.Save(name + ".xml");

            return true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            directory = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            name = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            spriteList = getSprites(directory);
            createSheet(spriteList);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select animation directory";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                    directory = dialog.SelectedPath;
                }
            }
        }
    }
}
