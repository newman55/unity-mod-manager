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

namespace UnityModManagerNet.Installer
{
    public partial class SetFolder : Form
    {
        public SetFolder()
        {
            InitializeComponent();
        }

        public SetFolder(string path)
        {
            InitializeComponent();
            textBox1.Text = path;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
