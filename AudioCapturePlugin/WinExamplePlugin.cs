using System;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;


namespace AudioCapturePlugin
{
	//public class SimpleExamplePlugin : AudioPluginBase
	public class WinExamplePlugin : AudioPluginWPF
	{
		AudioIOPort stereoInput;
		AudioIOPort stereoOutput;

		AudioPluginParameter gainParameter = null;
		AudioPluginParameter panParameter = null;
		AudioPluginParameter testParameter = null;

		public WinExamplePlugin()
		{
			Company = "My Company";
			Website = "www.mywebsite.com";
			Contact = "contact@my.email";
			PluginName = "Audio Capture Plugin";
			PluginCategory = "Fx";
			PluginVersion = "1.0.0";

			// Unique 64bit ID for the plugin
			PluginID = 0xDC2D469D4B28EE40;

			HasUserInterface = true;
			EditorWidth = 200;
			EditorHeight = 100;
		}

		public override UserControl GetEditorView()
		{
			return new PluginView(); // (this);
		}

		public override void Initialize()
		{
			base.Initialize();

			InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
			OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };

			AddParameter(gainParameter = new AudioPluginParameter
			{
				ID = "gain",
				Name = "Gain",
				Type = EAudioPluginParameterType.Float,
				MinValue = -20,
				MaxValue = 20,
				DefaultValue = 0,
				ValueFormat = "{0:0.0}dB"
			});

			AddParameter(panParameter = new AudioPluginParameter
			{
				ID = "pan",
				Name = "Pan",
				Type = EAudioPluginParameterType.Float,
				MinValue = -1,
				MaxValue = 1,
				DefaultValue = 0,
				ValueFormat = "{0:0.0}"
			});

			AddParameter(testParameter = new AudioPluginParameter
			{
				ID = "test",
				Name = "test",
				Type = EAudioPluginParameterType.Float,
				MinValue = -1,
				MaxValue = 1,
				DefaultValue = 0,
				ValueFormat = "{0:0.0}"
			});
		}

		public override void Process()
		{
			base.Process();

			ReadOnlySpan<double> inSamplesLeft = stereoInput.GetAudioBuffer(0);
			ReadOnlySpan<double> inSamplesRight = stereoInput.GetAudioBuffer(1);

			Span<double> outLeftSamples = stereoOutput.GetAudioBuffer(0);
			Span<double> outRightSamples = stereoOutput.GetAudioBuffer(1);

			int currentSample = 0;
			int nextSample = 0;

			double linearGain = Math.Pow(10.0, 0.05 * gainParameter.ProcessValue);
			double pan = panParameter.ProcessValue;

			do
			{
				nextSample = Host.ProcessEvents();  // Handle sample-accurate parameters - see the SimpleExample plugin for a simpler, per-buffer parameter approach

				bool needGainUpdate = gainParameter.NeedInterpolationUpdate;
				bool needPanUpdate = panParameter.NeedInterpolationUpdate;

				for (int i = currentSample; i < nextSample; i++)
				{
					if (needGainUpdate)
					{
						linearGain = Math.Pow(10.0, 0.05 * gainParameter.GetInterpolatedProcessValue(i));
					}

					if (needPanUpdate)
					{
						pan = panParameter.GetInterpolatedProcessValue(i);
					}

					outLeftSamples[i] = inSamplesLeft[i] * linearGain * (1 - pan);
					outRightSamples[i] = inSamplesRight[i] * linearGain * (1 + pan);
				}

				currentSample = nextSample;
			}
			while (nextSample < inSamplesLeft.Length); // Continue looping until we hit the end of the buffer
		}
	}
}
