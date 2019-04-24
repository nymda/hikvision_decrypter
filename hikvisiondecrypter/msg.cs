using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hikvisiondecrypter
{
    public partial class msg : Form
    {
        public msg(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }

        private void msg_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
