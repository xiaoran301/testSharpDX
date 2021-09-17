using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Foundation
{
    class Transform
    {
        float m_FrameTime;
        float m_ForwardSpeed, m_BackwardSpeed;
        float m_UpwardSpeed, m_DownwardSpeed;
        float m_LeftTurnSpeed, m_RightTurnSpeed;
        float m_LookUpSpeed, m_LookDownSpeed;

        // Properties
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }

        // Public Methods
        public void SetPosition(float x, float y, float z)
        {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
        }
        public void SetRotation(float x, float y, float z)
        {
            RotationX = x;
            RotationY = y;
            RotationZ = z;
        }
        public void SetScale(float x, float y, float z)
        {
            ScaleX = x;
            ScaleY = y;
            ScaleZ = z;
        }
        public void SetFrameTime(float time)
        {
            m_FrameTime = time;
        }
        public void MoveForward(bool keydown)
        {
            // Update the forward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_ForwardSpeed += m_FrameTime * 0.001f;
                if (m_ForwardSpeed > m_FrameTime * 0.03)
                    m_ForwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_ForwardSpeed -= m_FrameTime * 0.0007f;
                if (m_ForwardSpeed < 0)
                    m_ForwardSpeed = 0;
            }

            // Convert degrees to radians.
            float radians = RotationY * 0.0174532925f;

            // Update the position.
            PositionX += (float)Math.Sin(radians) * m_ForwardSpeed;
            PositionZ += (float)Math.Cos(radians) * m_ForwardSpeed;
        }
        public void MoveBackward(bool keydown)
        {
            // Update the backward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_BackwardSpeed += m_FrameTime * 0.001f;
                if (m_BackwardSpeed > m_FrameTime * 0.03f)
                    m_BackwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_BackwardSpeed -= m_FrameTime * 0.0007f;
                if (m_BackwardSpeed < 0)
                    m_BackwardSpeed = 0;
            }

            // Convert degrees to radians.
            float radians = RotationY * 0.0174532925f;

            // Update the position.
            PositionX -= (float)Math.Sin(radians) * m_BackwardSpeed;
            PositionZ -= (float)Math.Cos(radians) * m_BackwardSpeed;
        }
        public void MoveUpward(bool keydown)
        {
            // Update the downward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_UpwardSpeed += m_FrameTime * 0.003f;
                if (m_UpwardSpeed > (m_FrameTime * 0.03f))
                    m_UpwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_UpwardSpeed -= m_FrameTime * 0.002f;

                if (m_UpwardSpeed < 0.0f)
                    m_UpwardSpeed = 0.0f;
            }

            // Update the height position.
            PositionY += m_UpwardSpeed;
        }
        public void MoveDownward(bool keydown)
        {
            // Update the downward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_DownwardSpeed += m_FrameTime * 0.003f;
                if (m_DownwardSpeed > (m_FrameTime * 0.03f))
                    m_DownwardSpeed = m_FrameTime * 0.03f;
            }
            else
            {
                m_DownwardSpeed -= m_FrameTime * 0.002f;
                if (m_DownwardSpeed < 0.0f)
                    m_DownwardSpeed = 0.0f;
            }

            // Update the height position.
            PositionY -= m_DownwardSpeed;
        }
        public void TurnLeft(bool keydown)
        {
            // Update the left turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LeftTurnSpeed += m_FrameTime * 0.01f;
                if (m_LeftTurnSpeed > m_FrameTime * 0.15)
                    m_LeftTurnSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LeftTurnSpeed -= m_FrameTime * 0.005f;
                if (m_LeftTurnSpeed < 0)
                    m_LeftTurnSpeed = 0;
            }

            // Update the rotation using the turning speed.
            RotationY -= m_LeftTurnSpeed;

            // Keep the rotation in the 0 to 360 range.
            if (RotationY < 0)
                RotationY += 360;
        }
        public void TurnRight(bool keydown)
        {
            // Update the right turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_RightTurnSpeed += m_FrameTime * 0.01f;
                if (m_RightTurnSpeed > m_FrameTime * 0.15)
                    m_RightTurnSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_RightTurnSpeed -= m_FrameTime * 0.005f;
                if (m_RightTurnSpeed < 0)
                    m_RightTurnSpeed = 0;
            }

            // Update the rotation using the turning speed.
            RotationY += m_RightTurnSpeed;

            // Keep the rotation in the 0 to 360 range which is looking stright Up.
            if (RotationY > 360)
                RotationY -= 360;
        }
        public void LookUpward(bool keydown)
        {
            // Update the upward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LookUpSpeed += m_FrameTime * 0.01f;
                if (m_LookUpSpeed > m_FrameTime * 0.15)
                    m_LookUpSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LookUpSpeed -= m_FrameTime * 0.005f;
                if (m_LookUpSpeed < 0)
                    m_LookUpSpeed = 0;
            }

            // Update the rotation using the turning speed.
            RotationX -= m_LookUpSpeed;

            // Keep the rotation maximum 90 degrees.
            if (RotationX > 90)
                RotationX = 90;
        }
        public void LookDownward(bool keydown)
        {
            // Update the downward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                m_LookDownSpeed += m_FrameTime * 0.01f;
                if (m_LookDownSpeed > m_FrameTime * 0.15)
                    m_LookDownSpeed = m_FrameTime * 0.15f;
            }
            else
            {
                m_LookDownSpeed -= m_FrameTime * 0.005f;
                if (m_LookDownSpeed < 0)
                    m_LookDownSpeed = 0;
            }

            // Update the rotation using the turning speed.
            RotationX += m_LookDownSpeed;

            // Keep the rotation maximum 90 degrees which is looking straight down.
            if (RotationX < -90)
                RotationX = -90;
        }

    }
}
