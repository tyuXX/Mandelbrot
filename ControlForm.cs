namespace Mandelbrot;

public partial class ControlForm : Form
{
    private readonly MainForm parentForm;

    public ControlForm(MainForm parentForm)
    {
        InitializeComponent();
        this.parentForm = parentForm;
        Owner = parentForm;

        foreach (MandelbrotRendererBase r in parentForm.RendererList) cbRenderer.Items.Add(r);
        cbRenderer.SelectedIndex = 0;
        cbRenderer.SelectedIndexChanged += cbRenderer_SelectedIndexChanged;
    }
    
    private void udIterations_ValueChanged(object sender, EventArgs e)
    {
        parentForm.OnParametersChanged();
    }
    
    private void btnReset_Click(object sender, EventArgs e)
    {
        parentForm.ResetInitialParams();
        parentForm.OnParametersChanged();
    }

    private void cbRenderer_SelectedIndexChanged(object sender, EventArgs e)
    {
        parentForm.SetRenderer(cbRenderer.SelectedIndex);
        parentForm.OnParametersChanged();
    }
}