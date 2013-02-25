using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content;

namespace WickedCrush
{
    public class Animation
    {
        #region fields
        public Texture2D animationSheet;
        Point sheetSize;
        public TimeSpan frameInterval;
        TimeSpan nextFrame;
        private Vector2 frameSizeFloat; //variable so the division only has to be done once

        private bool stopped;

        public Point currentFrame;
        public Point frameSize;
        public int totalFrames;
        public bool complete = false; //used to check if an animation is finished and can be transitioned to the next animation

        public bool loop = true;
        public Animation nextAnimation; //implement this
        #endregion

        #region init
        public Animation(Texture2D animationSheet, Point frameSize, Point sheetSize, TimeSpan frameInterval, int totalFrames)
        {
            this.animationSheet = animationSheet;
            this.frameSize = frameSize;
            this.sheetSize = sheetSize;
            this.frameInterval = frameInterval;
            this.totalFrames = totalFrames;
            this.currentFrame = new Point(0, 0);
            this.stopped = false;
            nextFrame = TimeSpan.Zero;

            frameSizeFloat = new Vector2(((float)frameSize.X) / ((float)animationSheet.Width), ((float)frameSize.Y) / ((float)animationSheet.Height));
        }

        public Animation(String path, ContentManager cm)
        {
            LoadAnimation(path, cm);
        }
        #endregion

        #region update and render
        public bool Update(GameTime gameTime)
        {
            bool progressed;

            if (!stopped)
            {
                if (nextFrame >= frameInterval)
                {
                    currentFrame.X++;
                    if (currentFrame.X >= sheetSize.X)
                    {
                        currentFrame.X = 0;
                        currentFrame.Y++;
                    }
                    if (currentFrame.Y >= sheetSize.Y)
                    {
                        if(loop)
                            currentFrame.Y = 0;
                        else
                            if (currentFrame.X == 0)
                            {
                                currentFrame.X = sheetSize.X - 1;
                                currentFrame.Y--;
                            }
                            else
                            {
                                currentFrame.X--;
                            }
                            //changeToNextAnimation();

                        complete = true;
                    }

                    if ((currentFrame.Y * sheetSize.X) + currentFrame.X >= totalFrames)
                    {
                        if (loop)
                        {
                            currentFrame.X = 0;
                            currentFrame.Y = 0;
                        }
                        else
                        {
                            if (currentFrame.X == 0)
                            {
                                currentFrame.X = sheetSize.X - 1;
                                currentFrame.Y--;
                            }
                            else
                            {
                                currentFrame.X--;
                            }
                        }
                        complete = true;
                    }

                    progressed = true;
                    nextFrame = TimeSpan.Zero;
                }
                else
                {
                    nextFrame += gameTime.ElapsedGameTime;
                    progressed = false;
                }
            }
            else
            {
                progressed = false;
            }
            return progressed;
        }

        /*public void SetAnimation(Animation nAnimation)
        {
            if (!this.Equals(nAnimation))
            {
                this.ResetAnimation();
            }
        }*/

        public void changeToNextAnimation()
        {
            if (nextAnimation != null)
            {


                this.animationSheet = nextAnimation.animationSheet;
                this.frameSize = nextAnimation.frameSize;
                this.sheetSize = nextAnimation.sheetSize;
                this.totalFrames = nextAnimation.totalFrames;
                this.frameInterval = nextAnimation.frameInterval;
                this.currentFrame = new Point(0, 0);

                nextFrame = TimeSpan.Zero;

                frameSizeFloat = new Vector2(
                    ((float)frameSize.X) / ((float)animationSheet.Width),
                    ((float)frameSize.Y) / ((float)animationSheet.Height));
            }
        }

        public void ResetAnimation()
        {
            complete = false;
            currentFrame.X = 0;
            currentFrame.Y = 0;
            nextFrame = TimeSpan.Zero;

            if (nextAnimation != null)
                nextAnimation.ResetAnimation();
        }

        public void StopAnimation()
        {
            stopped = true;
        }

        public void StartAnimation()
        {
            stopped = false;
        }

        public Vector2 getTopLeftCoordinate() //expensive division? possible micro-optimization
        {
            return new Vector2((float)currentFrame.X / sheetSize.X, (float)currentFrame.Y / sheetSize.Y);
        }
        public Vector2 getTopRightCoordinate()
        {
            return new Vector2((float)(currentFrame.X + 1) / sheetSize.X, (float)currentFrame.Y / sheetSize.Y);
        }
        public Vector2 getBottomLeftCoordinate()
        {
            return new Vector2((float)currentFrame.X / sheetSize.X, (float)(currentFrame.Y + 1) / sheetSize.Y);
        }
        public Vector2 getBottomRightCoordinate()
        {
            return new Vector2((float)(currentFrame.X + 1) / sheetSize.X, (float)(currentFrame.Y + 1) / sheetSize.Y);
        }

        public int getCurrentFrameNumber()
        {
            return currentFrame.X + currentFrame.Y * sheetSize.X;
        }

        /*public float UpdateLeftFloat()
        {
            return (float)currentFrame.X * frameSize.X;
        }
        public float UpdateRightFloat()
        {
            return (float)(currentFrame.X + 1) * frameSize.X;
        }
        public float UpdateTopFloat()
        {
            return (float)currentFrame.Y * frameSize.Y;
        }
        public float UpdateBottomFloat()
        {
            return (float)(currentFrame.Y + 1) * frameSize.Y;
        }*/
        #endregion

        public void LoadAnimation(string path, ContentManager cm)
        {
            XDocument doc = XDocument.Load(@"Content/animations/" + path + ".xml");
            XElement rootElement = doc.Element("animation");
            this.animationSheet = cm.Load<Texture2D>(@"animations/" + rootElement.Attribute("name").Value);
            XElement attributes = rootElement.Element("attributes");
            this.frameSize = new Point(int.Parse(attributes.Attribute("frameSize.X").Value), int.Parse(attributes.Attribute("frameSize.Y").Value));
            this.sheetSize = new Point(int.Parse(attributes.Attribute("sheetSize.X").Value), int.Parse(attributes.Attribute("sheetSize.Y").Value));
            this.totalFrames = int.Parse(attributes.Attribute("totalFrames").Value);
            this.frameInterval = new TimeSpan(0,0,0,0,int.Parse(attributes.Attribute("frameInterval").Value));
            this.currentFrame = new Point(0, 0);
            nextFrame = TimeSpan.Zero;

            frameSizeFloat = new Vector2(((float)frameSize.X) / ((float)animationSheet.Width), ((float)frameSize.Y) / ((float)animationSheet.Height));
        }

    }
}
