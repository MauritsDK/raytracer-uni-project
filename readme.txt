Team members
- Gabriëlle Zuiddam (4285794)
- Maurits Dick      (5537509)
- Arjan Adriaanse   (5757959)

Controls
- Move camera: arrow keys and left control/shift
- Rotate camera: W/A/S/D
- Adjust field of view: Z/X
- Toggle anti-aliasing: space bar

Debug legend
- White dot and lines: camera position and screen
- Yellow line: regular ray
- Blue line: visible shadow ray
- Red line: blocked shadow ray
- Green line: intersection normal

Bonus assignments
- Anti-aliasing: raytracer.Render traces N*N rays for each pixel according to the value of raytracer.aa
- Textured HDR skydome: scene.environment contains a HDR floating point texture loaded from a PFM file which the direction of a ray is mapped to when there is no intersection
- Refraction: raytracer.Trace can reflect and refract rays according to the Fresnel factor which is calculated using Schlick's approximation

Materials used
- Jacco's slides
- http://www.pauldebevec.com/Probes/
- http://www.pauldebevec.com/Research/HDR/PFM/
