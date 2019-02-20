using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public enum DepthViewMode
{
	SeparateSourceReaders,
	MultiSourceReader,
}

public class DepthSourceManager : MonoBehaviour
{
	public DepthViewMode ViewMode = DepthViewMode.SeparateSourceReaders;
	public int DepthWidth { get; private set; }
	public int DepthHeight { get; private set; }

	public ColorSourceManager ColorManager;
	public MultiSourceManager MultiManager;
	public GameObject PointCloud;
	public RawImage RegisteredColorView;
	public int DownsampleSize = 4;						// Only works at 4 right now
	public double DepthScale = 0.01f;
	public bool TransparentBG = false;

	private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private CoordinateMapper _Mapper;
	private ushort[] _Data;
	private Mesh _Mesh;
	private Vector3[] _Vertices;
	private Vector2[] _UV;
	private int[] _Triangles;

	private Texture2D _RegisteredColorViewTexture;
	private byte[] _RegisteredColorViewData;
	private int _BytesPerPixel;
	private int _ColorWidth = 0, _ColorHeight = 0;
	
	void Start () 
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null) 
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader();
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels];
			_Mapper = _Sensor.CoordinateMapper;

			var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
			DepthWidth = frameDesc.Width;
			DepthHeight = frameDesc.Height;

			// Downsample to lower resolution
			if(PointCloud != null)
				CreateMesh(frameDesc.Width / DownsampleSize, frameDesc.Height / DownsampleSize);

			if (!_Sensor.IsOpen)
				_Sensor.Open();
		}
    }

	void Update()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			if (ViewMode == DepthViewMode.MultiSourceReader)
				ViewMode = DepthViewMode.SeparateSourceReaders;
			else
				ViewMode = DepthViewMode.MultiSourceReader;
		}

		if (_Sensor == null)
			return;

		if (_Reader != null)
		{
			var frame = _Reader.AcquireLatestFrame();
			if (frame != null)
			{
				frame.CopyFrameDataToArray(_Data);
				frame.Dispose();
				frame = null;

				if (ViewMode == DepthViewMode.SeparateSourceReaders)
				{
					if (ColorManager == null)
						return;
					else
					{
						if (_RegisteredColorViewData == null || _RegisteredColorViewTexture == null)
							CreateRegisteredColorViewStorage(ColorManager.ColorWidth, ColorManager.ColorHeight, (int)ColorManager.BytesPerPixel);
					}
				} else
				{
					if (MultiManager == null)
						return;
					else
					{
						if (_RegisteredColorViewData == null || _RegisteredColorViewTexture == null)
							CreateRegisteredColorViewStorage(MultiManager.ColorWidth, MultiManager.ColorHeight, (int)MultiManager.BytesPerPixel);
					}
				}

				RefreshData();
				if (PointCloud != null)
					PointCloud.GetComponent<Renderer>().material.mainTexture = ColorManager.GetColorTexture();
			}
		}
		else
			Debug.Log("Reader is null!");
	}

	public ushort[] GetData()
	{
		return _Data;
	}

	private void RefreshData()
	{
		if (_Data == null || _Data.Length == 0 || _ColorWidth == 0 || _ColorHeight == 0)
			return;

		var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
		ColorSpacePoint[] colorSpace = new ColorSpacePoint[_Data.Length];
		DepthSpacePoint[] depthSpace = new DepthSpacePoint[_ColorWidth * _ColorHeight];
		_Mapper.MapDepthFrameToColorSpace(_Data, colorSpace);
		_Mapper.MapColorFrameToDepthSpace(_Data, depthSpace);

		for (int y = 0; y < frameDesc.Height; y += DownsampleSize)
		{
			for (int x = 0; x < frameDesc.Width; x += DownsampleSize)
			{
				int indexX = x / DownsampleSize;
				int indexY = y / DownsampleSize;
				int smallIndex = (indexY * (frameDesc.Width / DownsampleSize)) + indexX;

				double avg = GetAvg(_Data, x, y, frameDesc.Width, frameDesc.Height);

				avg = avg * DepthScale;

				_Vertices[smallIndex].z = (float)avg;

				// Update UV mapping with CDRP
				var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
				_UV[smallIndex] = new Vector2(colorSpacePoint.X / _ColorWidth, colorSpacePoint.Y / _ColorHeight);

				int colorSpaceIndex = (int)((colorSpacePoint.Y + 0.5f) * _ColorHeight + colorSpacePoint.X + 0.5f);
				int depthSpaceIndex = 0;
				if (colorSpaceIndex < _ColorWidth && colorSpaceIndex < _ColorHeight && colorSpaceIndex > 0 && colorSpaceIndex > 0)
					depthSpaceIndex = (int)((depthSpace[colorSpaceIndex].Y + 0.5f) * DepthWidth + depthSpace[colorSpaceIndex].X + 0.5);

				for (int i = 0; i < _BytesPerPixel; i++)
				{

					if (i == _BytesPerPixel - 1 && !TransparentBG)
					{
						_RegisteredColorViewData[_BytesPerPixel * smallIndex + i] = 255;
						continue;
					}

					if (ColorManager != null)
						_RegisteredColorViewData[_BytesPerPixel * smallIndex + i] = ColorManager.GetPixelColorByPosition(colorSpacePoint.X, colorSpacePoint.Y, i);
					else if (MultiManager != null)
						_RegisteredColorViewData[_BytesPerPixel * smallIndex + i] = MultiManager.GetPixelColorByPosition(colorSpacePoint.X, colorSpacePoint.Y, i);
					else
						break;
				}
			}
		}

		RefreshRegisteredColorStream();
		_Mesh.vertices = _Vertices;
		_Mesh.uv = _UV;
		_Mesh.triangles = _Triangles;
		_Mesh.RecalculateNormals();
	}

	private void RefreshRegisteredColorStream()
	{
		_RegisteredColorViewTexture.LoadRawTextureData(_RegisteredColorViewData);
		_RegisteredColorViewTexture.Apply();

		if (RegisteredColorView != null)
			RegisteredColorView.texture = _RegisteredColorViewTexture;

		return;
	}

	private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
	{
		double sum = 0.0;
		for (int y1 = y; y1 < y + DownsampleSize; y1++)
		{
			for (int x1 = x; x1 < x + DownsampleSize; x1++)
			{
				int fullIndex = (y1 * width) + x1;
				if (depthData[fullIndex] == 0)
					sum += 4500;
				else
					sum += depthData[fullIndex];

			}
		}

		return sum / (DownsampleSize * DownsampleSize);
	}

	private void CreateRegisteredColorViewStorage(int colorWidth, int colorHeight, int colorBPP)
	{
		// update color view realtyed variables
		_BytesPerPixel = colorBPP;
		_ColorWidth = colorWidth;
		_ColorHeight = colorHeight;
		// create storage variables
		var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
		_RegisteredColorViewData = new byte[(frameDesc.Width / DownsampleSize) * (frameDesc.Height / DownsampleSize) * _BytesPerPixel];
		_RegisteredColorViewTexture = new Texture2D(frameDesc.Width / DownsampleSize, frameDesc.Height / DownsampleSize, TextureFormat.RGBA32, false);
		return;
	}

	private void CreateMesh(int width, int height)
	{
		_Mesh = new Mesh();
		PointCloud.GetComponent<MeshFilter>().mesh = _Mesh;

		_Vertices = new Vector3[width * height];
		_UV = new Vector2[width * height];
		_Triangles = new int[6 * ((width - 1) * (height - 1))];

		int triangleIndex = 0;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int index = (y * width) + x;

				_Vertices[index] = new Vector3(x, -y, 0);
				_UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

				// Skip the last row/col
				if (x != (width - 1) && y != (height - 1))
				{
					int topLeft = index;
					int topRight = topLeft + 1;
					int bottomLeft = topLeft + width;
					int bottomRight = bottomLeft + 1;

					_Triangles[triangleIndex++] = topLeft;
					_Triangles[triangleIndex++] = topRight;
					_Triangles[triangleIndex++] = bottomLeft;
					_Triangles[triangleIndex++] = bottomLeft;
					_Triangles[triangleIndex++] = topRight;
					_Triangles[triangleIndex++] = bottomRight;
				}
			}
		}

		_Mesh.vertices = _Vertices;
		_Mesh.uv = _UV;
		_Mesh.triangles = _Triangles;
		_Mesh.RecalculateNormals();
	}

	private void OnGUI()
	{
		GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
		GUI.TextField(new Rect(Screen.width - 250, 10, 250, 20), "DepthMode: " + ViewMode.ToString());
		GUI.EndGroup();
	}

	private void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

		if (_Mapper != null)
			_Mapper = null;

		if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
                _Sensor.Close();
            
            _Sensor = null;
        }
    }
}
