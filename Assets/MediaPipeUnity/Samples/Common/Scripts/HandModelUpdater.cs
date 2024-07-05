using Mediapipe.Unity.CoordinateSystem; // Import the RealWorldCoordinate class
using Mediapipe.Unity.Sample.HandTracking;
using Mediapipe;
using System.Collections.Generic;
using UnityEngine;
using Mediapipe.Unity;
using Color = UnityEngine.Color;

public class EventTest : MonoBehaviour
{
  public HandTrackingSolution handTrackingSolution;
  private Vector3 _targetPosition;
  private bool _positionUpdated = false;

  public bool isMirrored = false; // Set to true if your camera is mirrored


  private void Start()
  {
    if (handTrackingSolution != null)
    {
      handTrackingSolution.OnHandLandmarksOutputEvent += HandleNewLandmarks;
    }
  }

  private void HandleNewLandmarks(List<NormalizedLandmarkList> landmarks, NormalizedLandmark ringFingerLandmark)
  {
    if (landmarks == null || landmarks.Count == 0)
    {
      return;
    }

    //NormalizedLandmark targetLandmark = landmarks[0].Landmark[14];

    // Get landmarks 14 and 13
    var landmark14 = landmarks[0].Landmark[14];
    var landmark13 = landmarks[0].Landmark[13];

    // Calculate the midpoint
    var midX = (landmark14.X + landmark13.X) / 2f;
    var midY = (landmark14.Y + landmark13.Y) / 2f;
    var midZ = (landmark14.Z + landmark13.Z) / 2f;

    // Create a new NormalizedLandmark for the midpoint
    var midPoint = new NormalizedLandmark
    {
      X = midX,
      Y = midY,
      Z = midZ
    };

    //ImageToLocalPoint:
    var imageWidth = UnityEngine.Screen.width;
    var imageHeight = UnityEngine.Screen.height;
    var xMin = -30f;
    var xMax = 30f;
    var yMin = -50f;
    var yMax = 50f;

    // Invert the X-coordinate
    var invertedMidX = 1f - midPoint.X;

    // Use ImageToLocalPoint for positioning
    _targetPosition = ImageCoordinate.ImageToLocalPoint(
        (int)(invertedMidX * imageWidth), // Convert normalized X to pixel coordinates
        (int)(midPoint.Y * imageHeight), // Convert normalized Y to pixel coordinates
        (int)(midPoint.Z * 100), // Scale Z appropriately (adjust the multiplier as needed)
        xMin, xMax, yMin, yMax,
        imageWidth, imageHeight, RotationAngle.Rotation0, isMirrored);

    _positionUpdated = true;
  }

  private void Update()
  {
    if (_positionUpdated)
    {
      transform.position = _targetPosition;
      _positionUpdated = false;

    }
  }
}


