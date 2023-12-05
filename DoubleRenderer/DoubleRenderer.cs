﻿namespace Mandelbrot.FloatRenderer;

public class DoubleRenderer : MandelbrotRendererBase
{
    private double xmin, xmax, ymin, ymax;
    private double xorigin, yorigin, xextent;

    public DoubleRenderer(Form parentContext, uint[] colorPalette, int colorPaletteSize)
        : base(parentContext, colorPalette, colorPaletteSize)
    {
    }

    public override string ToString()
    {
        return "Double-precision";
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
        double x, y, x0, y0;
        double xsq, ysq;
        double xscale = (xmax - xmin) / screenWidth;
        double yscale = (ymax - ymin) / screenHeight;
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
        double xpos = xmin + mouseX * (xmax - xmin) / screenWidth;
        double ypos = ymin + mouseY * (ymax - ymin) / screenHeight;
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
        double xpos = xmin + posX * (xmax - xmin) / screenWidth;
        double ypos = ymin + posY * (ymax - ymin) / screenHeight;
        double xOffsetRatio = (xpos - xmin) / (xmax - xmin);
        double yOffsetRatio = (ypos - ymin) / (ymax - ymin);

        double newXextent = xmax - xmin;
        newXextent *= factor;

        double newYextent = ymax - ymin;
        newYextent *= factor;

        TerminateThreads();
        xextent = newXextent;
        xorigin = xpos - xextent * xOffsetRatio;
        yorigin = ypos - newYextent * yOffsetRatio;
        Draw(numIterations, numThreads);
    }
}