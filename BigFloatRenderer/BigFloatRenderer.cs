namespace Mandelbrot.BigFloatRenderer;

public class BigFloatRenderer : MandelbrotRendererBase
{
    private BigFloat xmin, xmax, ymin, ymax;
    private BigFloat xorigin, yorigin, xextent;

    public BigFloatRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "Giga-precision (Unusable / Too laggy)";
    }

    public override void SetInitialParams(double xorigin, double yorigin, double xextent)
    {
        this.xorigin = xorigin;
        this.yorigin = yorigin;
        this.xextent = xextent;
    }

    protected override void DrawInternal(object threadParams)
    {
        MandelThreadParams tParams = (MandelThreadParams) threadParams;
        BigFloat ratio = screenWidth / (BigFloat) screenHeight;
        xmin = xorigin;
        ymin = yorigin;
        xmax = xmin + xextent;
        ymax = ymin + xextent / ratio;

        int maxY = tParams.startY + tParams.startHeight;
        int maxX = tParams.startX + tParams.startWidth;
        BigFloat x, y, x0, y0;
        BigFloat xsq, ysq;
        BigFloat xscale = (xmax - xmin) / screenWidth;
        BigFloat yscale = (ymax - ymin) / screenHeight;
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
        BigFloat xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        BigFloat ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
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
        BigFloat xpos = xmin + posX * (xmax - xmin) / screenWidth;
        BigFloat ypos = ymin + posY * (ymax - ymin) / screenHeight;
        BigFloat xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        BigFloat yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        BigFloat newXextent = xmax - xmin;
        newXextent *= factor;

        BigFloat newYextent = ymax - ymin;
        newYextent *= factor;

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}