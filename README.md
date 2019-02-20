# KinectV2 For Unity
Library for using Kinect v2 with Unity.
Unity version = 2018.3 with .Net 4.x.

The project contains 2 scenes as of now:

![image](https://user-images.githubusercontent.com/32419039/53058714-23e60b80-3469-11e9-90ef-7e5fa51fd0ac.png)

## 1)	MainScene
This is a slightly modified version of the original demo-scene developed by Microsoft. Displays color, infrared and body streams along with streams along with a point-cloud view. I replacedthe 3D cube object by a UI canvas to get more FPS.

![image](https://user-images.githubusercontent.com/32419039/53059583-30b82e80-346c-11e9-8451-8ad9110adf10.png)

## 2)	PointCloud
This scene was developed by me using “registered frames” from Kinect. The bindings to get color and depth frame data were changed from multi-frame reader to color and depth frame readers. Reason being that using multi-frame reader limits Kinect’s output of depth data to 15 FPS. This is because getting color-frame data is an added stress for the multi-frame reader API. Using color-frame reader and depth-frame readers separately, along with the CoordinateMapper class, generates the output at 30 FPS which is more desirable. 

![image](https://user-images.githubusercontent.com/32419039/53058816-79221d00-3469-11e9-87cc-f54751cf579a.png)
