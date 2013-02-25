using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WickedCrush
{
    public enum CharState
    {
        Standing = 0,
        Walking = 1,
        Falling = 2,
        Jumping = 3,
        Dying = 4,
        Victory = 5
    }
    public enum Direction
    {
        Left = 0,
        Right = 1,
        Up = 2,
        Down = 3,
        None = 4
    }

    class PlayerCharacter
    {
        /*
        KeyboardState keyState, prevKeyState;
        int maxSpeed = 12;
        int walkSpeed = 8;
        int timer, endTimer;
        CharState currentState = CharState.Standing;
        Direction currentDirection = Direction.Right;
        bool feetOnGround;
        

        public PlayerCharacter()
        {
            type = EntType.PC;
            pos = new Vector2(0, 0);
            size = new Vector2(0, 0);
            offset = new Vector2(0, 0);
            hitZone = new Vector2(1f, 1f);
            velocity = new Vector2(0, 0);
            graphicBox = new Rectangle(0, 0, 0, 0);
            hitBox = new Rectangle(0, 0, 0, 0);
            tex = null;

            keyState = Keyboard.GetState();
        }

        public PlayerCharacter(int x, int y, int width, int height)
        {
            type = EntType.PC;
            pos = new Vector2(x, y);
            size = new Vector2(width, height);
            offset = new Vector2(0, 0);
            hitZone = new Vector2(1f, 1f);
            velocity = new Vector2(0, 0);
            graphicBox = new Rectangle(x, y, width, height);
            hitBox = new Rectangle((int)(x + offset.X), (int)(y + offset.Y), (int)(width * hitZone.X), (int)(height * hitZone.Y));
            tex = null;

            keyState = Keyboard.GetState();
        }

        public PlayerCharacter(int x, int y, int width, int height, Texture2D t)
        {
            type = EntType.PC;
            pos = new Vector2(x, y);
            size = new Vector2(width, height);
            offset = new Vector2(0, 0);
            hitZone = new Vector2(1f, 1f);
            velocity = new Vector2(0, 0);
            graphicBox = new Rectangle(x, y, width, height);
            hitBox = new Rectangle((int)(x+offset.X), (int)(y+offset.Y), (int)(width*hitZone.X), (int)(height*hitZone.Y));
            tex = t;

            keyState = Keyboard.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            resolveCollisions();
            handleInput();

            switch(currentState)
            {
                case CharState.Standing:
                    stopWalk();
                    if (!feetOnGround)
                        currentState = CharState.Falling;
                    enforceMaxSpeed();
                    break;

                case CharState.Falling:
                    applyForce(new Vector2(0, 1)); //force of gravity
                    if(feetOnGround)
                        currentState = CharState.Standing;
                    enforceMaxSpeed();
                    break;

                case CharState.Jumping:
                    //continueJump(gameTime);
                    enforceMaxSpeed();
                    break;
                case CharState.Walking:
                    walk(currentDirection);
                    enforceMaxSpeed();
                    break;
                    
            }

            
            pos += velocity;
            updateBox();
            
        }
        public override Texture2D getTexture()
        {
            return tex;
        }
        public override void setTexture(Texture2D t)
        {
            tex = t;
        }
        public override Rectangle getGraphicBox()
        {
            return graphicBox;
        }
        public override Rectangle getHitBox()
        {
            return hitBox;
        }
        public void resolveCollisions() // needs to be re-implemented hardkore with a backward k
        {
            
        }
        public void handleInput() //sloppy
        {
            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.Left))
            {
                if (currentState == CharState.Walking || currentState == CharState.Standing)
                {
                    currentDirection = Direction.Left;
                    currentState = CharState.Walking;
                }
                else if (currentState == CharState.Falling || currentState == CharState.Jumping)
                {
                    midAirNudge(Direction.Left);
                }
            }
            if (keyState.IsKeyDown(Keys.Right))
            {
                if (currentState == CharState.Walking || currentState == CharState.Standing)
                {
                    currentDirection = Direction.Right;
                    currentState = CharState.Walking;
                }
                else if (currentState == CharState.Falling || currentState == CharState.Jumping)
                {
                    midAirNudge(Direction.Right);
                }
            }
            if (keyState.IsKeyUp(Keys.Left) && keyState.IsKeyUp(Keys.Right))
            {
                if(currentState == CharState.Walking)
                    currentState = CharState.Standing;
            }
            if (keyState.IsKeyDown(Keys.Z) && prevKeyState.IsKeyUp(Keys.Z) && feetOnGround)
            {
                beginJump();
            }
            if (keyState.IsKeyUp(Keys.Z)) // cut off jump
            {
                if (currentState == CharState.Jumping)
                {
                    currentState = CharState.Falling;
                }
            }
        }
        public void enforceMaxSpeed()
        {
            if (Math.Abs(velocity.X) > walkSpeed)
            {
                if (velocity.X > 0)
                    velocity.X = walkSpeed;
                else
                    velocity.X = -walkSpeed;
            }
            if (Math.Abs(velocity.Y) > maxSpeed)
            {
                if (velocity.Y > 0)
                    velocity.Y = maxSpeed;
                else
                    velocity.Y = -maxSpeed;
            }
        }
        public void beginJump()
        {
            //timer = 0;
            //endTimer = 300;
            this.currentState = CharState.Jumping;
            this.applyForce(new Vector2(0, -16));
            this.feetOnGround = false;
        }
        public void endJump()
        {
            if (this.velocity.Y < 0)
            {
                this.velocity.Y = 0;
            }
            this.currentState = CharState.Falling;
        }
        public void walk(Direction d)
        {
            if (d == Direction.Left)
            {
                this.velocity += new Vector2(-1, 0);
            }
            else
            {
                this.velocity += new Vector2(1, 0);
            }
        }
        public void midAirNudge(Direction d)
        {
            if (d == Direction.Left)
            {
                this.velocity += new Vector2(-0.8f, 0);
            }
            else
            {
                this.velocity += new Vector2(0.8f, 0);
            }
        }
        public void stopWalk()
        {
            this.velocity.X /= 2f;
            if (Math.Abs(velocity.X) < 0.2)
                velocity.X = 0;
        }
        */
    }
}
