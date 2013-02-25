using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush
{
    public enum CameraMode
    {
        Still = 0,
        FollowTargetX = 1,
        FollowTargetY = 2,
        FollowTargetXY = 3
    }
    public class Camera
    {
        #region fields
        public Vector3 cameraPosition, cameraTarget, upVector, velocity;
        public Vector2 camOffset, minCamPos, camNudge;
        public float fov, minX, minY;
        public Entity target;
        public CameraMode camMode = CameraMode.Still;
        public float smoothVal = 7f;

        public ControlsManager _controls;
        #endregion

        public Camera()
        {
            cameraPosition = new Vector3(0f, 0f, 0f);
            cameraTarget = new Vector3(0f, 0f, 0f);
            velocity = new Vector3(0f, 0f, 0f);
            upVector = Vector3.Up;
            fov = MathHelper.PiOver4;
            minCamPos = new Vector2(356f, 448f);
            //minX = 0;
            //minY = 0;
            camOffset = new Vector2(0f, 0f);
            camNudge = new Vector2(0f, 0f);
        }

        public void SetTarget(Entity e)
        {
            target = e;
            camMode = CameraMode.FollowTargetXY;
            camOffset.X = (target.size.X / 2f + target.offset.X);
            camOffset.Y = (target.size.Y / 2f + target.offset.Y);
            cameraPosition.X = target.pos.X + camOffset.X;
            cameraPosition.Y = target.pos.Y + camOffset.Y;
            cameraTarget.X = target.pos.X + camOffset.X;
            cameraTarget.Y = target.pos.Y + camOffset.Y;
        }

        public void setCameraDepth(float nDepth)
        {
            cameraPosition.Z = nDepth;
        }

        public void Update() // to be updated with use of state machine...maybe
        {
            if (target == null)
            {
                camMode = CameraMode.Still;
            }

            camNudge.X = _controls.RStickXAxis() * 200f; //lol keyboard users get innate advantage
            camNudge.Y = _controls.RStickYAxis() * 200f; //lol keyboard users get innate advantage

                switch (camMode)
                {
                    case CameraMode.Still:
                        break;
                    case CameraMode.FollowTargetX:
                        cameraPosition.X = (cameraPosition.X * smoothVal + target.pos.X + camOffset.X) / (smoothVal+1f);
                        cameraTarget.X = (cameraPosition.X * smoothVal + target.pos.X + camOffset.X) / (smoothVal + 1f);
                        break;
                    case CameraMode.FollowTargetY:
                        cameraPosition.Y = (cameraPosition.Y * smoothVal + target.pos.Y + camOffset.Y) / (smoothVal + 1f);
                        cameraTarget.Y = (cameraPosition.Y * smoothVal + target.pos.Y + camOffset.Y) / (smoothVal + 1f);
                        break;
                    case CameraMode.FollowTargetXY:
                        cameraPosition.X = (cameraPosition.X * smoothVal + target.pos.X + camOffset.X + camNudge.X) / (smoothVal + 1f);
                        cameraTarget.X = (cameraPosition.X * smoothVal + target.pos.X + camOffset.X + camNudge.X) / (smoothVal + 1f);
                        cameraPosition.Y = (cameraPosition.Y * smoothVal + target.pos.Y + camOffset.Y + camNudge.Y) / (smoothVal + 1f);
                        cameraTarget.Y = (cameraPosition.Y * smoothVal + target.pos.Y + camOffset.Y + camNudge.Y) / (smoothVal + 1f);
                        break;
                }

            cameraPosition += velocity;
            cameraTarget += velocity;
            //adhereToBounds();
        }
        private void adhereToBounds()
        {
            if (cameraPosition.X < minCamPos.X)
            {
                cameraPosition.X = minCamPos.X;
                cameraTarget.X = minCamPos.X;
            }
            if (cameraPosition.Y < minCamPos.Y)
            {
                cameraPosition.Y = minCamPos.Y;
                cameraTarget.Y = minCamPos.Y;
            }
        }
        public void MoveCamLeft(float speed)
        {
            cameraPosition.X = cameraPosition.X - speed;
            cameraTarget.X = cameraTarget.X - speed;
            adhereToBounds();
        }
        public void MoveCamRight(float speed)
        {
            cameraPosition.X = cameraPosition.X + speed;
            cameraTarget.X = cameraTarget.X + speed;
        }
        public void MoveCamUp(float speed)
        {
            cameraPosition.Y = cameraPosition.Y + speed;
            cameraTarget.Y = cameraTarget.Y + speed;
        }
        public void MoveCamDown(float speed)
        {
            cameraPosition.Y = cameraPosition.Y - speed;
            cameraTarget.Y = cameraTarget.Y - speed;
            adhereToBounds();
        }
    }
}
