using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Noise
{
    //A version of value-noise using Lanczos-Resampling. Also has classic billiniar-noise
    //Made by Zomare
    
    class ValueNoise
    {
        //Hash-Function for rng, outputs in range [-1, 1]
        static float Random(int x1, int y1)
        {
            byte[] table = {151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7,
                            225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247,
                            120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33,
                            88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134,
                            139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220,
                            105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80,
                            73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86,
                            164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38,
                            147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189,
                            28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101,
                            155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232,
                            178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12,
                            191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181,
                            199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236,
                            205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180};

            return ((float)table[(x1 + table[y1 & 255]) & 255] / 255f) * 2 - 1;
        }


        public static float Compute (float x, float y)
        {
            int ix = (int)x;
            int iy = (int)y;

            float dx = x - ix;
            float dy = y - iy;

            //averages used for normalizing
            float avgY = 0;
            float avgX = 0;

            //Calculating lookup table for faster horizontal interpolation
            float[] lanczosX = new float[6];

            for (int px = -2; px < 4; px++)
            {
                float f = Lanczos(dx - px);
                avgX += f;
                lanczosX[px + 2] = f;
            }

            float n = 0;

            for (int py = -2; py<4; py++)
            {
                float a = 0;

                for (int px = -2; px < 4; px++)
                {
                    a += Random(ix + px, iy+py) * lanczosX[px+2];
                }

                a /= avgX;
                n += a * Lanczos(dy - py);
                avgY += Lanczos(dy - py);
            }

            //!Not correctly normalized!
            return smoothstep(-1, 1, (n / avgY / 1.25f+1)/2f);
        }

        //Lanczos function used for interpolation
        //L(x)=sinc(x)sinc(x/a)
        static float Lanczos(float t)
        {
            if (t==0)
            {
                return 1;
            } else if (t>4 || t < -4)
            {
                return 0;
            }

            return 3*(float)((Math.Sin (Math.PI*t)*Math.Sin (Math.PI*(t/3)))/(Math.PI*Math.PI*t*t));
        }


        //Left in for the purpose of maybe using it later
        static float Sinc (float x)
        {
            return (float)(Math.Sin(Math.PI * x) / (Math.PI * x));
        }

        //Outputs an low frequency octave of noise as a png
        static public void Test(int d)
        {
            var bm = new Bitmap(d, d);

            int off = new Random().Next(-1000, 1000);

            for (int y = 0; y < d; y++)
            {
                for (int x = 0; x < d; x++)
                {
                    int c = (int)(255*(Compute(x * 0.025f, y * 0.025f + off)+1)/2f);

                    bm.SetPixel(x, y, Color.FromArgb(c, c, c));
                }
            }

            bm.Save("test.png");
        }

        //Generates a worldmap and outputs it as a png
        static public void Generate(int d)
        {
            var bm = new Bitmap(d, d);

            int off = new Random ().Next (-1000, 1000);

            for (int y = 0; y < d; y++)
            {
                for (int x = 0; x < d; x++)
                {
                    float a = Math.Abs(ComputeFractal(x * 0.02f, y * 0.02f + off, 4));

                    if (a > 0.2f)
                    {
                        a = 0.2f;
                    }
                    a /= 0.2f;


                    a *= ComputeFractal(x * 0.025f, y * 0.025f + 1000+off, 8)*0.5f + 0.5f;

                    Color c = Color.Aqua;


                    if (a > 0.9f)
                    {
                        c = Color.White;
                    }
                    else if (a > 0.75f)
                    {
                        c = Color.LightGray;
                    } else if (a > 0.4f)
                    {
                        c = Color.ForestGreen;
                    }

                    bm.SetPixel(x, y, c);
                }
            }

            bm.Save("map.png");
        }

        //Fractal Noise; n: amount of octaves
        static public float ComputeFractal(float x, float y, int n)
        {
            float a = 0;
            float avg = 0;
            float w = 1;
            float frq = 1;

            for (int i = 0; i < n; i++)
            {
                a += Compute(x * frq, y * frq) * w;
                avg += w;
                w *= 0.25f;
                frq *= 3;
            }

            return a / avg;
        }

        //"Normal" Value-Noise
        static public float ComputeLinear  (float x, float y)
        {
            int ix = (int)x;
            int iy = (int)y;

            float dx = x - ix;
            float dy = y - iy;

            float fx1 = lerp(Random (ix, iy), Random (ix+1, iy), dx);
            float fx2 = lerp(Random(ix, iy+1), Random(ix+1, iy+1), dx);

            return lerp(fx1, fx2, dy);
        }


        //Basic interpolation Functions

        //7th order smoothstep
        static float smootherstep(float a1, float a2, float t)
        {
            return lerp(a1, a2, t * t * t * t * (t * (t * (70 - 20 * t) - 84) + 35));
        }

        //Normal smoothstep
        static float smoothstep(float a1, float a2, float t)
        {
            return lerp(a1, a2, t*t*(3-2*t));
        }

        static float lerp(float a1, float a2, float t)
        {
            t = Math.Clamp(t, 0f, 1f);

            return (1 - t) * a1 + t * a2;
        }
    }
}
