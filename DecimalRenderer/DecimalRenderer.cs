namespace Mandelbrot.Decimal;

public class DecimalRenderer : MandelbrotRendererBase
{
    private decimal xmin, xmax, ymin, ymax;
    private decimal xorigin, yorigin, xextent;

    public DecimalRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "Quad-precision";
    }

    public override void SetInitialParams(double xorigin, double yorigin, double xextent)
    {
        this.xorigin =Convert.ToDecimal( xorigin);
        this.yorigin =Convert.ToDecimal( yorigin);
        this.xextent =Convert.ToDecimal( xextent);
    }

    protected override void DrawInternal(object threadParams)
    {
        MandelThreadParams tParams = (MandelThreadParams) threadParams;
        decimal ratio = screenWidth / (decimal) screenHeight;
        xmin = xorigin;
        ymin = yorigin;
        xmax = xmin + xextent;
        ymax = ymin + xextent / ratio;

        int maxY = tParams.startY + tParams.startHeight;
        int maxX = tParams.startX + tParams.startWidth;
        decimal x, y, x0, y0;
        decimal xsq, ysq;
        decimal xscale = (xmax - xmin) / screenWidth;
        decimal yscale = (ymax - ymin) / screenHeight;
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

                xsq = ysq = x = y = 0.0M;
                while (xsq + ysq < 4.0M)
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
        decimal xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        decimal ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
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
        decimal xpos = xmin + posX * (xmax - xmin) / screenWidth;
        decimal ypos = ymin + posY * (ymax - ymin) / screenHeight;
        decimal xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        decimal yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        decimal newXextent = xmax - xmin;
        newXextent *= Convert.ToDecimal(factor);

        decimal newYextent = ymax - ymin;
        newYextent *= Convert.ToDecimal(factor);

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}