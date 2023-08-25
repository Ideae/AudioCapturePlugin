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
		AudioCapturePlugin audioCapturePlugin;
		public PluginView(AudioCapturePlugin audioCapturePlugin)
		{
			this.audioCapturePlugin = audioCapturePlugin;
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
			//string dateStr = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss");
			string dateStr = DateTime.Now.ToString("yyyy'-'MM'-'dd'__'HH'-'mm'-'ss");
			string defaultFileName = "AudioCapture__" + dateStr;
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = defaultFileName;
			Debug.WriteLine("ZZZ: Initial FileName: " + dlg.FileName);
			//Debug.WriteLine("ZZZ: Datestring: " + dateStr);
			if (dlg.ShowDialog() == true)
			{
				AudioFileTypes fileType = AudioFileTypes.MP3;
				if (FileType_ComboBox.Text == "MP3")
				{
					fileType = AudioFileTypes.MP3;
				}
				else if (FileType_ComboBox.Text == "WAV")
				{
					fileType = AudioFileTypes.WAV;
				}
				
				int durationInSeconds = fileDuration;
				if (TimeUnits_ComboBox.Text == "Seconds")
				{
					durationInSeconds = fileDuration;
				}
				else if (TimeUnits_ComboBox.Text == "Minutes")
				{
					durationInSeconds = fileDuration * 60;
				}
				bool success = audioCapturePlugin.WriteAudioFile(dlg.FileName, durationInSeconds, fileType);

				string extension = "." + fileType.ToString().ToLower();
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

		private void DecrementLeft_Button_Click(object sender, RoutedEventArgs e)
		{
			int fileDuration = 1;
			if (!int.TryParse(Duration_TextBox.Text, out fileDuration))
			{
				fileDuration = 1;
			}
			else
			{
				fileDuration = Math.Max(1, fileDuration - 1);
			}
			Duration_TextBox.Text = fileDuration.ToString();
		}

		private void IncrementRight_Button_Click(object sender, RoutedEventArgs e)
		{
			int fileDuration = 1;
			if (!int.TryParse(Duration_TextBox.Text, out fileDuration))
			{
				fileDuration = 1;
			}
			else
			{
				fileDuration++;
			}
			Duration_TextBox.Text = fileDuration.ToString();
		}
    }
}
