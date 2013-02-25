using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
//using WonderKnightRadish;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using WickedCrush.GameEntities;

namespace Raditor
{
    public partial class EditorForm : Form
    {
        #region fields
        ContentManager _content;
        GraphicsDevice _graphics;
        Editor _editor;

        #endregion


        public enum EditorTool
        {
            Block = 1,
            Entity = 2,
            Background = 3,
            Foreground = 4
        }

        public EditorForm()
        {
            InitializeComponent();
        }
        public void getContentManager(ContentManager cm)
        {
            _content = cm;
        }
        public void getGraphicsDevice(GraphicsDevice gd)
        {
            _graphics = gd;
        }
        public void getEditor(Editor ed) //going to hell
        {
            _editor = ed;
        }

        public IntPtr getDrawSurface()
        {
            return pctSurface.Handle;
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            _editor.newLevel();
            //currentLevel = new Level();
            
        }

        private void pctSurface_Click(object sender, EventArgs e)
        {
            _editor.clickHandler(PointToClient(Cursor.Position).X - pctSurface.Location.X, PointToClient(Cursor.Position).Y - pctSurface.Location.Y);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*currentLevel.sGCD = new Platform[5, 10];
            Block a = new Block();
            a.name = "tittayz";
            Block b = new Block();
            b.name = "boobiez";

            currentLevel.sGCD[0, 1] = new Platform(0, 0, 0, 0, a);
            currentLevel.sGCD[4, 8] = new Platform(0, 0, 0, 0, b);*/
            

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "XML Files|*.xml";
            saveFileDialog1.Title = "Save your level file";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    _editor.levelSaver(saveFileDialog1.FileName);
                }
            }

        }
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.newLevel();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "XML Files|*.xml";
            openFileDialog1.Title = "Select a Level File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileName != "")
                {
                    _editor.levelLoader(openFileDialog1.FileName);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0) // Block
            {
                _editor.currentTool = EditorTool.Block;
                _editor.currentCharacter = null;
                setListBoxToBlocks();
            }
            if (comboBox1.SelectedIndex == 1) // Entity
            {
                _editor.currentTool = EditorTool.Entity;
                setListBoxToEntity();
            }
            if (comboBox1.SelectedIndex == 2) // Background
            {
                _editor.currentTool = EditorTool.Background;
                _editor.currentCharacter = null;
                setListBoxToTiles();
            }
            if (comboBox1.SelectedIndex == 3) // Foreground
            {
                _editor.currentTool = EditorTool.Foreground;
                _editor.currentCharacter = null;
                setListBoxToTiles();
            }
        }
        private void setListBoxToBlocks()
        {
            Object[] files;
            this.listBox1.Items.Clear();
            files = System.IO.Directory.GetFiles(@"Content\blocks", "*", SearchOption.AllDirectories);
            this.listBox1.Items.AddRange(files);
        }
        private void setListBoxToEntity()
        {
            this.listBox1.Items.Clear();
            this.listBox1.Items.AddRange(new object[] {
            "Level_Entrance",
            "Level_Exit",
            "Spike_Trap",
            "TreeMob",
            "CaveCrystal1",
            "CaveCrystal2",
            "CaveCrystal3",
            "Birdy",
            "Rhino"});
        }
        private void setListBoxToTiles()
        {
            Object[] files;
            this.listBox1.Items.Clear();
            files = System.IO.Directory.GetFiles(@"Content\tiles", "*", SearchOption.AllDirectories);
            this.listBox1.Items.AddRange(files);
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.currentTool.Equals(EditorTool.Block))
            {
                _editor.currentBlock.loadBlock(listBox1.SelectedItem.ToString());
            }
            else if ((_editor.currentTool.Equals(EditorTool.Background)) || (_editor.currentTool.Equals(EditorTool.Foreground)))
            {
                _editor.currentTile.loadTile(listBox1.SelectedItem.ToString());
            }
            else if (_editor.currentTool.Equals(EditorTool.Entity))
            {
                _editor.loadCurrentCharacter(listBox1.SelectedItem.ToString());
            }
        }

        private void normalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _editor.ReplaceViewMode(EditorViewMode.Dimension3);
        }

        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.ReplaceViewMode(EditorViewMode.Dimension2);
        }

        private void eraserCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _editor.eraseMode = eraserCheckBox.Checked;
        }

        private void dToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.ReplaceViewMode(EditorViewMode.Dimension2);
        }

        private void dToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _editor.ReplaceViewMode(EditorViewMode.Dimension3);
        }

        private void pctSurface_MouseMove(object sender, MouseEventArgs e)
        {
            _editor.mouseOverPosition(PointToClient(Cursor.Position).X - pctSurface.Location.X, PointToClient(Cursor.Position).Y - pctSurface.Location.Y);
        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            _editor.levelName = nameTextBox.Text;
        }

        private void authorTextBox_TextChanged(object sender, EventArgs e)
        {
            _editor.authorName = authorTextBox.Text;
        }

    }
}
