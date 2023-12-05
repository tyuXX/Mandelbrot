using System.Numerics;

namespace Mandelbrot.FloatRenderer;

public class ComplexPlaneRenderer : MandelbrotRendererBase
{
    private Complex xmin, xmax, ymin, ymax;
    private Complex xorigin, yorigin, xextent;

    public ComplexPlaneRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "DualQuad-precision (Non-Practical)";
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
        Complex ratio = screenWidth / (Complex) screenHeight;
        xmin = xorigin;
        ymin = yorigin;
        xmax = xmin + xextent;
        ymax = ymin + xextent / ratio;

        int maxY = tParams.startY + tParams.startHeight;
        int maxX = tParams.startX + tParams.startWidth;
        Complex x, y, x0, y0;
        Complex xsq, ysq;
        Complex xscale = (xmax - xmin) / screenWidth;
        Complex yscale = (ymax - ymin) / screenHeight;
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
                while (xsq.Real + ysq.Real < 4.0f)
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
        Complex xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        Complex ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
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
        Complex xpos = xmin + posX * (xmax - xmin) / screenWidth;
        Complex ypos = ymin + posY * (ymax - ymin) / screenHeight;
        Complex xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        Complex yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        Complex newXextent = xmax - xmin;
        newXextent *= factor;

        Complex newYextent = ymax - ymin;
        newYextent *= factor;

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}