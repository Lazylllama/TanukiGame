#ifndef NOISE_INCLUDED 
#define NOISE_INCLUDED

float2 hash2(float2 input)
{
    float a = input.x * 127.1 + input.y * 311.7;
    float b = input.x * 265.7 + input.y * 945.3;

    a = frac(sin(a) * 43758.5453);
    b = frac(cos(b) * 83712.8374);

    return float2(a, b);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash2(i).x;
    float b = hash2(i + float2(1.0, 0.0)).x;
    float c = hash2(i + float2(0.0, 1.0)).x;
    float d = hash2(i + float2(1.0, 1.0)).x;

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

#endif
