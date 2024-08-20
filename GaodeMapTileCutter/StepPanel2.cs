using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GaodeMapTileCutter
{
    public partial class StepPanel2 : UserControl
    {
        private string outputPath = "";
        public StepPanel2()
        {
            InitializeComponent();
            tbxOutputPath.LostFocus += (s, e) =>
            {
                if (tbxOutputPath.TextLength > 0 &&
                    Directory.Exists(tbxOutputPath.Text))
                {
                    outputPath = tbxOutputPath.Text;
                }
                else
                    outputPath = "";
            };
        }

        private void btnChooseOutputPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbxOutputPath.Text = folderBrowserDialog.SelectedPath;
                outputPath = folderBrowserDialog.SelectedPath;
            }
        }

        public string getOutputPath() => outputPath;
    }
}
