using System;
using System.Diagnostics;
using System.Windows.Controls;
using AudioPlugSharp;
using AudioPlugSharpWPF;


namespace AudioCapturePlugin
{
	public class AudioCapturePlugin : AudioPluginWPF
	{
		AudioIOPort stereoInput;
		AudioIOPort stereoOutput;

		public double[] samplesBufferLeft = null;
		public double[] samplesBufferRight = null;
		private int _bufferSizeInSeconds = 31 * 60;
		public int bufferSizeInSeconds { get { return _bufferSizeInSeconds; } }
		private int _currentBufferIndex = 0;

		public int sampleRate { get { return (int)Host.SampleRate; } }
		private const int channels = 2;

		public AudioCapturePlugin()
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
			EditorWidth = 300;
			EditorHeight = 160;

			// int sampRate = (int)Host.SampleRate; //todo: find out why host isn't initialized here or how to get around this (and why isn't Initialize being called before Process?)
			int sampRate = 44100; //todo: this should be based on the Host
			int bufferSizeInSamples = sampRate * _bufferSizeInSeconds;
			samplesBufferLeft = new double[bufferSizeInSamples]; // Big Buffer.
			samplesBufferRight = new double[bufferSizeInSamples]; // Big Buffer.
		}

		public override UserControl GetEditorView()
		{
			return new PluginView(this);
		}

		public override void Initialize()
		{
			base.Initialize();

			Debug.WriteLine("ZZZ: Calling Initialize");

			InputPorts = new AudioIOPort[] { stereoInput = new AudioIOPort("Stereo Input", EAudioChannelConfiguration.Stereo) };
			OutputPorts = new AudioIOPort[] { stereoOutput = new AudioIOPort("Stereo Output", EAudioChannelConfiguration.Stereo) };
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

			do
			{
				nextSample = Host.ProcessEvents();  // Handle sample-accurate parameters - see the SimpleExample plugin for a simpler, per-buffer parameter approach

				for (int i = currentSample; i < nextSample; i++)
				{
					outLeftSamples[i] = inSamplesLeft[i];
					outRightSamples[i] = inSamplesRight[i];

					// This saves the audio to the Big Buffer for writing to file later.
					// todo: only saving left channel now, find out if interleaving is necessary for two channels
					samplesBufferLeft[_currentBufferIndex] = outLeftSamples[i];
					samplesBufferRight[_currentBufferIndex] = outRightSamples[i];
					_currentBufferIndex++;
					if (_currentBufferIndex >= samplesBufferLeft.Length)
					{
						_currentBufferIndex = 0;
					}
				}
				currentSample = nextSample;
			}
			while (nextSample < inSamplesLeft.Length); // Continue looping until we hit the end of the buffer
		}
		public bool WriteAudioFile(string path, int durationInSeconds, AudioFileTypes fileType)
		{
			int durationSecs = durationInSeconds;
			if (durationSecs > bufferSizeInSeconds)
			{
				Debug.WriteLine("Warning: The maximum file length is " + bufferSizeInSeconds + ". Writing that length instead.");
				durationSecs = bufferSizeInSeconds;
			}
			int totalSamples = sampleRate * durationSecs;
			int firstSampleIndexInBuffer = nfmod((_currentBufferIndex - totalSamples), samplesBufferLeft.Length);

			int samplesBufferLength = samplesBufferLeft.Length;
			double[] samples = null;
			if (channels == 1)
			{
				samples = new double[totalSamples];
				for (int i = 0; i < samples.Length; i++)
				{
					int sampleIndex = nfmod((firstSampleIndexInBuffer + i), samplesBufferLength);
					double samp = samplesBufferLeft[sampleIndex];
					samples[i] = samp;
				}
			}
			else if (channels == 2)
			{
				// Interleave the left and right samples
				samples = new double[totalSamples * 2];
				for (int i = 0; i < totalSamples; i++)
				{
					int sampleIndex = nfmod((firstSampleIndexInBuffer + i), samplesBufferLength);
					double sampLeft = samplesBufferLeft[sampleIndex];
					samples[i * 2] = sampLeft;
					double sampRight = samplesBufferRight[sampleIndex];
					samples[i * 2 + 1] = sampRight;
				}
			}
			else
			{
				Debug.WriteLine("Error: Channel counts other than 1 or 2 are unsupported, but channels = " + channels);
				return false;
			}
			string extension = "." + fileType.ToString().ToLower();
			string fullPath = path + extension;
			Debug.WriteLine("Attempting to write file: " + fullPath + "   File Duration: " + durationInSeconds + " seconds");
			if (fileType == AudioFileTypes.WAV)
			{
				AudioWriter.WriteWAV(fullPath, samples, sampleRate, channels);
				Debug.WriteLine("Finished saving to WAV file at: " + path + ".");
			}
			else if (fileType == AudioFileTypes.MP3)
			{
				AudioWriter.WriteMP3(fullPath, samples);
				Debug.WriteLine("Finished saving to MP3 file at: " + path + ".");
			}
			return true;
		}
		public static int nfmod(int a, int b)
		{
			return a - b * (int)Math.Floor((double)a / b);
		}
	}
}
