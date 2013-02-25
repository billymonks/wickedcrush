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
    //brings game back to level select screen, in future will trigger loading of next level
    public class LevelExit : Character
    {
        //Stack<GameState> _cGameState;
        //SoundManager _sound;
        Overlord _overlord;

        public LevelExit(Vector2 pos, Overlord overlord)
            : base(pos, new Vector2(108f, 128f), new Vector2(0.1f, 0.1f), new Vector2(-48.6f, 0f), 1.5f, overlord._gd)
        {
            _overlord = overlord;
            //_cGameState = cGameState;
            //_sound = sound;
            CreateCharacter(_overlord._cm);
            walkThrough = true;
            invuln = true;
        }

        public void CreateCharacter(ContentManager cm)
        {
            name = "Level_Exit";
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
                    
                }));

            stateList.Add("door-chilling", new State("door-chilling",
                c => sm.previousControlState != null,
                c =>
                {
                    //currentAnimation = animationList["placeholder-door"];
                    //currentAnimationNorm = animationList["placeholder-door-normal"];

                    for (int i = 0; i < collisionList.Count; i++)
                    {
                        if (collisionList[i].name.Equals("Hero"))
                        {
                            //_sound.stopAmbientLoops();
                            //_cGameState.Pop(); //needs to be heavily changed obviously, probably shouldn't pass gamestate directly, needs overlord container
                            _overlord.returnToLevelMenu();
                        }
                    }

                }));

            sm = new StateMachine(stateList);

        }
    }
}
