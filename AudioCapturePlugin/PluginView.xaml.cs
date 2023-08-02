using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioCapturePlugin
{
	public enum AudioFileTypes
	{
		WAV,
		MP3
	}
	/// <summary>
	/// Interaction logic for PluginView.xaml
	/// </summary>
	public partial class PluginView : UserControl
	{
		AudioFileTypes audioFileType = AudioFileTypes.WAV;
		public PluginView()
		{
			InitializeComponent();
		}

		private void SaveFile_Button_Click(object sender, RoutedEventArgs e)
		{
			int fileDurationSeconds = 0;
			if (!int.TryParse(Duration_TextBox.Text, out fileDurationSeconds))
			{
				string errorMessage = "Error: The duration textbox did not contain a valid integer value.";
				Debug.WriteLine("ZZZ: " + errorMessage);
				MessageBox.Show(errorMessage, "Error saving file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
				return;
			}
			SaveFileDialog dlg = new SaveFileDialog();
			if (dlg.ShowDialog() == true)
			{
				string extension = "." + audioFileType.ToString().ToLower();
				Debug.WriteLine("ZZZ: Filename: " + dlg.FileName + extension + "   File Duration: " + Duration_TextBox.Text);
				bool success = AudioCapturePlugin.WriteAudioFile(dlg.FileName, fileDurationSeconds);
				
				string popupCaption = "Success";
				string popupMessage = "Successfully wrote file: " + dlg.FileName + extension;
				var popupImage = MessageBoxImage.None;
				if (!success)
				{
					popupCaption = "Error: Failed to write audio file.";
					popupMessage = "Error writing file.";
					popupImage = MessageBoxImage.Error;

				}
				MessageBox.Show(popupMessage, popupCaption, MessageBoxButton.OK, popupImage, MessageBoxResult.OK, MessageBoxOptions.None);
			}
		}
    }
}
