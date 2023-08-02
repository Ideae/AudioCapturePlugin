using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AudioCapturePlugin
{
	public static class AudioWriter
	{
		// todo: assuming for now the bitsPerSample is 16, so we will write a short to the stream writer
		public static void WriteMP3(string filename, double[] samples) //, int bitsPerSample)
		{
			MemoryStream outputStream = new MemoryStream();
			BinaryWriter outputWriter = new BinaryWriter(outputStream);

			// Convert numeric audio data to bytes
			for (int i = 0; i < samples.Length; i++)
			{
				// write the short to the stream
				outputWriter.Write((short)(samples[i] * (double)Int16.MaxValue));
			}

			int BITS_PER_SAMPLE = 16;
			EncodeMP3.WriteMP3(filename, outputStream.ToArray(), BITS_PER_SAMPLE);
		}

		public static void WriteWAV(string filename, double[] samples, int sampleRate) //, int bitsPerSample)
		{
			//Debug.WriteLine("ZZZ: WriteWAV 1");

			MemoryStream outputStream = new MemoryStream();
			BinaryWriter outputWriter = new BinaryWriter(outputStream);

			//Debug.WriteLine("ZZZ: WriteWAV 2");


			// Convert numeric audio data to bytes
			for (int i = 0; i < samples.Length; i++)
			{
				// write the short to the stream
				outputWriter.Write((short)(samples[i] * (double)Int16.MaxValue));
			}
			//int BITS_PER_SAMPLE = 16;

			string fullPath = filename + ".wav";
			Debug.WriteLine("Attempting to write wav file: " + fullPath);

			//File.WriteAllBytes(fullPath, outputStream.ToArray());

			FileStream fs = File.OpenWrite(fullPath);

			int HEADER_SIZE = 44;
			short BITS_PER_SAMPLE = 16;
			int channels = 1;
			AddHeader(fs, samples.Length, BITS_PER_SAMPLE, channels, HEADER_SIZE, sampleRate);

			outputStream.WriteTo(fs);
			fs.Close();

			Debug.WriteLine("Successfully wrote wav file: " + fullPath);
		}

		/// This generates a simple header for a canonical wave file, 
		/// which is the simplest practical audio file format. It
		/// writes the header and the audio file to a new stream, then
		/// moves the reference to that stream.
		/// 
		/// See this page for details on canonical wave files: 
		/// http://www.lightlink.com/tjweber/StripWav/Canon.html
		private static void AddHeader(FileStream fs, long numberOfSamples, short BITS_PER_SAMPLE, int channels, int HEADER_SIZE, int SAMPLE_RATE)
		{
			// reset the output stream
			//outputStream.Position = 0;

			// calculate the number of samples in the data chunk
			//long numberOfSamples = outputStream.Length / (BITS_PER_SAMPLE / 8);

			// create a new MemoryStream that will have both the audio data AND the header
			MemoryStream newOutputStream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(newOutputStream);

			writer.Write(0x46464952); // "RIFF" in ASCII

			// write the number of bytes in the entire file
			writer.Write((int)(HEADER_SIZE + (numberOfSamples * BITS_PER_SAMPLE * channels / 8)) - 8);

			writer.Write(0x45564157); // "WAVE" in ASCII
			writer.Write(0x20746d66); // "fmt " in ASCII
			writer.Write(16);

			// write the format tag. 1 = PCM
			writer.Write((short)1);

			// write the number of channels.
			writer.Write((short)channels);

			// write the sample rate. 44100 in this case. The number of audio samples per second
			writer.Write(SAMPLE_RATE);

			writer.Write(SAMPLE_RATE * channels * (BITS_PER_SAMPLE / 8));
			writer.Write((short)(channels * (BITS_PER_SAMPLE / 8)));

			// 16 bits per sample
			writer.Write(BITS_PER_SAMPLE);

			// "data" in ASCII. Start the data chunk.
			writer.Write(0x61746164);

			// write the number of bytes in the data portion
			writer.Write((int)(numberOfSamples * BITS_PER_SAMPLE * channels / 8));

			// copy over the actual audio data
			//this.outputStream.WriteTo(newOutputStream);
			// move the reference to the new stream
			//this.outputStream = newOutputStream;

			newOutputStream.WriteTo(fs);
		}
	}
}
