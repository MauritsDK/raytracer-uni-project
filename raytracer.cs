using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics.ES30;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Template
{
    class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;
        //public float 
    }
    class raytracer
    {
        scene scene = new scene();
        FloatSurface screen;
        Surface debug;
        camera camera;

        public const float epsilon = 1f / 1000;
        public const int maxdepth = 100;
        private Vector3 min = Vector3.Zero;
        private Vector3 max = new Vector3(1 ,1 ,1);
        public int aa = 1;

        Stopwatch stopwatch = new Stopwatch();
        public bool restart;

        public raytracer(FloatSurface screen ,Surface debug,camera camera) {
            this.screen = screen;
            this.debug = debug;
            this.camera = camera;
        }

        public void Run() {
            restart = true;
            while (true) {
                if (!restart) {
                    lock (this) {
                        Monitor.Wait(this);
                    }
                }
                while (restart) {
                    restart = false;
                    debug.Clear(0);
                    camera.Update();
                    RenderDebug();
                    stopwatch.Restart();
                    Render();
                    // print rendering time
                    debug.Print(stopwatch.ElapsedMilliseconds.ToString() + "ms",0 ,0 ,0xffffff);
                }
            }
        }

        //uses camera to loop over pixels of screen plane, generating ray for each pixel to find nearest intersection
        public void Render()
        {
            Ray ray = new Ray();
            ray.origin = camera.position;
            ray.distance = 1000f;

            Vector3 row = camera.topright - camera.topleft;
            Vector3 column = camera.bottomleft - camera.topleft;
            int w = screen.width;
            int h = screen.height;

            Vector3 stepx = row / (h*aa - 1);
            Vector3 stepy = column / (w*aa - 1);
            Vector3 location = camera.topleft;
            Vector3 locationy = location;
            bool renderDebug = false;


            float aa2inv = 1f / (aa * aa);

            for (int y = 0; y<h; y++) {
                for (int x = 0; x < w; x++) {
                    Vector3 c = Vector3.Zero;
                    Vector3 location2 = location;
                    Vector3 locationy2 = location2;
                    renderDebug = y == h / 2 && x % 16 == 0;
                    for (int y2 = 0; y2 < aa; y2++) {
                        for (int x2 = 0; x2 < aa; x2++) {
                            if (restart)
                                return;
                            ray.direction = location2 - ray.origin;
                            ray.direction.Normalize();
                            c += Vector3.Clamp(Trace(ray ,0 ,renderDebug), min, max) * aa2inv;
                            location2 += stepx;
                        }
                        location2 = (locationy2 += stepy);
                    }
                    screen.Plot(x ,y ,c);
                    location += stepx * aa;
                }
                location = (locationy += stepy * aa);
            }
        }

        Vector3 Trace(Ray r, int depth, bool renderDebug)
        {
            Vector3 c = Vector3.Zero;
            if (depth > maxdepth)
                return c;
            
            intersection i = scene.Intersect(r);
            if (i != null) {
                Vector3 position = r.origin + r.direction * i.distance;
                Vector3 c2 = i.nearest.Color(position);
                if (i.nearest.material.mirror) {
                    c = Trace(Reflect(r ,i) ,depth + 1 ,renderDebug) * c2;
                }
                else if (i.nearest.material.refraction > 0 )
                    {
                    float srefect = schlick(r, i);
                    c = srefect*(Trace(Reflect(r, i), depth + 1, renderDebug) * c2 +
                        (1 - srefect)*(Trace(Refract(r, i), depth + 1, renderDebug) * c2));
                    }
                else {
                    if (i.nearest.material.reflectance > 0)
                        c += Trace(Reflect(r ,i) ,depth + 1 ,renderDebug) * c2 * i.nearest.material.reflectance;
                    Ray shadow = new Ray();
                    foreach (light l in scene.lights) {
                        shadow.direction = l.location - position;
                        shadow.distance = shadow.direction.Length;
                        if (shadow.distance < l.range) {
                            shadow.direction /= shadow.distance;
                            shadow.distance -= 2 * epsilon;
                            shadow.origin = position + shadow.direction * epsilon;
                            float nl = Vector3.Dot(i.normal ,shadow.direction);
                            if (nl > 0) {
                                intersection i2 = scene.IntersectFast(shadow ,l.previous[depth]);
                                if (i2 == null) {
                                    l.previous[depth] = null;
                                    c += c2 * l.colour * (1 / (shadow.distance * shadow.distance)) * nl * (1 - i.nearest.material.reflectance);
                                }
                                else {
                                    l.previous[depth] = i2.nearest;
                                }
                                if (renderDebug) {
                                    Vector3 ip = (shadow.origin + shadow.direction * shadow.distance);
                                    debug.Line(TX(shadow.origin.X) ,TY(shadow.origin.Z) ,TX(ip.X) ,TY(ip.Z) ,i2 == null ? 0x00ffff : 0xff0000);
                                }
                            }
                        }
                    }
                }
            }
            else {
                c += scene.environment.Color(r.direction);
            }
            

            if (renderDebug)
            {
                float d = i != null ? i.distance : r.distance;
                Vector3 ip = (r.origin + r.direction * d);
                debug.Line(TX(r.origin.X), TY(r.origin.Z), TX(ip.X), TY(ip.Z), 0xffff00);
                if (i != null)
                    debug.Line(TX(ip.X), TY(ip.Z), TX(ip.X + i.normal.X), TY(ip.Z + i.normal.Z), 0x00ff00);
            }
            return c;
        }

        Ray Reflect(Ray ray ,intersection i) {
            Ray reflection = new Ray();
            reflection.direction = ray.direction - 2 * Vector3.Dot(ray.direction ,i.normal) * i.normal;
            reflection.origin = ray.origin + ray.direction* i.distance + reflection.direction* epsilon;
            reflection.distance = ray.distance - i.distance;
            return reflection;
        }

        Ray Refract(Ray ray, intersection i)
        {
            Ray refraction = new Ray();
            Vector3 d = new Vector3();
            d = ray.direction * -1;
            float refrac = i.nearest.material.refraction;
            float cosi = Vector3.Dot(d, i.normal);
            float sini = 1/refrac*refrac*(1 - cosi * cosi);
            refraction.direction = (1 / refrac) * ray.direction + (((float)((1 / refrac) * cosi - Math.Sqrt(1 - sini)) * i.normal));
            refraction.origin = ray.origin + ray.direction * i.distance + refraction.direction * epsilon;
            refraction.distance = ray.distance - i.distance;
            return refraction;
        }

        float schlick(Ray ray, intersection i)
        {
            // here 1 should be the value of refraction index of the incomming ray 
            float r = (1 - i.nearest.material.refraction) / (1 + i.nearest.material.refraction);
            float r0 = r * r;

            Vector3 d = new Vector3();
            d = ray.direction * -1;
            float cos1 = 1 - Vector3.Dot(d, i.normal);
            float r1 = r0 + (1 - r0) * cos1 * cos1 * cos1 * cos1 * cos1;
            return r1;

        }


        private void RenderDebug() {
            // print relevant parameters
            debug.Print("AA " + aa* aa,128 ,0 ,0xffffff);
            debug.Print("FOV " + camera.viewAngle,256 ,0 ,0xffffff);

        	// render slice of primitives at y=0 in debug view
        	foreach (primitive p in scene.primitives) {
        		if (p is sphere) {
        			sphere s = (sphere)p;
                    Vector3 pos = Vector3.Transform(s.position - camera.position, Matrix4.CreateFromAxisAngle(Vector3.UnitX ,-camera.angleX));
                    float y = pos.Y;
        			if (y <= s.radius && y >= -s.radius) {
        				float r = (float)Math.Sqrt(s.radius * s.radius - y * y);
        				for (int i = 0; i < 100; i++) {

        					float x1 = (float)Math.Cos(Math.PI * i / 50.0) * r;
        					float y1 = (float)Math.Sin(Math.PI * i / 50.0) * r;
        					float x2 = (float)Math.Cos(Math.PI * (i + 1) / 50.0) * r;
        					float y2 = (float)Math.Sin(Math.PI * (i + 1) / 50.0) * r;

        					debug.Line(TX(s.position.X + x1) ,TY(s.position.Z + y1) ,TX(s.position.X + x2) ,TY(s.position.Z + y2) ,TC(p.colour));
        				}
        			}
        		}
        	}

        	// render camera position in debug view
        	debug.Plot(TX(camera.position.X) ,TY(camera.position.Z) ,0xffffff);
        	debug.Plot(TX(camera.position.X) - 1 ,TY(camera.position.Z) ,0xffffff);
        	debug.Plot(TX(camera.position.X) - 1 ,TY(camera.position.Z) - 1 ,0xffffff);
        	debug.Plot(TX(camera.position.X) ,TY(camera.position.Z) - 1 ,0xffffff);

        	// render screen plane in debug view
        	debug.Line(TX(camera.topleft.X) ,TY(camera.topleft.Z) ,TX(camera.topright.X) ,TY(camera.topright.Z) ,0xffffff);
        	debug.Line(TX(camera.topright.X) ,TY(camera.topright.Z) ,TX(camera.bottomright.X) ,TY(camera.bottomright.Z) ,0xffffff);
        	debug.Line(TX(camera.bottomright.X) ,TY(camera.bottomright.Z) ,TX(camera.bottomleft.X) ,TY(camera.bottomleft.Z) ,0xffffff);
        	debug.Line(TX(camera.bottomleft.X) ,TY(camera.bottomleft.Z) ,TX(camera.topleft.X) ,TY(camera.topleft.Z) ,0xffffff);
        }

        int TX(float x) {
            x += 5;
            x /= 10;
            x *= debug.width;
            return (int)x;
        }

        int TY(float y) {
            y = -y;
        	y += 5;
            y /= 10;
            y *= debug.height;
        	return (int)y;
        }

        int TC(Vector3 c) {
            return ((int)(Math.Min(255f * c.X ,255f)) << 16) + ((int)(Math.Min(255f * c.Y ,255f)) << 8) + (int)(Math.Min(255f * c.Z ,255f));
        }
    }
}
