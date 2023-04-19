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
    class light
    {
        public Vector3 location, colour;
        public float range;

        public primitive[] previous = new primitive[raytracer.maxdepth];

        public light(Vector3 Location ,Vector3 Colour ,float range)
        {
            this.location = Location;
            this.colour = Colour;
            this.range = range;
        }
    }
}
