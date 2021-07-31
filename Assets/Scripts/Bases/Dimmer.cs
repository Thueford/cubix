using System.Collections.Generic;
using UnityEngine;

public struct Dimmer
{
    private float value, factor, min, max;

    public Dimmer(float factor = 1, float initVal = 0, float min = 0, float max = 1)
    {
        this.min = min;
        this.max = max;
        this.factor = factor;
        this.value = initVal;
    }

    public float Update(bool active)
    {
        value += factor * (active ? Time.deltaTime : -Time.deltaTime);
        value = Mathf.Clamp01(value);
        return this;
    }

    public static implicit operator float(Dimmer d) => d.min + d.value * (d.max - d.min);
    public Color fRed => new Color(this, 1, 1);
    public Color fGreen => new Color(1, this, 1);
    public Color fBlue => new Color(1, 1, this);
    public Color fCol => new Color(this, this, this);
}
