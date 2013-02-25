using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class CaveCrystal2 : Character
    {
        public CharacterFactory _cf;

        public CaveCrystal2(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf)
            : base(pos, new Vector2(196f, 196f), new Vector2(0.5f, 1f), new Vector2(-49f, 0f), 230f, gd)
        {
            _cf = cf;

            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 0;
            name = "CaveCrystal2";
            walkThrough = true;
            ignorePlatforms = true;
            invuln = true;

            CreateAnimationList(cm);
            CreateStateMachine();

            /*_cf.AddLightToList(new PointLight(this,
                350f,
                new Vector4(0.2f, 0.6f, 0.6f, 1f),
                new Vector4(0.1f, 0.5f, 0.5f, 1f),
                450f,
                0.8f,
                new Vector2(98f, 98f)));*/

            if (_cf != null)
                _cf.AddLightToList(new GlowingPointLight(this,
                    350f,
                    new Vector4(0.2f, 0.6f, 0.6f, 1f),
                    new Vector4(0.1f, 0.5f, 0.5f, 1f),
                    450f,
                    0.8f,
                    new Vector2(98f, 98f),
                    0.1f,
                    500));
        }

        public void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("cave_crystal_2", new Animation("cave_crystal_2", cm));
            animationList.Add("cave_crystal_2_normal", new Animation("cave_crystal_2_normal", cm));

            defaultTexture = animationList["cave_crystal_2"].animationSheet;
            defaultNormal = animationList["cave_crystal_2_normal"].animationSheet;
        }

        public void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("default", new State("default", //the only state that does anything
                c => true,
                c =>
                {
                    currentAnimation = animationList["cave_crystal_2"];
                    currentAnimationNorm = animationList["cave_crystal_2_normal"];



                    //readyForRemoval = true;
                }));
            sm = new StateMachine(stateList);
        }
    }
}
