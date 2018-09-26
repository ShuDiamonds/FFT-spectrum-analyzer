# FFT-spectrum-analyzer
This project file is FFT spectrum analyzer on `WPF application` using `NAudio`, `OxyPlot`, `Math Net`.
<p align="center"> 
<img  src="https://github.com/ShuDiamonds/FFT-spectrum-analyzer/blob/master/FFTspectrum.gif" width="480px"  title="FFT-spectrum-analyzer">
</p>

このプロジェクトは、FFTスペクトルアナライザーを `Windows WPFアプリケーション`で制作したものです。ライブラリには、音声信号処理に`NAudio`、グラフ描画に`OxyPlot`、数学系の計算に`Math Net` を使用しています。

## Description
All FFT processes in this C# program are written without other librarys. the sampling rate is 40kHz, the window function is hamming function. this [repository code](https://github.com/ShuDiamonds/FFT-spectrum-analyzer/blob/master/WpfApplication1/MainWindow.xaml.cs) contains FFT and DFT methods. please feel free to use them !

 C#でのFFTの処理には、ライブラリを使わず、自作で書き上げました。サンプリング周波数は40kHz、窓関数にはハミング窓を使用しています。
 [コード](https://github.com/ShuDiamonds/FFT-spectrum-analyzer/blob/master/WpfApplication1/MainWindow.xaml.cs)の中には、FFTとDFTのメソッドも入っています。  
 
 `WaveIn_DataAvailableメソッド`:NAudioでの音声信号の入力を処理
 
 `ProcessSampleメソッド`：窓間の処理やFFT,DFTの関連の処理を行う
 


## Requirement
* USBマイク（マイク入力がないと実行できません）
* .NETフレームワーク4.5 以上
 
## Usage
  FFT-spectrum-analyzer/WpfApplication1/bin/Release/WpfApplication1.exe を実行すると、上の図のようなアプリケーションが立ち上がります。

## Licence

  [MIT](https://github.com/tcnksm/tool/blob/master/LICENCE)

## Author

  [ShuDiamonds](https://github.com/ShuDiamonds)
