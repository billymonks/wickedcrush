using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WickedCrush.GameEntities
{
    //places hero in the world
    public class LevelEntrance : Character
    {
        public Hero hero;

        public LevelEntrance(ContentManager cm, GraphicsDevice gd, Vector2 pos, Hero hero)
            : base(pos, new Vector2(108f, 128f), new Vector2(1f, 1f), new Vector2(0f, 0f), 1.5f, gd)
        {
            CreateCharacter(cm);
            walkThrough = true;
            this.hero = hero;
            //this.hero.pos = this.pos;
            invuln = true;
        }

        public void CreateCharacter(ContentManager cm)
        {
            name = "Level_Entrance";
            CreateAnimationList(cm);
            CreateStateMachine();
        }

        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("placeholder-door", new Animation("placeholder-door", cm));
            animationList.Add("placeholder-door-normal", new Animation("placeholder-door-normal", cm));

            defaultTexture = animationList["placeholder-door"].animationSheet;
            defaultNormal = animationList["placeholder-door-normal"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("door-load", new State("door-load",
                c => sm.previousControlState == null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["placeholder-door"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["placeholder-door"];
                    currentAnimationNorm = animationList["placeholder-door-normal"];
                    hero.pos = this.pos+new Vector2(35f,0f);
                }));

            stateList.Add("door-chilling", new State("door-chilling",
                c => sm.previousControlState != null,
                c =>
                {
                    currentAnimation = animationList["placeholder-door"];
                    currentAnimationNorm = animationList["placeholder-door-normal"];
                    //if (currentAnimation != null && !currentAnimation.Equals(animationList["placeholder-door"]))
                        //currentAnimation.ResetAnimation();
                    //currentAnimation = animationList["placeholder-door"];
                    //currentAnimationNorm = animationList["placeholder-door-normal"];
                }));

            sm = new StateMachine(stateList);

        }
    }
}
