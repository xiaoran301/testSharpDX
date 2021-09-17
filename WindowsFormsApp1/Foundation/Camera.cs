using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace WindowsFormsApp1.Foundation
{
    public class Camera                    // 112 lines
    {
        // Properties.
        private float PositionX { get; set; }
        private float PositionY { get; set; }
        private float PositionZ { get; set; }
        private float RotationX { get; set; }
        private float RotationY { get; set; }
        private float RotationZ { get; set; }
        public Matrix ViewMatrix { get; private set; }
        public Matrix BaseViewMatrix { get; private set; }
        public Matrix ReflectionViewMatrix { get; private set; }

        // Constructor
        public Camera() { }

        // Methods.
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
        public Vector3 GetPosition()
        {
            return new Vector3(PositionX, PositionY, PositionZ);
        }
        public void Render()
        {
            // Setup the position of the camera in the world.
            Vector3 position = new Vector3(PositionX, PositionY, PositionZ);

            // Setup where the camera is looking  forwardby default.
            Vector3 lookAt = new Vector3(0, 0, 1.0f);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.
            float pitch = RotationX * 0.0174532925f;
            float yaw = RotationY * 0.0174532925f;
            float roll = RotationZ * 0.0174532925f;

            //// Create the rotation matrix from the yaw, pitch, and roll values.
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            lookAt = position + lookAt;

            // Finally create the view matrix from the three updated vectors.
            ViewMatrix = Matrix.LookAtLH(position, lookAt, up);
        }
        internal void RenderBaseViewMatrix()
        {
            // Setup the vector that points upwards.
            Vector3 up = Vector3.Up;

            // Calculate the rotation in radians.
            float radians = RotationY * 0.0174532925f;

            // Setup where the camera is looking.
            Vector3 lookAt = new Vector3();
            lookAt.X = (float)Math.Sin(radians) + PositionX;
            lookAt.Y = PositionY;
            lookAt.Z = (float)Math.Cos(radians) + PositionZ;

            // Create the base view matrix from the three vectors.
            BaseViewMatrix = Matrix.LookAtLH(GetPosition(), lookAt, up);
        }
        public void RenderReflection(float height)
        {
            // Setup the vector that points upwards.
            Vector3 up = Vector3.Up;

            // Setup the position of the camera in the world.  For planar reflection invert the Y position of the camera.
            Vector3 position = new Vector3(PositionX, -PositionY + (height * 2.0f), PositionZ);

            // Setup where the camera is looking by default.
            Vector3 lookAt = new Vector3(0, 0, 1.0f);

            // Set the yaw (Y axis), pitch (X axis), and roll (Z axis) rotations in radians.  Invert the X rotation for reflection.
            float pitch = -RotationX * 0.0174532925f;
            float yaw = RotationY * 0.0174532925f;
            float roll = RotationZ * 0.0174532925f;

            // Create the rotation matrix from the yaw, pitch, and roll values.
            Matrix rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            // Transform the lookAt and up vector by the rotation matrix so the view is correctly rotated at the origin.
            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            up = Vector3.TransformCoordinate(up, rotationMatrix);

            // Translate the rotated camera position to the location of the viewer.
            lookAt = position + lookAt;

            // Finally create the reflection view matrix from the three updated vectors.
            ReflectionViewMatrix = Matrix.LookAtLH(position, lookAt, up);
        }
    }

}
