using VidyoClient;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VidyoConnector.ViewModel
{
    class WaveConstants
    {
        public const int NumChannelsB = 22;
        public const int NumChannelsE = 24;
        public const int SampleRateB = 24;
        public const int SampleRateE = 28;
        public const int ByteRateB = 28;
        public const int ByteRateE = 32;
        public const int BitsPerSampleB = 34;
        public const int BitsPerSampleE = 35;
        public const int DataChunk = 44;

        public const int bitsPerByte = 8;
        public const int packetInterval = 20;
        public const int millisecondsInSeconds = 1000;
    }

    class WaveFile
    {
        public string wavFilename { get; set; }
        public int sampleRate { get; set; }
        public int numChannels { get; set; }
        public int byteRate { get; set; }
        public int numberOfFramesPerSecond { get; set; }
        public int bytesPerSample { get; set; }
        public int numberOfBytesPerFrame { get; set; }
        public int samplesPerFrame { get; set; }
        public int numberBytesPerAudioChunk { get; set; }
        public int numberOfAudioChunks { get; set; }
        public byte[] audioDataConverted { get; set; }
        public byte[] frame { get; set; }
        public ulong elapsedTime { get; set; }

        public byte[] GetFileByteArray()
        {
            byte[] byteArray;
            using (FileStream wavInfo = new FileStream(wavFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byteArray = new byte[wavInfo.Length];
                wavInfo.Read(byteArray, 0, (int)byteArray.Length);
            }
            return byteArray;
        }

        public byte[] GetInfo(byte[] data, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex + 1;
            byte[] bytesArray = new byte[length];
            Array.Copy(data, startIndex, bytesArray, 0, length);
            return bytesArray;
        }

        public byte[] GetAudioData(byte[] byteData, int startIndexOfDataChunk)
        {
            byte[] audioData = new byte[byteData.Length - startIndexOfDataChunk];
            Array.Copy(byteData, startIndexOfDataChunk, audioData, 0, byteData.Length - startIndexOfDataChunk);
            return audioData;
        }

        public void WaveFileGetInfo()
        {
            byte[] byteArray = GetFileByteArray();
            byte[] rawAudioData = GetAudioData(byteArray, WaveConstants.DataChunk);

            byteRate = BitConverter.ToInt32(GetInfo(byteArray, WaveConstants.ByteRateB, WaveConstants.ByteRateE), 0);
            numChannels = BitConverter.ToUInt16(GetInfo(byteArray, WaveConstants.NumChannelsB, WaveConstants.NumChannelsE), 0);
            bytesPerSample = (BitConverter.ToUInt16(GetInfo(byteArray, WaveConstants.BitsPerSampleB, WaveConstants.BitsPerSampleE), 0) / WaveConstants.bitsPerByte);
            sampleRate = BitConverter.ToInt32(GetInfo(byteArray, WaveConstants.SampleRateB, WaveConstants.SampleRateE), 0);
            samplesPerFrame = WaveConstants.packetInterval * sampleRate * numChannels / WaveConstants.millisecondsInSeconds;
            numberOfFramesPerSecond = sampleRate / samplesPerFrame;
            numberOfBytesPerFrame = byteRate / numberOfFramesPerSecond;
            numberOfAudioChunks = rawAudioData.Length / numberOfBytesPerFrame;
            numberBytesPerAudioChunk = numberOfBytesPerFrame * bytesPerSample;

            Int16[] asInt16 = new Int16[rawAudioData.Length/sizeof(Int16)];
            Buffer.BlockCopy(rawAudioData, 0, asInt16, 0, rawAudioData.Length);
            float[] convertedDataFloat = Array.ConvertAll(asInt16, e => e * (float)(1.0 / (float)(short.MaxValue + 1)));
            audioDataConverted = new byte[convertedDataFloat.Length * sizeof(float)];
            Buffer.BlockCopy(convertedDataFloat, 0, audioDataConverted, 0, audioDataConverted.Length);

            frame = new byte[numberBytesPerAudioChunk];
        }

        public WaveFile(string wavFilename)
        {
            this.wavFilename = wavFilename;

            Task.Run(() => {
                WaveFileGetInfo();
            });
        }        
    }

    class VidyoVirtualDeviceViewModel : VidyoConnectorViewModel
    {
        private object DataContext = null;

        WaveFile waveFile;

        private Task sendFrames { get; set; }

        private CancellationTokenSource token { get; set; }

        public delegate void NotifyFeedingStateChanged(string message);

        private NotifyFeedingStateChanged notifyStateChanged { get; set; }
               
        public VidyoVirtualDeviceViewModel(object DataContext, string wavFilename, NotifyFeedingStateChanged notifyCallback)
        {
            this.DataContext = DataContext;            
            this.notifyStateChanged = notifyCallback;

            this.waveFile = new WaveFile(wavFilename);
            
        }

        public bool StartAudioFeeding()
        {
            token = new CancellationTokenSource();

            var source = ((VidyoConnectorViewModel)DataContext).VirtualAudioSources.FirstOrDefault(x => ((x.IsStreamingAudio || x.IsSharingContent) && x.Id != null));
            if (source == null || source.Object == null)
                return false;

            sendFrames = Task.Run(() =>
            {
                notifyStateChanged("Stop feeding");

                waveFile.elapsedTime = 0;

                int generalSize = waveFile.numberOfAudioChunks * waveFile.numberBytesPerAudioChunk;
                for (int i = 0; i < generalSize; i += waveFile.numberBytesPerAudioChunk)
                {
                    Buffer.BlockCopy(waveFile.audioDataConverted, i, waveFile.frame, 0, waveFile.numberBytesPerAudioChunk);

                    if (token.IsCancellationRequested)
                        break;

                    GetConnectorInstance.SendVirtualAudioSourceFrameWithExternalData(source.Object, waveFile.frame, (SizeT)waveFile.samplesPerFrame, waveFile.elapsedTime);
                    waveFile.elapsedTime += WaveConstants.packetInterval;
                    Thread.Sleep(WaveConstants.packetInterval);
                }

                notifyStateChanged("Start feeding");
            }, token.Token);

            return sendFrames.Status == TaskStatus.Running;
        }

        public bool StopAudioFeeding()
        {
            if (sendFrames.Status == TaskStatus.Running)
            {
                token.Cancel();
                sendFrames.Wait();
            }

            token.Dispose();
            notifyStateChanged("Start feeding");
            return sendFrames.Status == TaskStatus.RanToCompletion;
        }

        public bool IsAudioFeeding()
        {
            if(sendFrames != null)
                return sendFrames.Status == TaskStatus.Running;
            return false;
        }

        public bool CreateVirtualAudioDevice(string name, string id)
        {
            return GetConnectorInstance.CreateVirtualAudioSource(name, id, "");
        }        
    }
}
