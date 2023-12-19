namespace Mandelbrot.SimpleBigIntRenderer;

public class SimpleBigIntRenderer : MandelbrotRendererBase
{
    private BigDecimal xmin, xmax, ymin, ymax;
    private BigDecimal xorigin, yorigin, xextent;

    public SimpleBigIntRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "Naive BigInteger";
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
        double ratio = screenWidth / (double) screenHeight;
        xmin = xorigin;
        ymin = yorigin;
        xmax = xmin + xextent;
        ymax = ymin + xextent / ratio;

        int maxY = tParams.startY + tParams.startHeight;
        int maxX = tParams.startX + tParams.startWidth;
        BigDecimal x0 = 0, y0 = 0;
        BigDecimal d = xmax - xmin;
        BigDecimal xscale = d / screenWidth;
        BigDecimal yscale = (ymax - ymin) / screenHeight;
        int iteration;
        int iterScale = 1;
        int px, py;
        BigDecimal bailout = 4.0;

        BigDecimal[] x0Row = new BigDecimal[maxX - tParams.startX];
        for (int i = 0; i < x0Row.Length; i++)
        {
            x0Row[i].Set(xscale);
            x0Row[i].Multiply(i + tParams.startX);
            x0Row[i].Add(xmin);
        }

        BigDecimal x = 0;
        BigDecimal y = 0;
        BigDecimal xsq = 0;
        BigDecimal ysq = 0;

        if (numIterations < colorPaletteSize) iterScale = colorPaletteSize / numIterations;

        for (py = tParams.startY; py < maxY; py++)
        {
            y0.Set(yscale);
            y0.Multiply(py);
            y0.Add(ymin);
            y0.Truncate();

            for (px = tParams.startX; px < maxX; px++)
            {
                x0.Set(x0Row[px - tParams.startX]);

                x.Zero();
                y.Zero();
                xsq.Zero();
                ysq.Zero();
                iteration = 0;

                while (xsq + ysq < bailout)
                {
                    y.Multiply(x);
                    y.Truncate();
                    y.Add(y);
                    y.Add(y0);

                    x.Set(xsq);
                    x.Add(-ysq);
                    x.Add(x0);
                    x.Truncate();

                    xsq.Set(x);
                    xsq.Multiply(x);

                    ysq.Set(y);
                    ysq.Multiply(y);

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
        BigDecimal xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        BigDecimal ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
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
        BigDecimal xpos = xmin + posX * (xmax - xmin) / screenWidth;
        BigDecimal ypos = ymin + posY * (ymax - ymin) / screenHeight;
        BigDecimal xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        BigDecimal yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        BigDecimal newXextent = xmax - xmin;
        newXextent *= factor;

        BigDecimal newYextent = ymax - ymin;
        newYextent *= factor;

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}