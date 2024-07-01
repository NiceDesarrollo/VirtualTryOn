using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Mediapipe.Unity.CoordinateSystem;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mediapipe.Unity.Tutorial
{
  public class FaceMesh : MonoBehaviour
  {
    [SerializeField] private TextAsset _configAsset;
    [SerializeField] private RawImage _screen;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private int _fps;

    private CalculatorGraph _graph;
    private OutputStream<ImageFrame> _outputVideoStream;
    private OutputStream<List<NormalizedLandmarkList>> _multiFaceLandmarksStream;
    private ResourceManager _resourceManager;

    private WebCamTexture _webCamTexture;
    private Texture2D _inputTexture;
    private Color32[] _inputPixelData;
    private Texture2D _outputTexture;
    private Color32[] _outputPixelData;

    private IEnumerator Start()
    {

      if (WebCamTexture.devices.Length == 0)
      {
        throw new System.Exception("Web Camera devices are not found");
      }

      var webCamDevice = WebCamTexture.devices[0];
      _webCamTexture = new WebCamTexture(webCamDevice.name, _width, _height, _fps);
      _webCamTexture.Play();

      yield return new WaitUntil(() => _webCamTexture.width > 16);

      _screen.rectTransform.sizeDelta = new Vector2(_width, _height);

      _inputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
      _inputPixelData = new Color32[_width * _height];
      _outputTexture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
      _outputPixelData = new Color32[_width * _height];

      _screen.texture = _outputTexture;

      _resourceManager = new StreamingAssetsResourceManager();
      yield return _resourceManager.PrepareAssetAsync("face_detection_short_range.bytes");
      yield return _resourceManager.PrepareAssetAsync("face_landmark_with_attention.bytes");

      var stopwatch = new Stopwatch();

      _graph = new CalculatorGraph(_configAsset.text);
      _outputVideoStream = new OutputStream<ImageFrame>(_graph, "output_video");
      _multiFaceLandmarksStream = new OutputStream<List<NormalizedLandmarkList>>(_graph, "multi_face_landmarks");
      _outputVideoStream.StartPolling();
      _multiFaceLandmarksStream.StartPolling();
      _graph.StartRun();
      stopwatch.Start();

      var screenRect = _screen.GetComponent<RectTransform>().rect;

      while (true)
      {


        _inputTexture.SetPixels32(_webCamTexture.GetPixels32(_inputPixelData));
        var imageFrame = new ImageFrame(ImageFormat.Types.Format.Srgba, _width, _height, _width * 4, _inputTexture.GetRawTextureData<byte>());
        var currentTimestamp = stopwatch.ElapsedTicks / (System.TimeSpan.TicksPerMillisecond / 1000);
        _graph.AddPacketToInputStream("input_video", Packet.CreateImageFrameAt(imageFrame, currentTimestamp));

        var task1 = _outputVideoStream.WaitNextAsync();
        var task2 = _multiFaceLandmarksStream.WaitNextAsync();
        var task = Task.WhenAll(task1, task2);

        yield return new WaitUntil(() => task.IsCompleted);




        if (!task1.Result.ok || !task2.Result.ok)
        {
          throw new System.Exception("Something went wrong");
        }




        var outputVideoPacket = task1.Result.packet;
        if (outputVideoPacket != null)
        {
          var outputVideo = outputVideoPacket.Get();
          if (outputVideo.TryReadPixelData(_outputPixelData))
          {
            _outputTexture.SetPixels32(_outputPixelData);
            _outputTexture.Apply();
          }
        }



        var multiFaceLandmarksPacket = task2.Result.packet;
        if (multiFaceLandmarksPacket != null)
        {
          var multiFaceLandmarks = multiFaceLandmarksPacket.Get(NormalizedLandmarkList.Parser);
          if (multiFaceLandmarks != null && multiFaceLandmarks.Count > 0)
          {
            foreach (var landmarks in multiFaceLandmarks)
            {
              // top of the head
              var topOfHead = landmarks.Landmark[10];
              Debug.Log($"Unity Local Coordinates: {screenRect.GetPoint(topOfHead)}, Image Coordinates: {topOfHead}");
            }
          }
        }




      }

    }

    private void OnDestroy()
    {
      if (_webCamTexture != null)
      {
        _webCamTexture.Stop();
      }

      _outputVideoStream?.Dispose();
      _outputVideoStream = null;
      _multiFaceLandmarksStream?.Dispose();
      _multiFaceLandmarksStream = null;

      if (_graph != null)
      {
        try
        {
          _graph.CloseInputStream("input_video");
          _graph.WaitUntilDone();
        }
        finally
        {
          _graph.Dispose();
          _graph = null;
        }
      }
    }





  }




}
