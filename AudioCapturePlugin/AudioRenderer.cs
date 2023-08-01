using System;
using System.IO;
using System.Diagnostics;

public class AudioRenderer
{
    public bool WriteMP3 = true;


    #region Fields, Properties, and Inner Classes
    // constants for the wave file header
    private const int HEADER_SIZE = 44;
    private const short BITS_PER_SAMPLE = 16;
    private const int SAMPLE_RATE = 48000;//44100;

    // the number of audio channels in the output file
    private int channels = 2;

    // the audio stream instance
    private MemoryStream outputStream;
    private BinaryWriter outputWriter;

    // should this object be rendering to the output stream?
    private bool Rendering = false;

    /// The status of a render
    public enum Status
    {
        UNKNOWN,
        SUCCESS,
        FAIL,
        ASYNC
    }

    /// The result of a render.
    public class Result
    {
        public Status State;
        public string Message;

        public Result(Status newState = Status.UNKNOWN, string newMessage = "")
        {
            this.State = newState;
            this.Message = newMessage;
        }
    }
    #endregion

    public AudioRenderer()
    {
        this.Clear();
    }


	/*private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Insert))
		{
            //Debug.WriteLine(Application.persistentDataPath);
            if (!Rendering)
			{
                StartRecording();
            }
			else
            {
                FinishRecording();
            }
		}
	}*/

    public void StartRecording()
	{
        Debug.WriteLine("Starting to record...");
        this.Clear();
        Rendering = true;
	}

    /*private void FinishRecording()
	{
        //todo: open 'save file dialog', with the default filename generated below
        Debug.WriteLine("Finished recording, attempting to save file...");
        Rendering = false;

        int attempts = 500;
        for(int i = 0; i < attempts; i++)
		{
            string filename = Application.persistentDataPath + "/mg_recording_" + (i + 1);// + ".wav";
            //string filename = Application.dataPath + "/../AudioRecordings/" + "audiorec_test"; //todo: find unique new filename
            filename += WriteMP3 ? ".mp3" : ".wav";

            if (!File.Exists(filename))
			{
                if (WriteMP3)
				{
                    SaveMP3(filename);
                }
                else
				{
                    Save(filename);
                }

                return;
            }
        }
        Debug.WriteLine("Error: Could not write audio file because it took too many attempts to find a unique filename.");
    }*/

	// reset the renderer
	public void Clear()
    {
        this.outputStream = new MemoryStream();
        this.outputWriter = new BinaryWriter(outputStream);
    }

    /// Write a chunk of data to the output stream.
    public void Write(float[] audioData)
    {
        // Convert numeric audio data to bytes
        for (int i = 0; i < audioData.Length; i++)
        {
            // write the short to the stream
            this.outputWriter.Write((short)(audioData[i] * (float)Int16.MaxValue));
        }
    }

    // write the incoming audio to the output string
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (this.Rendering)
        {
            // store the number of channels we are rendering
            this.channels = channels;

            // store the data stream
            this.Write(data);
        }

    }

    public void SaveMP3(string filename)
	{
        if (outputStream.Length > 0)
		{
            EncodeMP3.WriteMP3(filename, outputStream.ToArray(), BITS_PER_SAMPLE);
		}
		else
		{
            Debug.WriteLine("There is no audio data to save!");
        }
	}

    
    #region File I/O
    public AudioRenderer.Result Save(string filename)
    {
        Result result = new AudioRenderer.Result();

        if (outputStream.Length > 0)
        {
            // add a header to the file so we can send it to the SoundPlayer
            this.AddHeader();

            // if a filename was passed in
            if (filename.Length > 0)
            {
                // Save to a file. Print a warning if overwriting a file.
                if (File.Exists(filename))
                    Debug.WriteLine("Overwriting " + filename + "...");

                // reset the stream pointer to the beginning of the stream
                outputStream.Position = 0;

                // write the stream to a file
                FileStream fs = File.OpenWrite(filename);

                this.outputStream.WriteTo(fs);

                fs.Close();

                // for debugging only
                Debug.WriteLine("Finished saving to WAV file at: " + filename + ".");
            }

            result.State = Status.SUCCESS;
        }
        else
        {
            Debug.WriteLine("There is no audio data to save!");

            result.State = Status.FAIL;
            result.Message = "There is no audio data to save!";
        }

        return result;
    }

    /// This generates a simple header for a canonical wave file, 
    /// which is the simplest practical audio file format. It
    /// writes the header and the audio file to a new stream, then
    /// moves the reference to that stream.
    /// 
    /// See this page for details on canonical wave files: 
    /// http://www.lightlink.com/tjweber/StripWav/Canon.html
    private void AddHeader()
    {
        // reset the output stream
        outputStream.Position = 0;

        // calculate the number of samples in the data chunk
        long numberOfSamples = outputStream.Length / (BITS_PER_SAMPLE / 8);

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
        this.outputStream.WriteTo(newOutputStream);

        // move the reference to the new stream
        this.outputStream = newOutputStream;
    }
    #endregion

    
}