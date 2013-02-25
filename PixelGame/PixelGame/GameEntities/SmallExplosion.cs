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
    public class SmallExplosion : Character
    {
        CharacterFactory _cf;

        public SmallExplosion(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf)
            : base(pos, new Vector2(105f, 105f), new Vector2(1f, 1f), new Vector2(0f, 0f), 128f, gd)
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
            name = "SmallExplosion";
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
                    240f,
                    new Vector4(1f, 0.27f, 0f, 1f),
                    new Vector4(1f, 0.55f, 0f, 1f),
                    450f,
                    0.1f,
                    new Vector2(52.5f, 52.5f),
                    0.4f,
                    200));
        }
        private void CreateAnimationList(ContentManager cm)
        {
            Animation temp;
            animationList = new Dictionary<string, Animation>();
            temp = new Animation("assplosion", cm);
            temp.loop = false;
            animationList.Add("explosion", temp);

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
