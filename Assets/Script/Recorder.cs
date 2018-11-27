using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;

public class Recorder : MonoBehaviour {

    private int bufferSize;
    private int numBuffers;
    private int outputRate = 44100;
    private String fileName = "recTest.wav";
    private int headerSize = 44; //default for uncompressed wav
    private Boolean recOutput = false;
    private FileStream fileStream;

    public Canvas canvasSaveAudioFile;
    public Text txtAudioFileName;

    [Header("Record Button")]
    public Color btnNormalColor = new Color32(77, 77, 77, 255);
    public Color btnSelectedColor = new Color32(190, 48, 48, 255);
    public GameObject btnRecord;


    private void Awake()
    {
        AudioSettings.outputSampleRate = outputRate;
    }

    private void Start()
    {
        AudioSettings.GetDSPBufferSize(out bufferSize,out numBuffers);
    }

    public void StartStopRecording()
    {
        if (recOutput == false)
        {
            StartWriting(fileName);
            recOutput = true;
            btnRecord.GetComponent<Image>().color = btnSelectedColor;
        }else{
            canvasSaveAudioFile.enabled = true;
            recOutput = false;
            btnRecord.GetComponent<Image>().color = btnNormalColor;
            WriteHeader();
        }
    }

    public void SaveAudioFile()
    {
        string fileName = txtAudioFileName.text;
        if(fileName.Trim().Equals("") == false){
            RenameFile(fileName.Trim() + ".wav");
            canvasSaveAudioFile.enabled = false;
        }
    }

    private void StartWriting(String name)
    {
        fileStream = new FileStream(Path.Combine(Application.persistentDataPath, name), FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < headerSize; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }
    }

    private void OnAudioFilterRead(float[] data,int channels)
    {
        if (recOutput)
        {
            ConvertAndWrite(data); //audio data is interlaced
        }
    }

    private void ConvertAndWrite(float[] dataSource)
    {
        Int16[] intData = new Int16[dataSource.Length];
        Byte[] bytesData = new Byte[dataSource.Length * 2];

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < dataSource.Length; i++)
        {
            intData[i] = (short)(dataSource[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader()
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);
        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);
        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);
        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);
        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);
        Byte[] numChannels = BitConverter.GetBytes(two);
        fileStream.Write(numChannels, 0, 2);
        Byte[] sampleRate = BitConverter.GetBytes(outputRate);
        fileStream.Write(sampleRate, 0, 4);
        Byte[] byteRate = BitConverter.GetBytes(outputRate * 4);
        // sampleRate * bytesPerSample*number of channels, here 44100*2*2

        fileStream.Write(byteRate, 0, 4);

        UInt16 four = 4;
        Byte[] blockAlign = BitConverter.GetBytes(four);
        fileStream.Write(blockAlign, 0, 2);
        UInt16 sixteen = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
        fileStream.Write(bitsPerSample, 0, 2);
        Byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);
        Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
        fileStream.Write(subChunk2, 0, 4);

        fileStream.Close();
    }

    private void RenameFile(String newName){
        String file_path = Path.Combine(Application.persistentDataPath, newName);
        if (File.Exists(file_path))
        {
            File.Delete(file_path);
        }
        Debug.Log(Path.Combine(Application.persistentDataPath, fileName));
        File.Move(Path.Combine(Application.persistentDataPath, fileName), file_path);
    }
}
