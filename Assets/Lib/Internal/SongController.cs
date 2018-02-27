using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using System.Numerics;
using DSPLib;


public class SongController : MonoBehaviour {

	float[] realTimeSpectrum;
	SpectralFluxAnalyzer realTimeSpectralFluxAnalyzer;
	PlotController realTimePlotController;

	int numChannels;
	int numTotalSamples;
	int sampleRate;
	float clipLength;
	float[] multiChannelSamples;
	SpectralFluxAnalyzer preProcessedSpectralFluxAnalyzer;
	PlotController preProcessedPlotController;

	AudioSource audioSource;

	public bool realTimeSamples = true;
	public bool preProcessSamples = false;

	void Start() {
		audioSource = GetComponent<AudioSource> ();

		// Process audio as it plays
		if (realTimeSamples) {
			realTimeSpectrum = new float[1024];
			realTimeSpectralFluxAnalyzer = new SpectralFluxAnalyzer ();
			realTimePlotController = GameObject.Find ("RealtimePlot").GetComponent<PlotController> ();

			this.sampleRate = AudioSettings.outputSampleRate;
		}

		// Preprocess entire audio file upfront
		if (preProcessSamples) {
			preProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer ();
			preProcessedPlotController = GameObject.Find ("PreprocessedPlot").GetComponent<PlotController> ();

			// Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
			// [L,R,L,R,L,R]
			multiChannelSamples = new float[audioSource.clip.samples * audioSource.clip.channels];
			numChannels = audioSource.clip.channels;
			numTotalSamples = audioSource.clip.samples;
			clipLength = audioSource.clip.length;

			// We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
			this.sampleRate = audioSource.clip.frequency;

			audioSource.clip.GetData(multiChannelSamples, 0);
			Debug.Log ("GetData done");

			Thread bgThread = new Thread (this.getFullSpectrumThreaded);

			Debug.Log ("Starting Background Thread");
			bgThread.Start ();
		}
	}

	void Update() {
		// Real-time
		if (realTimeSamples) {
			audioSource.GetSpectrumData (realTimeSpectrum, 0, FFTWindow.BlackmanHarris);
			realTimeSpectralFluxAnalyzer.analyzeSpectrum (realTimeSpectrum, audioSource.time);
			realTimePlotController.updatePlot (realTimeSpectralFluxAnalyzer.spectralFluxSamples);
		}

		// Preprocessed
		if (preProcessSamples) {
			int indexToPlot = getIndexFromTime (audioSource.time) / 1024;
			preProcessedPlotController.updatePlot (preProcessedSpectralFluxAnalyzer.spectralFluxSamples, indexToPlot);
		}
	}

	public int getIndexFromTime(float curTime) {
		float lengthPerSample = this.clipLength / (float)this.numTotalSamples;

		return Mathf.FloorToInt (curTime / lengthPerSample);
	}

	public float getTimeFromIndex(int index) {
		return ((1f / (float)this.sampleRate) * index);
	}

	public void getFullSpectrumThreaded() {
		try {
			// We only need to retain the samples for combined channels over the time domain
			float[] preProcessedSamples = new float[this.numTotalSamples];

			int numProcessed = 0;
			float combinedChannelAverage = 0f;
			for (int i = 0; i < multiChannelSamples.Length; i++) {
				combinedChannelAverage += multiChannelSamples [i];

				// Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
				if ((i + 1) % this.numChannels == 0) {
					preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
					numProcessed++;
					combinedChannelAverage = 0f;
				}
			}

			Debug.Log ("Combine Channels done");
			Debug.Log (preProcessedSamples.Length);

			// Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
			int spectrumSampleSize = 1024;
			int iterations = preProcessedSamples.Length / spectrumSampleSize;

			FFT fft = new FFT ();
			fft.Initialize ((UInt32)spectrumSampleSize);

			Debug.Log (string.Format("Processing {0} time domain samples for FFT", iterations));
			double[] sampleChunk = new double[spectrumSampleSize];
			for (int i = 0; i < iterations; i++) {
				// Grab the current 1024 chunk of audio sample data
				Array.Copy (preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

				// Apply our chosen FFT Window
				double[] windowCoefs = DSP.Window.Coefficients (DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
				double[] scaledSpectrumChunk = DSP.Math.Multiply (sampleChunk, windowCoefs);
				double scaleFactor = DSP.Window.ScaleFactor.Signal (windowCoefs);

				// Perform the FFT and convert output (complex numbers) to Magnitude
				Complex[] fftSpectrum = fft.Execute (scaledSpectrumChunk);
				double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude (fftSpectrum);
				scaledFFTSpectrum = DSP.Math.Multiply (scaledFFTSpectrum, scaleFactor);

				// These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
				float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

				// Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
				preProcessedSpectralFluxAnalyzer.analyzeSpectrum (Array.ConvertAll (scaledFFTSpectrum, x => (float)x), curSongTime);
			}

			Debug.Log ("Spectrum Analysis done");
			Debug.Log ("Background Thread Completed");
				
		} catch (Exception e) {
			// Catch exceptions here since the background thread won't always surface the exception to the main thread
			Debug.Log (e.ToString ());
		}
	}
}