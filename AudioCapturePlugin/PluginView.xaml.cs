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
			int fileDuration = 0;
			if (!int.TryParse(Duration_TextBox.Text, out fileDuration))
			{
				string errorMessage = "Error: The duration textbox must contain a numeric integer value.";
				Debug.WriteLine("ZZZ: " + errorMessage);
				MessageBox.Show(errorMessage, "Error saving file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
				return;
			}
			SaveFileDialog dlg = new SaveFileDialog();
			Debug.WriteLine("ZZZ: Initial Directory: " + dlg.InitialDirectory);
			if (dlg.ShowDialog() == true)
			{
				// todo: propogate file extension into WriteAudioFile function
				string extension = "." + audioFileType.ToString().ToLower();
				Debug.WriteLine("ZZZ: Filename: " + dlg.FileName + extension + "   File Duration: " + Duration_TextBox.Text);
				int durationInSeconds = fileDuration;
				if (TimeUnits_ComboBox.Text == "Seconds")
				{
					durationInSeconds = fileDuration;
				}
				else if (TimeUnits_ComboBox.Text == "Minutes")
				{
					durationInSeconds = fileDuration * 60;
				}
				bool success = AudioCapturePlugin.WriteAudioFile(dlg.FileName, durationInSeconds);
				
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
