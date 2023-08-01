using System;
using System.Diagnostics;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;


namespace AudioCapturePlugin
{
	public class AudioCapturePlugin : AudioPluginWPF
	{
		private static AudioCapturePlugin _instance = null;
		public static AudioCapturePlugin Instance { get { return _instance; } }

		AudioIOPort stereoInput;
		AudioIOPort stereoOutput;

		/*AudioPluginParameter gainParameter = null;
		AudioPluginParameter panParameter = null;*/

		public static double[] samplesBuffer = null;
		private int _bufferSizeInSeconds = 10;
		public int bufferSizeInSeconds { get { return _bufferSizeInSeconds; } }
		//private int bufferSizeInSamples = 0;
		private int _currentBufferIndex = 0;

		public static int sampleRate { get { return (int)Instance.Host.SampleRate; } }

		public static void WriteAudioFile(string path, int durationInSeconds)
		{
			int durationSecs = durationInSeconds;
			if (durationSecs > Instance.bufferSizeInSeconds)
			{
				Debug.WriteLine("Warning: The maximum file length is " + Instance.bufferSizeInSeconds + ". Writing that length instead.");
				durationSecs = Instance.bufferSizeInSeconds;
			}
			int totalSamples = sampleRate * durationSecs;
			double[] samples = new double[totalSamples];

			int firstSampleIndexInBuffer = (Instance._currentBufferIndex - totalSamples) % samplesBuffer.Length; //todo: make sure this works with negative values

			for (int i = 0; i < samples.Length; i++)
			{
				int sampleIndex = (firstSampleIndexInBuffer + i) % samplesBuffer.Length;
				double samp = samplesBuffer[sampleIndex];
				samples[i] = samp;
			}

			AudioWriter.WriteWAV(path, samples, sampleRate);
			//AudioWriter.WriteMP3(path, samples);
		}

		public AudioCapturePlugin()
		{
			if (_instance != null)
			{
				Debug.WriteLine("Warning: AudioCapturePlugin _instance already had a value and the singleton is being overwritten. (Multiple instances of AudioCapturePlugin were created)");
			}
			_instance = this;

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

			// int sampRate = (int)Host.SampleRate; //todo: find out why host isn't initialized here or how to get around this (and why isn't Initialize being called before Process?)
			int sampRate = 44100; //todo: this should be based on the Host
			int bufferSizeInSamples = sampRate * _bufferSizeInSeconds;
			samplesBuffer = new double[bufferSizeInSamples]; // Big Buffer.
		}

		public override UserControl GetEditorView()
		{
			return new PluginView(); // (this);
		}

		public override void Initialize()
		{
			base.Initialize();

			Debug.WriteLine("ZZZ: Calling Initialize");

			InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
			OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };

			/*AddParameter(gainParameter = new AudioPluginParameter
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
			});*/
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

			/*double linearGain = Math.Pow(10.0, 0.05 * gainParameter.ProcessValue);
			double pan = panParameter.ProcessValue;*/

			do
			{
				nextSample = Host.ProcessEvents();  // Handle sample-accurate parameters - see the SimpleExample plugin for a simpler, per-buffer parameter approach

				/*bool needGainUpdate = gainParameter.NeedInterpolationUpdate;
				bool needPanUpdate = panParameter.NeedInterpolationUpdate;*/

				for (int i = currentSample; i < nextSample; i++)
				{
					/*if (needGainUpdate)
					{
						linearGain = Math.Pow(10.0, 0.05 * gainParameter.GetInterpolatedProcessValue(i));
					}
					if (needPanUpdate)
					{
						pan = panParameter.GetInterpolatedProcessValue(i);
					}
					outLeftSamples[i] = inSamplesLeft[i] * linearGain * (1 - pan);
					outRightSamples[i] = inSamplesRight[i] * linearGain * (1 + pan);*/

					outLeftSamples[i] = inSamplesLeft[i];
					outRightSamples[i] = inSamplesRight[i];

					// This saves the audio to the Big Buffer for writing to file later.
					// todo: only saving left channel now, find out if interleaving is necessary for two channels
					samplesBuffer[_currentBufferIndex++] = outLeftSamples[i];
					//samplesBuffer[_currentBufferIndex++] = outRightSamples[i];
					if (_currentBufferIndex >= samplesBuffer.Length)
					{
						_currentBufferIndex = 0;
					}
				}
				currentSample = nextSample;
			}
			while (nextSample < inSamplesLeft.Length); // Continue looping until we hit the end of the buffer
		}
	}
}
