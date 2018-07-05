using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NAudio.Wave;

using OxyPlot;
using OxyPlot.Series;

using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
//MathNet.Numerics.IntegralTransforms.Transform.FourierForward(samples);

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        #region FFT関係
        // 波形サイズ
        int size = 4096;
        // サンプリング周波数
        int fs = 4000*10;
        // DFT用データ
        double[] re;
        double[] im;
        // FFT用データ
        double[] reFFT;
        double[] imFFT;
        double[] reFFT_2;
        double[] imFFT_2;
        // 正弦波生成
        public double Sin(int freq, int i)
        {

            double sin = 0.2 * Math.Sin(2 * Math.PI / fs * freq * i);
            return sin;
        }

        // DFT
        public void DFT(int n, double[] inputRe, out double[] real, out double[] imag)
        {
            real = new double[n];
            imag = new double[n];

            for (int i = 0; i < n; i++)
            {
                real[i] = 0.0;
                imag[i] = 0.0;

                for (int j = 0; j < n; j++)
                {
                    real[i] += inputRe[j] * Math.Cos(2 * Math.PI * i * j / fs);
                    imag[i] += inputRe[j] * -(Math.Sin(2 * Math.PI * i * j / fs));
                }
            }
        }

        // FFT
        public static void FFT(double[] inputRe, double[] inputIm, out double[] outputRe, out double[] outputIm, int bitSize)
        {
            int dataSize = 1 << bitSize;
            int[] reverseBitArray = BitScrollArray(dataSize);

            outputRe = new double[dataSize];
            outputIm = new double[dataSize];

            // バタフライ演算のための置き換え
            for (int i = 0; i < dataSize; i++)
            {
                outputRe[i] = inputRe[reverseBitArray[i]];
                outputIm[i] = inputIm[reverseBitArray[i]];
            }

            // バタフライ演算
            for (int stage = 1; stage <= bitSize; stage++)
            {
                int butterflyDistance = 1 << stage;
                int numType = butterflyDistance >> 1;
                int butterflySize = butterflyDistance >> 1;

                double wRe = 1.0;
                double wIm = 0.0;
                double uRe = System.Math.Cos(System.Math.PI / butterflySize);
                double uIm = -System.Math.Sin(System.Math.PI / butterflySize);

                for (int type = 0; type < numType; type++)
                {
                    for (int j = type; j < dataSize; j += butterflyDistance)
                    {
                        int jp = j + butterflySize;
                        double tempRe = outputRe[jp] * wRe - outputIm[jp] * wIm;
                        double tempIm = outputRe[jp] * wIm + outputIm[jp] * wRe;
                        outputRe[jp] = outputRe[j] - tempRe;
                        outputIm[jp] = outputIm[j] - tempIm;
                        outputRe[j] += tempRe;
                        outputIm[j] += tempIm;
                    }
                    double tempWRe = wRe * uRe - wIm * uIm;
                    double tempWIm = wRe * uIm + wIm * uRe;
                    wRe = tempWRe;
                    wIm = tempWIm;
                }
            }
        }
        // ビットを左右反転した配列を返す
        private static int[] BitScrollArray(int arraySize)
        {
            int[] reBitArray = new int[arraySize];
            int arraySizeHarf = arraySize >> 1;

            reBitArray[0] = 0;
            for (int i = 1; i < arraySize; i <<= 1)
            {
                for (int j = 0; j < i; j++)
                    reBitArray[j + i] = reBitArray[j] + arraySizeHarf;
                arraySizeHarf >>= 1;
            }
            return reBitArray;
        }


        #endregion
         
        
        public MainWindow()
        {
            InitializeComponent();
           

            lin2axes.Maximum =40;

            #region debug
            //IList<DataPoint> data = new List<DataPoint>
            //{
            //    new DataPoint(0, 1),
            //    new DataPoint(1, 5),
            //    new DataPoint(2, 3),
            //};
            //line1.ItemsSource = data;
            #endregion

            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                var deviceInfo = WaveIn.GetCapabilities(i);
                Console.WriteLine(String.Format("Device {0}: {1}, {2} channels",
                    i, deviceInfo.ProductName, deviceInfo.Channels));
            }


            WaveIn waveIn = new WaveIn()
            {
                DeviceNumber = 0, // Default
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.WaveFormat = new WaveFormat(sampleRate: fs, channels: 1);
            waveIn.StartRecording();


        }


        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // 32bitで最大値1.0fにする
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);

                float sample32 = sample / 32768f;
                ProcessSample(sample32);
            }
        }

        List<double> _recorded = new List<double>(); // 音声データ
        public LineSeries _lineSeries = new LineSeries();


        

        private void ProcessSample(float sample)
        {
            var windowsize = size;
            _recorded.Add(sample);
            if (_recorded.Count == windowsize)
            {
                #region add
                // DFT用データ
                double[] dftIn = new double[size];
                double[] dftInIm = new double[size];
                DataPoint[] DftIn = new DataPoint[size];
                DataPoint[] DFTResult = new DataPoint[size];
                DataPoint[] FFTResult = new DataPoint[size];
                // 窓関数後データ
                double[] data = new double[size];
                // 波形生成
                for (int i = 0; i < size; i++)
                {
                    dftInIm[i] = 0.0;
                }
                var window = MathNet.Numerics.Window.Hamming(windowsize);
                _recorded = _recorded.Select((v, i) => (double)v * window[i]).ToList();
                dftIn = _recorded.Take(size).ToArray();


                // DFT
                //DFT(size, dftIn, out re, out im);
                // FFT
                FFT(dftIn, dftInIm, out reFFT, out imFFT, (int)Math.Log(size, 2));
                // DFT波形表示
                for (int i = 0; i < size / 2; i++)
                {
                    if (i > 0)
                    {
                        //FFTの横軸の周波数の単位、分解能についてはここを見る
                        //URL　http://detail.chiebukuro.yahoo.co.jp/qa/question_detail/q1350561093
                        //FFTの分解能（ひと間隔）はfs/sizeでももとまる。また、横軸の最大値は、サンプリング周波数の半分（サンプリング定理）となる。

                        //DFTResult[i] = new DataPoint((double)i, Math.Sqrt(re[i] * re[i] + im[i] * im[i]));
                        float a = ((float)fs / size);
                        float x = (float)i * a;
                        double y = Math.Sqrt(reFFT[i] * reFFT[i] + imFFT[i] * imFFT[i]);
                        FFTResult[i] = new DataPoint(x, y);
                    }
                }

                //line1.ItemsSource = dftIn;
                line2.ItemsSource = FFTResult.Take((FFTResult.Count() / 20));
                

                //line1.ItemsSource = DFTResult;
                #endregion


                //var points = _recorded.Select((v, index) =>
                //        new DataPoint((double)index, v)
                //    ).ToList();

                //var window = MathNet.Numerics.Window.Hamming(windowsize);
                //_recorded = _recorded.Select((v, i) => (double)v * window[i]).ToList();
                //double[] Fourierdata = _recorded.Select(v => (v)).ToArray();
                //Fourier.ForwardReal(Fourierdata, Fourierdata.Length - 2, FourierOptions.Matlab); ;
                //var s = windowsize * (1.0 / 8000.0);
                ////var point = Fourierdata.Take(Fourierdata.Count()).Select((v, index) =>
                ////       new DataPoint((double)index, v)
                ////).ToList();
                //var point = Fourierdata.Take(Fourierdata.Count() / 2).Select((v, index) =>new DataPoint((double)index / s,v)).ToList();

                //_lineSeries.Points.Clear();
                //_lineSeries.Points.AddRange(points);
                //line1.ItemsSource = points;
                //line2.ItemsSource = point;
                _recorded.Clear();
            }
        }


    }
}
