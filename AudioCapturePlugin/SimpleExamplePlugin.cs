using System;
using System.Diagnostics;
using AudioPlugSharp;

namespace AudioCapturePlugin
{
	public class SimpleExamplePlugin : AudioPluginBase
	{
		public SimpleExamplePlugin()
		{
			Company = "My Company";
			Website = "www.mywebsite.com";
			Contact = "contact@my.email";
			PluginName = "Simple Gain Plugin";
			PluginCategory = "Fx";
			PluginVersion = "1.0.0";

			// Unique 64bit ID for the plugin
			PluginID = 0xF57703946AFC4EF8;
		}

		AudioIOPort stereoInput;
		AudioIOPort stereoOutput;

		public override void Initialize()
		{
			base.Initialize();

			InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
			OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };

			AddParameter(new AudioPluginParameter
			{
				ID = "gain",
				Name = "Gain",
				Type = EAudioPluginParameterType.Float,
				MinValue = -20,
				MaxValue = 20,
				DefaultValue = 0,
				ValueFormat = "{0:0.0}dB"
			});
		}

		public override void Process()
		{
			base.Process();
			// Debug.WriteLine("ZZZ: Running Process.");
			// This will trigger all Midi note events and parameter changes that happend during this process window
			// For sample-accurate tracking, see the WPFExample or MidiExample plugins
			Host.ProcessAllEvents();

			double gain = GetParameter("gain").ProcessValue;
			double linearGain = Math.Pow(10.0, 0.05 * gain);

			ReadOnlySpan<double> inSamplesLeft = stereoInput.GetAudioBuffer(0);
			ReadOnlySpan<double> inSamplesRight = stereoInput.GetAudioBuffer(1);
			Span<double> outSamplesLeft = stereoOutput.GetAudioBuffer(0);
			Span<double> outSamplesRight = stereoOutput.GetAudioBuffer(1);

			for (int i = 0; i < inSamplesLeft.Length; i++)
			{
				outSamplesLeft[i] = inSamplesLeft[i] * linearGain;
				outSamplesRight[i] = inSamplesRight[i] * linearGain;
			}
		}
	}
}
