using Mediapipe.Unity.Sample.HandTracking;
using Mediapipe; // Import for Mediapipe types (if needed)
using System.Collections.Generic; // Import for List<T>
using UnityEngine; // Import for MonoBehaviour, Debug.Log, etc.

public class EventTest : MonoBehaviour
{
  public HandTrackingSolution handTrackingSolution;
  private Vector3 targetPosition; // Store the new position
  private bool positionUpdated = false; // Flag to signal position change

  void Start()
  {
    if (handTrackingSolution != null)
    {
      handTrackingSolution.OnHandLandmarksOutputEvent += HandleNewLandmarks;
    }
  }

  // Event handler method 
  private void HandleNewLandmarks(List<NormalizedLandmarkList> landmarks, NormalizedLandmark ringFingerLandmark)
  {
    if (landmarks == null || landmarks.Count == 0 || ringFingerLandmark == null)
    {
      return;
    }

    // **Safely update the position**
    targetPosition = new Vector3(ringFingerLandmark.X, ringFingerLandmark.Y, ringFingerLandmark.Z);
    positionUpdated = true;
  }

  void Update()
  {
    // Apply the position change on the main thread
    if (positionUpdated)
    {
      transform.position = targetPosition;
      positionUpdated = false;
    }
  }
}
