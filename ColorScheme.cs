namespace Mandelbrot;

public static class ColorScheme
{
    private static uint[] CreateColorScheme(uint[] colorArray, int numElements)
    {
        int elementsPerStep = numElements / (colorArray.Length - 1);
        uint[] colors = new uint[numElements];
        float r = 0f, g = 0f, b = 0f;
        float rInc = 0f, gInc = 0f, bInc = 0f;
        
        int cIndex = 0;
        int cCounter = 0;
        for (int i = 0; i < numElements; i++)
        {
            if (cCounter == 0)
            {
                b = colorArray[cIndex] & 0xff;
                g = (colorArray[cIndex] & 0xff00) >> 8;
                r = (colorArray[cIndex] & 0xff0000) >> 16;
                if (cIndex < colorArray.Length - 1)
                {
                    bInc = ((colorArray[cIndex + 1] & 0xff) - b) / elementsPerStep;
                    gInc = (((colorArray[cIndex + 1] & 0xff00) >> 8) - g) / elementsPerStep;
                    rInc = (((colorArray[cIndex + 1] & 0xff0000) >> 16) - r) / elementsPerStep;
                }

                cIndex++;
                cCounter = elementsPerStep;
            }
            colors[i] = 0xff000000 | ((uint) b << 16) | ((uint) g << 8) | (uint) r;
            b = b + bInc;
            g = g + gInc;
            r = r + rInc;
            if (b < 0f) b = 0f;
            if (g < 0f) g = 0f;
            if (r < 0f) r = 0f;
            if (b > 255f) b = 255f;
            if (g > 255f) g = 255f;
            if (r > 255f) r = 255f;
            cCounter--;
        }
        return colors;
    }
    public static uint[] CreateColorScheme(Color[] colorArray, int numElements)
    {
        uint[] colors = new uint[colorArray.Length];
        for (int i = 0; i < colorArray.Length; i++)
        {
            colors[i] = (uint) colorArray[i].ToArgb();
            uint b = colors[i] & 0xff;
            uint g = (colors[i] & 0xff00) >> 8;
            uint r = (colors[i] & 0xff0000) >> 16;
            colors[i] = 0xff000000 | (b << 16) | (g << 8) | r;
        }

        return CreateColorScheme(colors, numElements);
    }
}