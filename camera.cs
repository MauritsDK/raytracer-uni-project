using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Template
{
    class camera
    {
        /*Needs to contain:
         * data members position and direction
         * stores screen plane, specified by four corners, 
         * updated when position and/or direction are changed
         * Hardcode at first.
         */

        //position is camera origin, direction is where it is looking
        //hardcoded stuff - take out later
        public Vector3 position = new Vector3(0,0,-5);

        public float viewAngle = 60;
        public float angleX = 0;
        public float angleY = 0;


        public Vector3 topleft;
        public Vector3 topright;
        public Vector3 bottomright;
        public Vector3 bottomleft;

        public void Move(Vector3 offset) {
            position += Vector3.Transform(offset, Matrix4.CreateFromAxisAngle(Vector3.UnitY, angleY));
        }

        public void Zoom(float offset) {
            viewAngle += offset;
            viewAngle = Math.Max(viewAngle ,10f);
            viewAngle = Math.Min(viewAngle ,170f);
        }

        public void RotateX(float offset) {
            angleX += offset;
        }

        public void RotateY(float offset) {
            angleY += offset;
        }

        public void Update() {
            Matrix4 rot = Matrix4.CreateFromAxisAngle(Vector3.UnitX ,angleX) * Matrix4.CreateFromAxisAngle(Vector3.UnitY, angleY);
            Vector3 screen = position + Vector3.Transform(Vector3.UnitZ ,rot) * Viewdistance(viewAngle);
            topleft = screen + Vector3.Transform(new Vector3(-1 ,1 ,0), rot);
            topright = screen + Vector3.Transform(new Vector3(1 ,1 ,0), rot);
            bottomright = screen + Vector3.Transform(new Vector3(1 ,-1 ,0), rot);
            bottomleft = screen + Vector3.Transform(new Vector3(-1,-1,0), rot);
        }

        float Viewdistance(float x)
        {
            float r = x * (float)Math.PI / 180;
            float a = 1 / (float)Math.Tan(r / 2);
            return a;
        }
    }
}
