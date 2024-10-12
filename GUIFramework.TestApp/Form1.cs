using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIFramework.TestApp
{
    public partial class Form1 : Form, IDialog<int>
    {
        private int _i;

        public Form1(int i)
        {
            InitializeComponent();
            _i = i;
            Text = _i.ToString();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Finished?.Invoke(_i);
        }

        public event Action<int> Finished;

        public static MyTask<int> ShowForm1(int i)
        {
            return new MyTask<int>(new Form1(i));
        }

        public static MyTask<string> GetInput()
        {
            return new MyTask<string>(new InputForm());
        }

        void IDialog<int>.ShowDialog()
        {
            Show();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var input1 = await GetInput();
            var input2 = await GetInput();

            MessageBox.Show($"{input1}{Environment.NewLine}{input2}");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var state = new State() { /*Input1 = "111",*/ Input2 = "222" };

            state.Input1 = await GetInput().MapTo(state.Input1);
            state.Input2 = await GetInput().MapTo(state.Input2);

            MessageBox.Show($"{state.Input1}{Environment.NewLine}{state.Input2}");
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }
    }
}
