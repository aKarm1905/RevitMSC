using System.Windows.Forms;

namespace BuildingCoder
{
  public partial class CmdWindowHandleForm : Form
  {
    public CmdWindowHandleForm()
    {
      InitializeComponent();
    }

    public string LabelText
    {
      get
      {
        return label1.Text;
      }
      set
      {
        label1.Text = value;
      }
    }
  }
}
