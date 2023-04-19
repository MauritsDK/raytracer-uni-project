using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Drawing.Design;

namespace Template
{
    class scene
    {
        public List<primitive> primitives = new List<primitive>();
        public List<light> lights = new List<light>();
        public environment environment;


        public scene() {
            environment = new environment("../../assets/rnl_probe.pfm");

            material diffuse = new material();

            material partial = new material();
            partial.reflectance = 0.2f;

            material mirror = new material();
            mirror.mirror = true;

            material refraction = new material();
            refraction.refraction = 1.52f;

            lights.Add(new light(new Vector3(-4 ,0 ,1) ,new Vector3(10 ,10 ,10) ,20f));
            lights.Add(new light(new Vector3(0 ,3 ,3) ,new Vector3(10 ,10 ,10) ,20f));

            primitives.Add(new sphere(new Vector3(-3 ,0 ,3) ,new Vector3(1 ,1 ,1) ,1));
            primitives.Add(new sphere(new Vector3(0 ,0 ,3) ,new Vector3(1 ,1 ,1) ,1));
            primitives.Add(new sphere(new Vector3(3 ,0 ,3) ,new Vector3(0 ,1 ,0) ,1));

            primitives.Add(new plane(new Vector3(0, -1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 0)));

            primitives[0].material = refraction;
            primitives[1].material = mirror;
            primitives[2].material = diffuse;
            primitives[3].material = partial;
        }

        //loops over primitives and returns closest intersection
        public intersection Intersect(Ray ray)
        {
            intersection r = null;

            foreach (primitive p in primitives)
            {
                intersection i = p.Intersect(ray);
                if (i != null && (r == null || i.distance < r.distance))
                    r = i;
            }

            return r;
        }

        //loops over primitives and returns first intersection
        public intersection IntersectFast(Ray ray,primitive first) {
            intersection r = first == null ? null : first.Intersect(ray);
            if (r == null) {
                foreach (primitive p in primitives) {
                    if (p != first && (r = p.Intersect(ray)) != null)
                        break;
                }
            }
        	return r;
        }
    }

    class intersection
    {
        public Vector3 normal;
        public primitive nearest;
        public float distance;
    }

    class environment {
        private Vector3[,] texture;
        private int width, height;

        public environment(string file) {
            FileStream fs = File.OpenRead(file);
            // read header
            StringBuilder line = new StringBuilder();
            int b;
            for (int i = 0; i< 3; i++) {
                while ((b = fs.ReadByte()) != '\n') {
                    line.Append((char)b);
                }
                if (i == 1) {
                    string[] split = line.ToString().Split();
                    width = int.Parse(split[0]);
                    height = int.Parse(split[1]);
                }
                line.Clear();
            }
            // read image data
            texture = new Vector3[width ,height];
            for (int y = 0; y<height; y++) {
                for (int x = 0; x<width; x++) {
                    texture[x ,y] = new Vector3(ReadFloat(fs),ReadFloat(fs) ,ReadFloat(fs));
                }
            }
            fs.Close();
        }

        private byte[] buffer = new byte[4];
        private float ReadFloat(Stream stream) {
        	stream.Read(buffer ,0 ,4);
        	return System.BitConverter.ToSingle(buffer ,0);
        }

        private static float piInv = (float)(1 / Math.PI);
        public Vector3 Color(Vector3 direction) {
            Vector3 c = Vector3.Zero;

            // calculate coordinates
            float r = piInv * (float)(Math.Acos((double)direction.Z) / Math.Sqrt((double)(direction.X * direction.X + direction.Y * direction.Y)));
            float u = ((direction.X* r + 1f) / 2f) * (width - 2);
            float v = ((direction.Y* r + 1f) / 2f) * (height - 2);

            // interpolate
            c += texture[(int)u ,(int)v] * ((int)u - u + 1) * ((int)v - v + 1);
            c += texture[(int)u + 1,(int)v] * (u - (int)u) * ((int)v - v + 1);
            c += texture[(int)u,(int)v + 1] * ((int)u - u + 1) * (v - (int)v);
            c += texture[(int)u + 1,(int)v + 1] * (u - (int)u) * (v - (int)v);
            return c;
        }
    }
}
