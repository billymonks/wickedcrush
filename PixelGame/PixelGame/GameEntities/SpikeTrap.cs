using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.GameStates;

namespace WickedCrush.GameEntities
{
    public class SpikeTrap : Character
    {

        public SpikeTrap(ContentManager cm, GraphicsDevice gd, Vector2 pos)
            : base(pos, new Vector2(32f, 32f), new Vector2(0.5f, 0.5f), new Vector2(-8f, 0f), 128f, gd)
        {
            CreateCharacter(cm);
            walkThrough = false;
        }
        public void CreateCharacter(ContentManager cm)
        {
            name = "Spike_Trap";
            CreateAnimationList(cm);
            CreateStateMachine();

            invuln = true;
            immobile = true;
        }
        public void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("spiketrap", new Animation("spiketrap-placeholder", cm));
            animationList.Add("spiketrap-norm", new Animation("spiketrap-normal-placeholder", cm));

            defaultTexture = animationList["spiketrap"].animationSheet;
            defaultNormal = animationList["spiketrap-norm"].animationSheet;
        }
        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();

            stateList.Add("trap-load", new State("trap-load",
                c => sm.previousControlState == null,
                c =>
                {
                    currentAnimation = animationList["spiketrap"];
                    currentAnimationNorm = animationList["spiketrap-norm"];
                    //currentAnimationNorm = animationList["placeholder-door-normal"];
                }));

            stateList.Add("trap-chilling", new State("trap-chilling",
                c => sm.previousControlState != null,
                c =>
                {
                    //currentAnimationNorm = animationList["placeholder-door-normal"];
                    KillAllWhoTouchMe();
                }));

