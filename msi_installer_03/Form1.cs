namespace c_sharp;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    public void FormLayout()
    {
        this.Name = "Form1";
        this.Text = "Form1";
        this.Size = new System.Drawing.Size(500, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
    }
}
