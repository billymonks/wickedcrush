using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class BigExplosion : Character
    {
        CharacterFactory _cf;

        public BigExplosion(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf)
            : base(pos, new Vector2(256f, 256f), new Vector2(1f, 1f), new Vector2(0f, 0f), 128f, gd)
        {
            _cf = cf;

            CreateCharacter(cm);
            walkThrough = true;
            invuln = true;
            ignorePlatforms = true;
            bright = true;
        }
        public void CreateCharacter(ContentManager cm)
        {
            name = "BigExplosion";
            CreateAnimationList(cm);
            CreateStateMachine();

            /*_cf.AddLightToList(new PointLight(this,
                300f,
                new Vector4(1f, 0.8f, 0.1f, 1f),
                new Vector4(1f, 0.5f, 0.2f, 1f),
                450f,
                0.5f,
                new Vector2(52.5f, 52.5f)));*/

            if (_cf != null)
                _cf.AddLightToList(new GlowingPointLight(this,
                    440f,
                    new Vector4(1f, 0.47f, 0f, 1f),
                    new Vector4(1f, 0.85f, 0f, 1f),
                    550f,
                    0.1f,
                    new Vector2(52.5f, 52.5f),
                    0.6f,
                    300));
        }
        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<string, Animation>();
            animationList.Add("explosion", new Animation("assplosion", cm));
            animationList["explosion"].loop = false;
            animationList["explosion"].frameInterval = TimeSpan.FromMilliseconds(60);

            defaultTexture = animationList["explosion"].animationSheet;
        }
        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("default", new State("default",
                c => true,
                c =>
                {
                    currentAnimation = animationList["explosion"];
                    if(currentAnimation.complete)
                        readyForRemoval = true;
                }));
            sm = new StateMachine(stateList);
        }
    }
}
