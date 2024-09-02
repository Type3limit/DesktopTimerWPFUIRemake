using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Complex = NAudio.Dsp.Complex;

namespace DesktopTimer.Views.Controls
{
    public class AudioWaveDataModel : ObservableObject
    {

        private double dataWeight = 0.5;
        public double DataWeight
        {
            get => dataWeight;
            set => SetProperty(ref dataWeight, value);
        }

        public WasapiLoopbackCapture cap = new WasapiLoopbackCapture();

        public delegate void RecoredStartedHandler();
        public event RecoredStartedHandler? WaveRecordsStarted;


        public delegate void WaveDataChangedHandler(float[] samples);
        public event WaveDataChangedHandler? WaveDataChanged;

        private CancellationTokenSource? token = null;

        private List<float[]> WaveDatas = new List<float[]>();

        public AudioWaveDataModel()
        {
            cap.DataAvailable += WaveDataIn;
        }


        List<float> allSamples = new List<float>();
        List<List<float>> channelSamples = new List<List<float>>();
        List<float> samples = new List<float>();
        List<float> dftData = new List<float>();

        List<float> lastData = new List<float>();

        private void WaveDataIn(object? sender, WaveInEventArgs e)
        {

            allSamples = Enumerable.Range(0, e.BytesRecorded / 4).Select(i => BitConverter.ToSingle(e.Buffer, i * 4)).ToList();    
                                                                                                                                  
            int channelCount = cap.WaveFormat.Channels;   

            channelSamples = Enumerable
              .Range(0, channelCount)
              .Select(channel => Enumerable
                  .Range(0, allSamples.Count / channelCount)
                  .Select(i => allSamples[channel + i * channelCount])
                  .ToList())
              .ToList();

            samples = Enumerable.Range(0, allSamples.Count / channelCount)
                .Select(index => Enumerable
                .Range(0, channelCount)
                .Select(channel => channelSamples[channel][index])
                .Average())
                .ToList();


            float log = (float)Math.Ceiling(Math.Log(samples.Count, 2));
            int newLen = (int)Math.Pow(2, log);
            float[] filledSamples = new float[newLen];
            Array.Copy(samples.ToArray(), filledSamples, samples.Count);
            Complex[] complexSrc = filledSamples
              .Select(v => new Complex() { X = v })
              .ToArray();
            FastFourierTransform.FFT(false, (int)log, complexSrc);
            dftData = complexSrc.Take(complexSrc.Length / 2).Select(v => (float)Math.Sqrt(v.X * v.X + v.Y * v.Y)).ToList();   // 一半的数据
            int count = dftData.Count / (cap.WaveFormat.SampleRate / (filledSamples.Length == 0 ? 1 : filledSamples.Length));
            var TakeData = dftData.Take(count).ToList();
            var FinalResult = new List<float>(TakeData);
            if (lastData.Count > 0)
            {
                for (int i = 0; i < TakeData.Count && i < lastData.Count; i++)
                {
                    FinalResult[i] = TakeData[i] - (TakeData[i] - lastData[i]) * (float)DataWeight;
                }
            }
            WaveDatas.Add(FinalResult.ToArray());
            lastData = FinalResult;
            StartWaveDataInvokeThread();
        }

        private void StartWaveDataInvokeThread()
        {
            if (token != null)
            {
                return;
            }
            token = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    while (WaveDatas.Count > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            WaveDatas.Clear();
                            break;

                        }
                        var curValue = WaveDatas.FirstOrDefault();
                        WaveDataChanged?.Invoke(curValue ?? new float[0]);
                        WaveDatas.Remove(curValue ?? new float[0]);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    token = null;
                }

            }, token.Token);
        }

        public bool StartRecord()
        {
            try
            {
                WaveRecordsStarted?.Invoke();
                cap.StartRecording();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        public bool StopRecord()
        {
            try
            {
                cap.StopRecording();
                WaveDataChanged?.Invoke(new float[0]);
                //token?.Cancel();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }
    }
}
