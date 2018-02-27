using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectralFluxInfo {
	public float time;
	public float spectralFlux;
	public float threshold;
	public float prunedSpectralFlux;
	public bool isPeak;
}

public class SpectralFluxAnalyzer {
	int numSamples = 1024;

	// Sensitivity multiplier to scale the average threshold.
	// In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
	float thresholdMultiplier = 1.5f;

	// Number of samples to average in our window
	int thresholdWindowSize = 50;

	public List<SpectralFluxInfo> spectralFluxSamples;

	float[] curSpectrum;
	float[] prevSpectrum;

	int indexToProcess;

	public SpectralFluxAnalyzer () {
		spectralFluxSamples = new List<SpectralFluxInfo> ();

		// Start processing from middle of first window and increment by 1 from there
		indexToProcess = thresholdWindowSize / 2;

		curSpectrum = new float[numSamples];
		prevSpectrum = new float[numSamples];
	}

	public void setCurSpectrum(float[] spectrum) {
		curSpectrum.CopyTo (prevSpectrum, 0);
		spectrum.CopyTo (curSpectrum, 0);
	}
		
	public void analyzeSpectrum(float[] spectrum, float time) {
		// Set spectrum
		setCurSpectrum(spectrum);

		// Get current spectral flux from spectrum
		SpectralFluxInfo curInfo = new SpectralFluxInfo();
		curInfo.time = time;
		curInfo.spectralFlux = calculateRectifiedSpectralFlux ();
		spectralFluxSamples.Add (curInfo);

		// We have enough samples to detect a peak
		if (spectralFluxSamples.Count >= thresholdWindowSize) {
			// Get Flux threshold of time window surrounding index to process
			spectralFluxSamples[indexToProcess].threshold = getFluxThreshold (indexToProcess);

			// Only keep amp amount above threshold to allow peak filtering
			spectralFluxSamples[indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

			// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
			int indexToDetectPeak = indexToProcess - 1;

			bool curPeak = isPeak (indexToDetectPeak);

			if (curPeak) {
				spectralFluxSamples [indexToDetectPeak].isPeak = true;
			}
			indexToProcess++;
		}
		else {
			Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
		}
	}

	float calculateRectifiedSpectralFlux() {
		float sum = 0f;

		// Aggregate positive changes in spectrum data
		for (int i = 0; i < numSamples; i++) {
			sum += Mathf.Max (0f, curSpectrum [i] - prevSpectrum [i]);
		}
		return sum;
	}

	float getFluxThreshold(int spectralFluxIndex) {
		// How many samples in the past and future we include in our average
		int windowStartIndex = Mathf.Max (0, spectralFluxIndex - thresholdWindowSize / 2);
		int windowEndIndex = Mathf.Min (spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);
		
	    // Add up our spectral flux over the window
		float sum = 0f;
		for (int i = windowStartIndex; i < windowEndIndex; i++) {
			sum += spectralFluxSamples [i].spectralFlux;
		}

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);
		return avg * thresholdMultiplier;
	}

	float getPrunedSpectralFlux(int spectralFluxIndex) {
		return Mathf.Max (0f, spectralFluxSamples [spectralFluxIndex].spectralFlux - spectralFluxSamples [spectralFluxIndex].threshold);
	}

	bool isPeak(int spectralFluxIndex) {
		if (spectralFluxSamples [spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples [spectralFluxIndex + 1].prunedSpectralFlux &&
			spectralFluxSamples [spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples [spectralFluxIndex - 1].prunedSpectralFlux) {
			return true;
		} else {
			return false;
		}
	}

	void logSample(int indexToLog) {
		int windowStart = Mathf.Max (0, indexToLog - thresholdWindowSize / 2);
		int windowEnd = Mathf.Min (spectralFluxSamples.Count - 1, indexToLog + thresholdWindowSize / 2);
		Debug.Log (string.Format (
			"Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
			"Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.",
			spectralFluxSamples [indexToLog].time,
			spectralFluxSamples [indexToLog].prunedSpectralFlux,
			spectralFluxSamples [indexToLog].spectralFlux,
			spectralFluxSamples [indexToLog].threshold,
			spectralFluxSamples [windowStart].time,
			spectralFluxSamples [windowEnd].time,
			spectralFluxSamples [windowEnd].time - spectralFluxSamples [windowStart].time,
			windowEnd - windowStart
		));
	}
}