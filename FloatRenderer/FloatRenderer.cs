namespace Mandelbrot.FloatRenderer;

public class FloatRenderer : MandelbrotRendererBase
{
    private float xmin, xmax, ymin, ymax;
    private float xorigin, yorigin, xextent;

    public FloatRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "Single-precision (Might be broken)";
    }

    public override void SetInitialParams(double xorigin, double yorigin, double xextent)
    {
        this.xorigin = Convert.ToSingle(xorigin);
        this.yorigin = Convert.ToSingle(yorigin);
        this.xextent = Convert.ToSingle(xextent);
    }

    protected override void DrawInternal(object threadParams)
    {
        MandelThreadParams tParams = (MandelThreadParams) threadParams;
        float ratio = screenWidth / (float) screenHeight;
        xmin = xorigin;
        ymin = yorigin;
        xmax = xmin + xextent;
        ymax = ymin + xextent / ratio;

        int maxY = tParams.startY + tParams.startHeight;
        int maxX = tParams.startX + tParams.startWidth;
        float x, y, x0, y0;
        float xsq, ysq;
        float xscale = (xmax - xmin) / screenWidth;
        float yscale = (ymax - ymin) / screenHeight;
        int iteration;
        int iterScale = 1;
        int px, py;

        if (numIterations < colorPaletteSize) iterScale = colorPaletteSize / numIterations;

        for (py = tParams.startY; py < maxY; py++)
        {
            y0 = ymin + py * yscale;

            for (px = tParams.startX; px < maxX; px++)
            {
                x0 = xmin + px * xscale;
                iteration = 0;

                xsq = ysq = x = y = 0.0f;
                while (xsq + ysq < 4.0)
                {
                    y = x * y;
                    y += y;
                    y += y0;
                    x = xsq - ysq + x0;
                    xsq = x * x;
                    ysq = y * y;

                    if (iteration++ > numIterations) break;
                }

                if (iteration >= numIterations)
                    bitmapBits[py * screenWidth + px] = 0xFF000000;
                else
                    bitmapBits[py * screenWidth + px] = colorPalette[iteration * iterScale % colorPaletteSize];
            }

            if (terminateThreads) break;
        }

        tParams.parentForm.BeginInvoke(new MethodInvoker(delegate { tParams.parentForm.Invalidate(); }));
    }


    public override string GetCoordinateStr(int mouseX, int mouseY)
    {
        float xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        float ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
        return xpos + ", " + ypos;
    }

    public override void Move(int moveX, int moveY)
    {
        TerminateThreads();
        xorigin -= moveX * (xmax - xmin) / screenWidth;
        yorigin -= moveY * (ymax - ymin) / screenHeight;
        Draw(numIterations, numThreads);
    }

    public override void Zoom(int posX, int posY, double factor)
    {
        float xpos = xmin + posX * (xmax - xmin) / screenWidth;
        float ypos = ymin + posY * (ymax - ymin) / screenHeight;
        float xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        float yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        float newXextent = xmax - xmin;
        newXextent *= Convert.ToSingle(factor);

        float newYextent = ymax - ymin;
        newYextent *= Convert.ToSingle(factor);

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}