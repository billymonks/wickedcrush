using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WickedCrush.GameEntities
{
    public class Attack : Character //mostly melee attacks, they're invisible, only exist for one tick and then die
    {
        private int dmgAmount;
        private float force; //effects knockback / stun time / shield deflection recovery time
        private Direction atkDir;
        private Character creator; //don't attack your creator even if your hitboxes overlap
        private bool callBackOnHit;

        public Attack(ContentManager cm, GraphicsDevice gd, Vector2 pos, Vector2 size, int dmgAmount, float force,
            Direction atkDir, Character creator)
            : base(pos, size, new Vector2(1f, 1f), new Vector2(0f, 0f), 130f, gd)
        {
            this.dmgAmount = dmgAmount;
            this.force = force;
            this.atkDir = atkDir;
            this.creator = creator;
            this.callBackOnHit = false;
            CreateCharacter(cm);
        }

        public Attack(ContentManager cm, GraphicsDevice gd, Vector2 pos, Vector2 size, int dmgAmount, float force, bool callBack,
            Direction atkDir, Character creator)
            : base(pos, size, new Vector2(1f, 1f), new Vector2(0f, 0f), 130f, gd)
        {
            this.dmgAmount = dmgAmount;
            this.force = force;
            this.atkDir = atkDir;
            this.creator = creator;
            this.callBackOnHit = callBack;
            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 0;
            name = "Attack";
            walkThrough = true;
            ignorePlatforms = true;
            CreateAnimationList(cm);
            CreateStateMachine();
        }
        public void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("red-box", new Animation("red-box", cm)); //temp for testing
        }
        public void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("attack-load", new State("attack-load", //the only state that does anything
                c => sm.previousControlState == null,
                c =>
                {
                    currentAnimation = animationList["red-box"];
                    //find collisions and make them pay if they aren't creator or invuln
                    foreach (Entity ch in collisionList)
                    {
                        if (ch.type.Equals(EntType.Character))
                        {
                            if (!ch.invuln && !ch.Equals(creator))
                            {
                                ((Character)ch).TakeDamage(dmgAmount, force, atkDir);
                                if (callBackOnHit)
                                {
                                    creator.AttackCallback();
                                    callBackOnHit = false;
                                }
                            }
                        }
                    }


                    readyForRemoval = true; //only here for one tick so we're done
                }));
            stateList.Add("attack-done", new State("attack-done", //should never reach this
                c => sm.previousControlState != null,
                c =>
                {
                    readyForRemoval = true;
                }));

            sm = new StateMachine(stateList);
        }

    }
}
