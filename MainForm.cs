using System.Runtime.InteropServices;
using Mandelbrot.Decimal;
using Mandelbrot.FloatRenderer;
using static System.Drawing.Imaging.PixelFormat;

namespace Mandelbrot;

public sealed partial class MainForm : Form
{
    private const int WHEEL_DELTA = 120;
    private const double ZOOM_FACTOR_IN = 0.8;
    private const double ZOOM_FACTOR_OUT = 1.2;
    private Bitmap bmp;

    private uint[] bmpBits;

    private ControlForm controlForm;
    private MandelbrotRendererBase currentRenderer;
    private GCHandle gcHandle;
    private int moveX0, moveY0;
    private static double _currentZoom = 1.00f;

    private bool moving;
    public List<MandelbrotRendererBase> RendererList { get; }
    public MainForm()
    {
        InitializeComponent();
        Text = Application.ProductName;
        bmpBits = Array.Empty<uint>();

        int colorPaletteSize = 1024;
        uint[] colorPalette = ColorScheme.CreateColorScheme(
            new[] {Color.BurlyWood, Color.Chocolate, Color.Tan, Color.Sienna, Color.LightSteelBlue, Color.BurlyWood},
            colorPaletteSize);

        RendererList = new List<MandelbrotRendererBase>
        {
            new DoubleRenderer(this, colorPalette, colorPaletteSize),
            new BigIntegerRenderer.BigIntegerRenderer(this, colorPalette, colorPaletteSize),
            new SimpleBigIntRenderer.SimpleBigIntRenderer(this, colorPalette, colorPaletteSize),
            new BigFloatRenderer.BigFloatRenderer(this, colorPalette, colorPaletteSize),
            new DecimalRenderer(this, colorPalette, colorPaletteSize),
            new FloatRenderer.FloatRenderer(this, colorPalette, colorPaletteSize),
            new ComplexPlaneRenderer(this, colorPalette, colorPaletteSize)
        };
        currentRenderer = RendererList[0];
        foreach (MandelbrotRendererBase r in RendererList) ResetInitialParams(r);

        DoubleBuffered = true;
        MouseWheel += MainFormMouseWheel;
    }
    
    private void MainFormLoad(object sender, EventArgs e)
    {
        controlForm = new ControlForm(this);
        controlForm.Show();
        MainFormSizeChanged(null, null);
    }

    private void MainFormFormClosed(object sender, FormClosedEventArgs e)
    {
        currentRenderer.TerminateThreads();
        FreeHeapMem();
        controlForm.Close();
    }

    private void FreeHeapMem()
    {
        if (bmp is not null)
        {
            bmp.Dispose();
            bmp = null;
        }

        if (gcHandle.IsAllocated) gcHandle.Free();
    }

    private void MainFormSizeChanged(object sender, EventArgs e)
    {
        if (currentRenderer == null) return;

        currentRenderer.TerminateThreads();

        int screenWidth = ClientSize.Width;
        int screenHeight = ClientSize.Height;
        if (screenWidth < 1) screenWidth = 1;
        if (screenHeight < 1) screenHeight = 1;

        int bufferLength = screenWidth * screenHeight;
        if (bufferLength > bmpBits.Length)
        {
            FreeHeapMem();
            bmpBits = new uint[bufferLength * 4];
            gcHandle = GCHandle.Alloc(bmpBits, GCHandleType.Pinned);
        }

        bmp?.Dispose();
        bmp = new Bitmap(screenWidth, screenHeight, screenWidth * 4, Format32bppArgb,
            gcHandle.AddrOfPinnedObject());

        currentRenderer.UpdateBitmapBits(bmpBits);
        currentRenderer.UpdateScreenDimensions(screenWidth, screenHeight);
        OnParametersChanged();
    }

    public void SetRenderer(int index)
    {
        _currentZoom = 1.00f;
        currentRenderer.TerminateThreads();
        currentRenderer = RendererList[index];
        MainFormSizeChanged(null, null);
    }

    private static void ResetInitialParams(MandelbrotRendererBase renderer)
    {
        _currentZoom = 1.00f;
        renderer.SetInitialParams(-2.0, -1.2, 3.0);
    }

    public void ResetInitialParams()
    {
        _currentZoom = 1.00f;
        ResetInitialParams(currentRenderer);
    }

    public void OnParametersChanged()
    {
        currentRenderer.TerminateThreads();
        currentRenderer.Draw((int) controlForm.udIterations.Value, (int) controlForm.udNumThreads.Value);
    }

    private void MainFormPaint(object sender, PaintEventArgs e)
    {
        e.Graphics.DrawImageUnscaled(bmp, 0, 0);
    }

    private void timerRefresh_Tick(object sender, EventArgs e)
    {
        Invalidate();
    }

    private void MainFormMouseDown(object sender, MouseEventArgs e)
    {
        moving = true;
        moveX0 = e.X;
        moveY0 = e.Y;
    }

    private void MainFormMouseMove(object sender, MouseEventArgs e)
    {
        controlForm.txtInfo.Text = $@"Zoom:{_currentZoom:F}x || Current Coordinates:{currentRenderer.GetCoordinateStr(e.X, e.Y)}";
        if (!moving) return;
        currentRenderer.Move(e.X - moveX0, e.Y - moveY0);
        moveX0 = e.X;
        moveY0 = e.Y;
    }

    private void MainFormMouseUp(object sender, MouseEventArgs e)
    {
        moving = false;
    }

    private void MainFormMouseWheel(object? sender, MouseEventArgs e)
    {
        double factor = 1.0;
        if (e.Delta > 0)
            factor = Math.Pow(ZOOM_FACTOR_IN, e.Delta / (double) WHEEL_DELTA);
        else if (e.Delta < 0) factor = Math.Pow(ZOOM_FACTOR_OUT, -e.Delta / (double) WHEEL_DELTA);
        currentRenderer.Zoom(e.X, e.Y, factor);
        _currentZoom /= factor;
    }
}