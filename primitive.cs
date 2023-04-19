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
    public class material {
        public bool mirror;
        public bool dielectric;
        public float refraction;
        public float reflectance;
    }

    abstract class primitive
    {
        /* Should have two subclasses: sphere and plane
         * 
         */
        public Vector3 position, colour;
        public material material;


        public primitive(Vector3 p, Vector3 c)
        {
            this.material = new material();
            this.position = p;
            this.colour = c;
        }

        public abstract intersection Intersect(Ray ray);

        public virtual Vector3 Color(Vector3 position) {
        	return colour;
        }
    }

    class sphere : primitive
    {
        public float radius;


        public sphere(Vector3 p, Vector3 c, float r) : base(p, c)
        {
            this.radius = r;
        }

        public override intersection Intersect(Ray ray)
        {

            Vector3 c = this.position - ray.origin;
            float distance = Vector3.Dot(c, ray.direction);
            Vector3 q = c - distance * ray.direction;
            float p2 = Vector3.Dot(q, q);

            if (p2 > radius * radius) return null; //no intersection
            distance -= (float)Math.Sqrt(radius * radius - p2);
            //if there is a intesection check 
            if (distance > ray.distance || distance < 0)
                return null;

            intersection intersection = new intersection();
            intersection.distance = distance;
            intersection.nearest = this;
            Vector3 a = ray.origin + ray.direction * distance;
            intersection.normal = Vector3.Normalize(a - position);
            return intersection;

        }
    }

    class plane : primitive
    {
        Vector3 normal;

        public plane(Vector3 p, Vector3 c, Vector3 n) : base(p, c)
        {

            this.normal = Vector3.Normalize(n);
        }

        public override Vector3 Color(Vector3 position) {
            bool checker = Math.Abs(position.X) % 1f < 0.5f ^ Math.Abs(position.Z) % 1f < 0.5f ^ position.X < 0f ^ position.Z < 0f;
            return base.Color(position) * (checker ? 1f : 0.1f);
        }

        public override intersection Intersect(Ray ray)
        {
            float ln = Vector3.Dot(ray.direction, normal);
            float distance;
            if (ln > raytracer.epsilon || ln < -raytracer.epsilon) {
                distance = Vector3.Dot(position - ray.origin, normal) / ln;

            }
            else return null;

            if (distance > ray.distance || distance < 0)
                return null;

            intersection intersection = new intersection();
            intersection.distance = distance;
            intersection.normal = ln > 0 ? -normal : normal;
            intersection.nearest = this;

            return intersection;
        }
    }
}
