using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class ColorSourceManager : MonoBehaviour 
{
	public RawImage ColorView;

    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
	public uint BytesPerPixel { get; private set; }

    private KinectSensor _Sensor;
    private ColorFrameReader _Reader;
    private byte[] _Data;
	private long _TotalPixelData;

	private Texture2D _Texture;
	public Texture2D GetColorTexture()
    {
        return _Texture;
    }

	public byte GetPixelColorByPosition(float x, float y, int rgbaOrder)
	{
		int x_index = (int)(x + 0.5f);
		int y_index = (int)(y + 0.5f);
		if (x_index < ColorWidth && x_index >= 0 && y_index < ColorHeight && y_index >= 0)
			return _Data[(BytesPerPixel * (ColorWidth * y_index + x_index)) + rgbaOrder];
		return 0;
	}

	void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader();
            
            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;
			BytesPerPixel = frameDesc.BytesPerPixel;
			_TotalPixelData = BytesPerPixel * ColorWidth * ColorHeight;

            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
            _Data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
            
            if (!_Sensor.IsOpen)
                _Sensor.Open();
        }
    }
    
    void Update () 
    {
        if (_Reader != null) 
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Rgba);
                _Texture.LoadRawTextureData(_Data);
                _Texture.Apply();
				if (ColorView != null)
					ColorView.texture = _Texture;

                frame.Dispose();
                frame = null;
            }
		}
	}

    void OnApplicationQuit()
    {
        if (_Reader != null) 
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null) 
        {
            if (_Sensor.IsOpen)
                _Sensor.Close();
            _Sensor = null;
        }
    }
}