            sm = new StateMachine(stateList);
        }

        protected override void SetupVerts()
        {
            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            topLeft = new Vector4(pos.X + offset.X, pos.Y + size.Y + offset.Y, 64f, 1f);
            bottomLeft = new Vector4(pos.X + offset.X, pos.Y + offset.Y, 64f, 1f);
            bottomRight = new Vector4(pos.X + size.X + offset.X, pos.Y + offset.Y, 64f, 1f);
            topRight = new Vector4(pos.X + size.X + offset.X, pos.Y + size.Y + offset.Y, 64f, 1f);

            if (currentAnimation != null)
            {
                texTopLeft = currentAnimation.getTopLeftCoordinate();
                texBottomLeft = currentAnimation.getBottomLeftCoordinate();
                texBottomRight = currentAnimation.getBottomRightCoordinate();
                texTopRight = currentAnimation.getTopRightCoordinate();
            }
            else
            {
                texTopLeft = new Vector2(0f, 0f);
                texBottomLeft = new Vector2(0f, 1f);
                texBottomRight = new Vector2(1f, 1f);
                texTopRight = new Vector2(1f, 0f);
            }

            vertices = new VertexPositionNormalTextureTangentBinormal[18];
            //top left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                    topLeft,
                    normal,
                    texTopLeft,
                    texTopLeft, 
                    tangent, 
                    binormal);
            //bottom left
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                bottomLeft,
                normal,
                texBottomLeft,
                texBottomLeft, 
                tangent,
                binormal);
            //bottom right
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                bottomRight,
                normal,
                texBottomRight,
                texBottomRight, 
                tangent,
                binormal);
            //top right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                topRight,
                normal,
                texTopRight,
                texTopRight, 
                tangent,
                binormal);

            vertices[4] = vertices[0];
            vertices[5] = vertices[2];

            topLeft = new Vector4(pos.X + offset.X, pos.Y + size.Y + offset.Y, 128f, 1f);
            bottomLeft = new Vector4(pos.X + offset.X, pos.Y + offset.Y, 128f, 1f);
            bottomRight = new Vector4(pos.X + size.X + offset.X, pos.Y + offset.Y, 128f, 1f);
            topRight = new Vector4(pos.X + size.X + offset.X, pos.Y + size.Y + offset.Y, 128f, 1f);

            vertices[6] = new VertexPositionNormalTextureTangentBinormal(
                    topLeft,
                    normal,
                    texTopLeft,
                    texTopLeft, 
                    tangent, 
                    binormal);
            //bottom left
            vertices[7] = new VertexPositionNormalTextureTangentBinormal(
                bottomLeft,
                normal,
                texBottomLeft,
                texBottomLeft, 
                tangent,
                binormal);
            //bottom right
            vertices[8] = new VertexPositionNormalTextureTangentBinormal(
                bottomRight,
                normal,
                texBottomRight,
                texBottomRight, 
                tangent,
                binormal);
            //top right
            vertices[9] = new VertexPositionNormalTextureTangentBinormal(
                topRight,
                normal,
                texTopRight,
                texTopRight, 
                tangent,
                binormal);

            vertices[10] = vertices[6];
            vertices[11] = vertices[8];

            topLeft = new Vector4(pos.X + offset.X, pos.Y + size.Y + offset.Y, 192f, 1f);
            bottomLeft = new Vector4(pos.X + offset.X, pos.Y + offset.Y, 192f, 1f);
            bottomRight = new Vector4(pos.X + size.X + offset.X, pos.Y + offset.Y, 192f, 1f);
            topRight = new Vector4(pos.X + size.X + offset.X, pos.Y + size.Y + offset.Y, 192f, 1f);

            vertices[12] = new VertexPositionNormalTextureTangentBinormal(
                    topLeft,
                    normal,
                    texTopLeft,
                    texTopLeft, 
                    tangent, 
                    binormal);
            //bottom left
            vertices[13] = new VertexPositionNormalTextureTangentBinormal(
                bottomLeft,
                normal,
                texBottomLeft,
                texBottomLeft, 
                tangent,
                binormal);
            //bottom right
            vertices[14] = new VertexPositionNormalTextureTangentBinormal(
                bottomRight,
                normal,
                texBottomRight,
                texBottomRight, 
                tangent,
                binormal);
            //top right
            vertices[15] = new VertexPositionNormalTextureTangentBinormal(
                topRight,
                normal,
                texTopRight,
                texTopRight, 
                tangent,
                binormal);

            vertices[16] = vertices[12];
            vertices[17] = vertices[14];
        }

        protected override void UpdateTextureCoords()
        {
            if (facingDir == Direction.Right)
            {
                texTopLeft = currentAnimation.getTopLeftCoordinate();
                texBottomLeft = currentAnimation.getBottomLeftCoordinate();
                texBottomRight = currentAnimation.getBottomRightCoordinate();
                texTopRight = currentAnimation.getTopRightCoordinate();
            }
            else
            {
                texTopLeft = currentAnimation.getTopRightCoordinate();
                texBottomLeft = currentAnimation.getBottomRightCoordinate();
                texBottomRight = currentAnimation.getBottomLeftCoordinate();
                texTopRight = currentAnimation.getTopLeftCoordinate();
            }

            vertices[0].TextureCoordinate = texTopLeft;
            vertices[1].TextureCoordinate = texBottomLeft;
            vertices[2].TextureCoordinate = texBottomRight;
            vertices[3].TextureCoordinate = texTopRight;

            vertices[4].TextureCoordinate = vertices[0].TextureCoordinate;
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            vertices[6].TextureCoordinate = texTopLeft;
            vertices[7].TextureCoordinate = texBottomLeft;
            vertices[8].TextureCoordinate = texBottomRight;
            vertices[9].TextureCoordinate = texTopRight;

            vertices[10].TextureCoordinate = vertices[6].TextureCoordinate;
            vertices[11].TextureCoordinate = vertices[8].TextureCoordinate;

            vertices[12].TextureCoordinate = texTopLeft;
            vertices[13].TextureCoordinate = texBottomLeft;
            vertices[14].TextureCoordinate = texBottomRight;
            vertices[15].TextureCoordinate = texTopRight;

            vertices[16].TextureCoordinate = vertices[12].TextureCoordinate;
            vertices[17].TextureCoordinate = vertices[14].TextureCoordinate;

            if (currentAnimationNorm != null)
            {
                vertices[0].NormalCoordinate = currentAnimationNorm.getTopLeftCoordinate();
                vertices[1].NormalCoordinate = currentAnimationNorm.getBottomLeftCoordinate();
                vertices[2].NormalCoordinate = currentAnimationNorm.getBottomRightCoordinate();
                vertices[3].NormalCoordinate = currentAnimationNorm.getTopRightCoordinate();

                vertices[4].NormalCoordinate = vertices[0].NormalCoordinate;
                vertices[5].NormalCoordinate = vertices[2].NormalCoordinate;

                vertices[6].NormalCoordinate = currentAnimationNorm.getTopLeftCoordinate();
                vertices[7].NormalCoordinate = currentAnimationNorm.getBottomLeftCoordinate();
                vertices[8].NormalCoordinate = currentAnimationNorm.getBottomRightCoordinate();
                vertices[9].NormalCoordinate = currentAnimationNorm.getTopRightCoordinate();

                vertices[10].NormalCoordinate = vertices[6].NormalCoordinate;
                vertices[11].NormalCoordinate = vertices[8].NormalCoordinate;

                vertices[12].NormalCoordinate = currentAnimationNorm.getTopLeftCoordinate();
                vertices[13].NormalCoordinate = currentAnimationNorm.getBottomLeftCoordinate();
                vertices[14].NormalCoordinate = currentAnimationNorm.getBottomRightCoordinate();
                vertices[15].NormalCoordinate = currentAnimationNorm.getTopRightCoordinate();

                vertices[16].NormalCoordinate = vertices[12].NormalCoordinate;
                vertices[17].NormalCoordinate = vertices[14].NormalCoordinate;
            }
        }

        protected override void UpdateVertexPositions()
        {
            topLeft.X = pos.X + offset.X;
            topLeft.Y = pos.Y + size.Y + offset.Y;
            bottomLeft.X = pos.X + offset.X;
            bottomLeft.Y = pos.Y + offset.Y;
            bottomRight.X = pos.X + size.X + offset.X;
            bottomRight.Y = pos.Y + offset.Y;
            topRight.X = pos.X + size.X + offset.X;
            topRight.Y = pos.Y + size.Y + offset.Y;

            topLeft.Z = 64f;
            bottomLeft.Z = 64f;
            bottomRight.Z = 64f;
            topRight.Z = 64f;

            vertices[0].Position = topLeft;
            vertices[1].Position = bottomLeft;
            vertices[2].Position = bottomRight;
            vertices[3].Position = topRight;

            vertices[4].Position = vertices[0].Position;
            vertices[5].Position = vertices[2].Position;

            topLeft.Z = 128f;
            bottomLeft.Z = 128f;
            bottomRight.Z = 128f;
            topRight.Z = 128f;

            vertices[6].Position = topLeft;
            vertices[7].Position = bottomLeft;
            vertices[8].Position = bottomRight;
            vertices[9].Position = topRight;

            vertices[10].Position = vertices[6].Position;
            vertices[11].Position = vertices[8].Position;

            topLeft.Z = 192f;
            bottomLeft.Z = 192f;
            bottomRight.Z = 192f;
            topRight.Z = 192f;

            vertices[12].Position = topLeft;
            vertices[13].Position = bottomLeft;
            vertices[14].Position = bottomRight;
            vertices[15].Position = topRight;

            vertices[16].Position = vertices[12].Position;
            vertices[17].Position = vertices[14].Position;
        }

        private void KillAllWhoTouchMe()
        {
            for (int i = 0; i < collisionList.Count; i++)
            {
                if (!collisionList[i].invuln)
                {
                    collisionList[i].hp = 0;
                }
            }
        }
    }
}
