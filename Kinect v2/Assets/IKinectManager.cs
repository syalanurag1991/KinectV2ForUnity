using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKinectManager
{
	// checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	bool IsKinectInitialized();

	// checks if Kinect is initialized and ready to use. If not, there was an error during Kinect-sensor initialization
	bool IsInitialized();

	// returns the single KinectManager instance
	IKinectManager GetInstance();

	// returns raw depth data
	ushort[] GetRawDepthData();

	// returns interpolated low resolution corrected depth data
	ushort[] GetLowResolutionDepthData();

	// returns corrected depth data for the whole frame
	ushort[] GetCorrectedDepthData();

	// returns normalized depth data for the whole frame
	float[] GetNormalizedDepthData();
	float[] GetSandBoxTopography();

	// returns depth data for a specific pixel
	ushort GetDepthForPixel(int x, int y);

	// get depth value for a pixel between 0 - 1
	float GetNormalizedDepthForPixel(ushort correctedDepth);

	// get depth value for a pixel between 0 - 1 for low resolution frame
	float[] GetLowResolutionNormalizedDepthData();

	// returns registered (corresponding) color pixel for input depth pixel's position
	Vector2 GetColorCorrespondingToDepthPixelPosition(Vector2 posDepth);

	// assigns in-place resistered (corresponding) color pixels to input depth pixels for the whole frame
	void GetColorForRegsiteredColorStreamPixel(ref int index, ref KinectData sourceKinectData, ref Color32 result);

	// returns raw color image data
	Color32[] GetRGBFrameData();

	// performs initialization of instance upon awake
	void Awake();

	// get colors for graded depth stream
	void InitializeGradedDepthStreamColors();

	// initialize storage for frames of various streams
	void InitializeFeeds();

	// get current instance of kinect-data class corresoponding to low resolution frames
	KinectData GetLowResolutionKinectData();

	// get current instance of kinect-data class corresoponding to full resolution frames
	KinectData GetFullResolutionKinectData();

	// generate numerical values of levels for graded depth stream
	void AdjustGradedDepthLevels();

	// create low resolution image and other data using Bilinear Interpolation
	void ProduceLowResolutionData();

	// function for assisting creation of low resolution images using bilinear interpolation 
	void GetFourNeighborPixels(ref int[] inputIndexesOfNeighborPixels, ref KinectData sourceKinectData, ref KinectData resultNeighborPixels);

	// get actual depth value in millimeters for each pixel
	void ProcessRawDepthData();

	// find minimum and maximum shortvalues received
	void FindMinMaxDistancePerceived(ushort correctedDistance);

	// create grayscale-color for depth pixel (in-place) using normalized depth
	void GetColorForDepthStream(ref float normalizedDepth, ref Color32 result);

	// assign rgb-color to depth pixel for generating graded-depth view
	void GetColorForGradedDepthStreamPixel(ref ushort correctedDistance, ref Color32 result);

	// Make sure to kill the Kinect on quitting
	void OnApplicationQuit();
}
