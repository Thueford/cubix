using System.Collections.Generic;
using UnityEngine;

struct Helper
{
    public static float round(float f, float d = 1e3f) => Mathf.RoundToInt(f * d) / d;
    public static int F(bool v, int p) => v ? 1 << p : 0;
    public static int G(int x, int p) => (x >> p) & 1;
    public static int C2B(Color c) => F(c.r > 0, 0) + F(c.g > 0, 1) + F(c.b > 0, 2);
    public static Color B2C(int c) => new Color(G(c, 0), G(c, 1), G(c, 2), 1);
}
