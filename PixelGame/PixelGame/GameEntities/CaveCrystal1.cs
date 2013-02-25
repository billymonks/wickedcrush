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
    public class CaveCrystal1 : Character
    {
        public CharacterFactory _cf;

        public CaveCrystal1(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf)
            : base(pos, new Vector2(196f, 196f), new Vector2(1f, 1f), new Vector2(0f, 0f), 184f, gd)
        {
            _cf = cf;

            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 0;
            name = "CaveCrystal1";
            walkThrough = true;
            ignorePlatforms = true;
            invuln = true;

            CreateAnimationList(cm);
            CreateStateMachine();

            /*_cf.AddLightToList(new PointLight(this,
                350f,
                new Vector4(0.5f, 0.5f, 0.8f, 1f),
                new Vector4(0.4f, 0.4f, 0.7f, 1f),
                450f,
                0.8f,
                new Vector2(98f, 98f)));*/

            if (_cf != null)
                _cf.AddLightToList(new GlowingPointLight(this,
                    350f,
                    new Vector4(0.5f, 0.5f, 0.8f, 1f),
                    new Vector4(0.4f, 0.4f, 0.7f, 1f),
                    450f,
                    0.8f,
                    new Vector2(98f, 158f),
                    0.1f,
                    500));
        }

        public void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("cave_crystal_1", new Animation("cave_crystal_1", cm));
            animationList.Add("cave_crystal_1_normal", new Animation("cave_crystal_1_normal", cm));

            defaultTexture = animationList["cave_crystal_1"].animationSheet;
            defaultNormal = animationList["cave_crystal_1_normal"].animationSheet;
        }

        public void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("default", new State("default", //the only state that does anything
                c => true,
                c =>
                {
                    currentAnimation = animationList["cave_crystal_1"];
                    currentAnimationNorm = animationList["cave_crystal_1_normal"];



                    //readyForRemoval = true;
                }));
            sm = new StateMachine(stateList);
        }
    }
}
