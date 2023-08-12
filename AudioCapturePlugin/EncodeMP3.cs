/*                    GNU GENERAL PUBLIC LICENSE
                       Version 3, 29 June 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

*/

/*---------------------- BeatUp (C) 2016-------------------- */


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using NAudio.Wave;
using NAudio.Lame;
using System.Diagnostics;

public static class EncodeMP3
{
	public static void convert (float[] samples, string path, int bitRate)
	{

		if (!path.EndsWith (".mp3"))
			path = path + ".mp3";
		
		ConvertAndWrite (samples, path, bitRate);

	}

	//  derived from Gregorio Zanon's script
	//private static void ConvertAndWrite (AudioClip clip, string path, int bitRate)
	private static void ConvertAndWrite(float[] samples, string path, int bitRate)
	{
		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		float rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i < samples.Length; i++) {
			intData [i] = (short)(samples [i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes (intData [i]);
			byteArr.CopyTo (bytesData, i * 2);
		}

		File.WriteAllBytes (path, ConvertWavToMp3 (bytesData,bitRate));
	}

	//added
	public static void WriteMP3(string path, byte[] wavFile, int bitDepth) //bitRate)
	{
		string fullPath = path + ".mp3";
		Debug.WriteLine("Attempting to encode mp3 file: " + fullPath);
		Debug.WriteLine("Debug log test 1...");
		/*TestCall();
		Debug.WriteLine("Debug log test 2...");
		var bytesTest = ConvertWavToMp3TEST(wavFile, bitDepth);
		Debug.WriteLine("Debug log test 3...");*/
		var bytesMP3 = ConvertWavToMp3(wavFile, bitDepth);
		Debug.WriteLine("Attempting to write all file bytes...");
		File.WriteAllBytes(fullPath, bytesMP3);
		Debug.WriteLine("Finished saving to MP3 file at: " + fullPath + ".");
	}

	/*public static void TestCall()
	{
		Debug.WriteLine("ZZZ: Debug log test...");
	}
	public static byte[] ConvertWavToMp3TEST(byte[] wavFile, int bitDepth) //bitRate)
	{
		Debug.WriteLine("ZZZ: ConvertWavToMp3TEST...");
		return new byte[] { };
	}*/

	public static byte[] ConvertWavToMp3 (byte[] wavFile, int bitDepth) //bitRate)
	{
		Debug.WriteLine("ZZZ: ConvertWavToMp3 1");
		var retMs = new MemoryStream ();
		Debug.WriteLine("ZZZ: ConvertWavToMp3 2");
		var ms = new MemoryStream (wavFile);
		Debug.WriteLine("ZZZ: ConvertWavToMp3 3");
		//var waveFormat = new WaveFormat(48000, bitDepth, 2);
		var waveFormat = new WaveFormat(44100, bitDepth, 2);
		Debug.WriteLine("ZZZ: ConvertWavToMp3 4");
		var rdr = new RawSourceWaveStream (ms, waveFormat);
		Debug.WriteLine("ZZZ: ConvertWavToMp3 5");
		var wtr = new LameMP3FileWriter (retMs, rdr.WaveFormat, bitDepth);
		Debug.WriteLine("ZZZ: ConvertWavToMp3 6");
		rdr.CopyTo (wtr);
		Debug.WriteLine("ZZZ: ConvertWavToMp3 7");
		wtr.Flush();
		Debug.WriteLine("ZZZ: ConvertWavToMp3 8");
		return retMs.ToArray ();
		//return new byte[] { };
	}
}