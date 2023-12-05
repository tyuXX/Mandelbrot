namespace Mandelbrot;

public abstract class MandelbrotRendererBase
{
    protected uint[] bitmapBits;
    protected uint[] colorPalette;
    protected int colorPaletteSize;

    private readonly List<Thread> currentThreads;

    protected int numIterations;
    protected int numThreads;
    private readonly Form parentContext;
    protected int screenWidth, screenHeight;
    protected bool terminateThreads;


    protected MandelbrotRendererBase(Form parentContext, uint[] colorPalette, int colorPaletteSize)
    {
        this.parentContext = parentContext;
        this.colorPalette = colorPalette;
        this.colorPaletteSize = colorPaletteSize;
        currentThreads = new List<Thread>();
    }
    public abstract void SetInitialParams(double xorigin, double yorigin, double xextent);
    
    public void UpdateBitmapBits(uint[] bitmapBits)
    {
        this.bitmapBits = bitmapBits;
    }

    public void UpdateScreenDimensions(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
    }

    public void Draw(int numIterations, int numThreads)
    {
        this.numIterations = numIterations;
        this.numThreads = numThreads;

        int yInc = screenHeight / numThreads;
        int yPos = 0;
        for (int i = 0; i < numThreads; i++)
        {
            Thread thread = new Thread(DrawInternal);
            currentThreads.Add(thread);
            MandelThreadParams p = new MandelThreadParams();
            p.parentForm = parentContext;
            p.startX = 0;
            p.startY = yPos;
            p.startWidth = screenWidth;
            if (i == numThreads - 1)
                p.startHeight = screenHeight - yPos;
            else
                p.startHeight = yInc;
            thread.Start(p);
            yPos += yInc;
        }
    }

    public void TerminateThreads()
    {
        terminateThreads = true;
        foreach (Thread t in currentThreads) t.Join();
        currentThreads.Clear();
        terminateThreads = false;
    }

    protected abstract void DrawInternal(object threadParams);

    public abstract void Move(int moveX, int moveY);

    public abstract void Zoom(int posX, int posY, double factor);

    public abstract string GetCoordinateStr(int mouseX, int mouseY);

    protected struct MandelThreadParams
    {
        public Form parentForm;
        public int startX, startY, startWidth, startHeight;
    }
}