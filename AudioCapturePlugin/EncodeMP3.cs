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
	public static void WriteMP3(string path, byte[] wavFile, int bitRate)
	{
		Debug.WriteLine("Attempting to encode mp3 file: " + path);
		var bytesMP3 = ConvertWavToMp3(wavFile, bitRate);
		//Debug.WriteLine("Attempting to write all file bytes...");
		File.WriteAllBytes(path, bytesMP3);
		//Debug.WriteLine("Finished saving to MP3 file at: " + path + ".");
	}

	public static byte[] ConvertWavToMp3 (byte[] wavFile, int bitRate)
	{
		var retMs = new MemoryStream ();
		var ms = new MemoryStream (wavFile);
		int bitDepth = 16;
		var waveFormat = new WaveFormat(44100, bitDepth, 2);
		var rdr = new RawSourceWaveStream (ms, waveFormat);
		//int bitRate = 128;
		var wtr = new LameMP3FileWriter (retMs, rdr.WaveFormat, bitRate);
		//var w = new LameMP3FileWriter()
		//LAMEPreset p = LAMEPreset.ABR_128;
		//var lc = new LameConfig();
		//wtr.PrintLAMEConfig();
		rdr.CopyTo (wtr);
		wtr.Flush();
		return retMs.ToArray ();
		//return new byte[] { };
	}
}