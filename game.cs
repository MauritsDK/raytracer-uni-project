using System;
using System.IO;
using System.Diagnostics;
using OpenTK;
using System.Threading;

namespace Template
{

    class Game
    {
        // member variables
        raytracer raytracer;
        camera camera;
        private bool spacePress = true;

        public FloatSurface screen;
        public Surface debug;
        // initialize
        public void Init()
        {
            camera = new camera();
            raytracer = new raytracer(screen, debug ,camera);
            Thread t = new Thread(raytracer.Run);
            t.Start();
        }
        // tick: renders one frame
        public void Tick()
        {
            var keyboard = OpenTK.Input.Keyboard.GetState();
            Vector3 offset = Vector3.Zero;
            float step = 0.02f;

            // handle movement
            if (keyboard[OpenTK.Input.Key.Left])
                offset.X -= step;
            if (keyboard[OpenTK.Input.Key.Right])
                offset.X += step;
            if (keyboard[OpenTK.Input.Key.Up])
                offset.Z += step;
            if (keyboard[OpenTK.Input.Key.Down])
                offset.Z -= step;
            if (keyboard[OpenTK.Input.Key.ShiftLeft])
                offset.Y += step;
            if (keyboard[OpenTK.Input.Key.ControlLeft])
                offset.Y -= step;

            if (keyboard[OpenTK.Input.Key.A]) {
                camera.RotateY(-step);
                raytracer.restart = true;
            }
            if (keyboard[OpenTK.Input.Key.D]) {
                camera.RotateY(step);
                raytracer.restart = true;
            }
            if (keyboard[OpenTK.Input.Key.W]) {
                camera.RotateX(-step);
                raytracer.restart = true;
            }
            if (keyboard[OpenTK.Input.Key.S]) {
                camera.RotateX(step);
                raytracer.restart = true;
            }

            if (offset != Vector3.Zero) {
                camera.Move(offset);
                raytracer.restart = true;
            }

            // handle field of view
            if (keyboard[OpenTK.Input.Key.Z]) {
                camera.Zoom(-step*25);
                raytracer.restart = true;
            }
            if (keyboard[OpenTK.Input.Key.X]) {
                camera.Zoom(step*25);
                raytracer.restart = true;
            }

            // handle anti aliasing switching
            if (!spacePress && keyboard.IsKeyDown(OpenTK.Input.Key.Space)) {
                spacePress = true;
                raytracer.aa = (raytracer.aa * 2) % 15;
                raytracer.restart = true;
            }
            else if (spacePress && keyboard.IsKeyUp(OpenTK.Input.Key.Space)) {
                spacePress = false;
            }

            if (raytracer.restart) {
                lock (raytracer) {
                    Monitor.PulseAll(raytracer);
                }
            }
        }
    }

} // namespace Template