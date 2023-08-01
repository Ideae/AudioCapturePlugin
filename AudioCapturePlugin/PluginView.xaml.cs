using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioCapturePlugin
{
	/// <summary>
	/// Interaction logic for PluginView.xaml
	/// </summary>
	public partial class PluginView : UserControl
	{
		public PluginView()
		{
			InitializeComponent();
		}

		private void SaveFile_Button_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == true)
			{
				int fileDurationSeconds = 0;
				if (!int.TryParse(Duration_TextBox.Text, out fileDurationSeconds))
				{
					// todo: make popup box to report error
					Debug.WriteLine("ZZZ: Error: The duration textbox did not contain a valid integer value.");
					return;
				}
				Debug.WriteLine("ZZZ: Filename: " + dlg.FileName + "   File Duration: " + Duration_TextBox.Text);
				//string directory = "";
				//string path = directory + "/" + dlg.FileName; // todo: check if the middle slash is necessary
				AudioCapturePlugin.WriteAudioFile(dlg.FileName, fileDurationSeconds);
			}
		}
    }
}
