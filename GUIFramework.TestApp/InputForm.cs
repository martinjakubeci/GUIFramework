using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIFramework.TestApp
{
    public partial class InputForm : Form, IDialog<string>
    {
        public InputForm()
        {
            InitializeComponent();
        }

        public event Action<string> Finished;

        private void button1_Click(object sender, EventArgs e)
        {
            Finished?.Invoke(textBox1.Text);
            Close();
        }

        void IDialog<string>.ShowDialog()
        {
            Show();
        }
    }
}
