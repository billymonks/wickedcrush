using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.GameEntities;
using System.Timers;

namespace WickedCrush.GameEntities
{
    class Flametosser : Character
    {
        public CharacterFactory _cf;

        private Timer timer;

        public Flametosser(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf, Direction d)
            : base(pos, new Vector2(32f, 32f), new Vector2(1f, 1f), new Vector2(0f, 0f), 128f, gd)
        {
            _cf = cf;
            facingDir = d;

            CreateCharacter(cm);
            invuln = true;
        }

        public void CreateCharacter(ContentManager cm)
        {
            name = "Flametosser";
            CreateAnimationList(cm);
            CreateStateMachine();

            //facingDir = Direction.Right;
            specular = 0f;

            StartFlameTimer();
        }

        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<string, Animation>();
            animationList.Add("flametosser", new Animation("flametosser", cm));
            animationList.Add("ftn", new Animation("ftn", cm));

            defaultTexture = animationList["flametosser"].animationSheet;
            defaultNormal = animationList["ftn"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("ft-default", new State("ft-default",
                c => true,
                c =>
                {
                    currentAnimation = animationList["flametosser"];
                    currentAnimationNorm = animationList["ftn"];

                }));
            sm = new StateMachine(stateList);
        }

        private void StartFlameTimer()
        {
            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(ShootFlame);

            timer.Interval = 1600;
            timer.Enabled = true;
        }

        private void ShootFlame(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;

            if (facingDir.Equals(Direction.Left))
            {
                _cf.AddCharacterToList(
                        new Fireball(_cf._cm, _cf._gd,
                            pos + new Vector2(0f, 16f),
                            1,
                            2f,
                            facingDir,
                            this,
                            _cf));
            }
            else
            {
                _cf.AddCharacterToList(
                        new Fireball(_cf._cm, _cf._gd,
                            pos + new Vector2(30f, 16f),
                            1,
                            2f,
                            facingDir,
                            this,
                            _cf));
            }

            StartFlameTimer();
        }
    }
}
