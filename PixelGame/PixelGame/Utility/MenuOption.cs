using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WickedCrush
{
    public class MenuOption
    {
        String name;
        Texture2D unSelectedTex;
        Texture2D selectedTex;
        bool isSelected;

        Rectangle pos;

        public MenuOption(Menu menu)
        {
            name = "unnamed";
            unSelectedTex = null;
            selectedTex = null;
            isSelected = false;

            pos = new Rectangle(0, 0, 0, 0);
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void setUnSelectedTex(Texture2D t)
        {
            unSelectedTex = t;
        }
        public void setSelectedTex(Texture2D t)
        {
            selectedTex = t;
        }
        public Texture2D getTexture()
        {
            if (isSelected)
                return selectedTex;
            else
                return unSelectedTex;
        }
        public Rectangle getPos()
        {
            return pos;
        }

    }
}
