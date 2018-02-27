using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotController : MonoBehaviour {

	public List<Transform> plotPoints;
	private Material highlightMaterial;
	public int displayWindowSize = 300;

	// Use this for initialization
	void Start () {
		plotPoints = new List<Transform> ();

		float localWidth = transform.Find("Point/BasePoint").localScale.x;
		// -n/2...0...n/2
		for (int i = 0; i < displayWindowSize; i++) {
			//Instantiate point
			Transform t = (Instantiate(Resources.Load("Point"), transform) as GameObject).transform;

			// Set position
			float pointX = (displayWindowSize / 2) * -1 * localWidth + i * localWidth;
			t.localPosition = new Vector3(pointX, t.localPosition.y, t.localPosition.z);

			plotPoints.Add (t);
		}
	}
		
	public void updatePlot(List<SpectralFluxInfo> pointInfo, int curIndex = -1) {
		if (plotPoints.Count < displayWindowSize - 1)
			return;

		int numPlotted = 0;
		int windowStart = 0;
		int windowEnd = 0;

		if (curIndex > 0) {
			windowStart = Mathf.Max (0, curIndex - displayWindowSize / 2);
			windowEnd = Mathf.Min (curIndex + displayWindowSize / 2, pointInfo.Count - 1);
		} else {
			windowStart = Mathf.Max (0, pointInfo.Count - displayWindowSize - 1);
			windowEnd = Mathf.Min (windowStart + displayWindowSize, pointInfo.Count);
		}

		for (int i = windowStart; i < windowEnd; i++) {
			int plotIndex = numPlotted;
			numPlotted++;

			Transform fluxPoint = plotPoints [plotIndex].Find ("FluxPoint");
			Transform threshPoint = plotPoints [plotIndex].Find ("ThreshPoint");
			Transform peakPoint = plotPoints [plotIndex].Find ("PeakPoint");


			if (pointInfo[i].isPeak) {
				setPointHeight (peakPoint, pointInfo [i].spectralFlux);
				setPointHeight (fluxPoint, 0f);
			} else {
				setPointHeight (fluxPoint, pointInfo [i].spectralFlux);
				setPointHeight (peakPoint, 0f);
			}
			setPointHeight (threshPoint, pointInfo [i].threshold);
		}
	}
	
	public void setPointHeight(Transform point, float height) {
		float displayMultiplier = 0.06f;

		point.localPosition = new Vector3(point.localPosition.x, height * displayMultiplier, point.localPosition.z);
	}
}
