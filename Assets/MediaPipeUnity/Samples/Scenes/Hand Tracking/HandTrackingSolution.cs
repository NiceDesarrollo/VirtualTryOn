// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Components.Containers;
using Mediapipe.Tasks.Vision.HandLandmarker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity.Sample.HandTracking
{
  public class HandTrackingSolution : ImageSourceSolution<HandTrackingGraph>
  {

    [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
    [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;


    //Important shet
    [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;


    [SerializeField] private NormalizedRectListAnnotationController _handRectsFromLandmarksAnnotationController;

    // Create a public event to notify when new landmarks are available
    public event Action<List<NormalizedLandmarkList>, NormalizedLandmark> OnHandLandmarksOutputEvent;


    public HandTrackingGraph.ModelComplexity modelComplexity
    {
      get => graphRunner.modelComplexity;
      set => graphRunner.modelComplexity = value;
    }

    public int maxNumHands
    {

      get => graphRunner.maxNumHands;
      set => graphRunner.maxNumHands = value;
    }

    public float minDetectionConfidence
    {
      get => graphRunner.minDetectionConfidence;
      set => graphRunner.minDetectionConfidence = value;
    }

    public float minTrackingConfidence
    {
      get => graphRunner.minTrackingConfidence;
      set => graphRunner.minTrackingConfidence = value;
    }

    protected override void OnStartRun()
    {
      if (!runningMode.IsSynchronous())
      {

        //not used
        //graphRunner.OnPalmDetectectionsOutput += OnPalmDetectionsOutput;

        //not used
        //graphRunner.OnHandRectsFromPalmDetectionsOutput += OnHandRectsFromPalmDetectionsOutput;

        //The lines of the hand !!MOST IMPORTANT¡¡
        graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;

        // TODO: render HandWorldLandmarks annotations

        //The purple square around the hand
        //graphRunner.OnHandRectsFromLandmarksOutput += OnHandRectsFromLandmarksOutput;

        graphRunner.OnHandednessOutput += OnHandednessOutput;

      }

      var imageSource = ImageSourceProvider.ImageSource;

      //SetupAnnotationController = Fix the image on the correct orientation
      SetupAnnotationController(_palmDetectionsAnnotationController, imageSource, true);
      SetupAnnotationController(_handRectsFromPalmDetectionsAnnotationController, imageSource, true);
      SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
      SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);
    }

    protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      graphRunner.AddTextureFrameToInputStream(textureFrame);
    }

    protected override IEnumerator WaitForNextValue()
    {
      var task = graphRunner.WaitNext();

      yield return new WaitUntil(() => task.IsCompleted);

      //Last tutorial part

      var result = task.Result;



      _palmDetectionsAnnotationController.DrawNow(result.palmDetections);
      _handRectsFromPalmDetectionsAnnotationController.DrawNow(result.handRectsFromPalmDetections);
      _handLandmarksAnnotationController.DrawNow(result.handLandmarks, result.handedness);
      // TODO: render HandWorldLandmarks annotations
      _handRectsFromLandmarksAnnotationController.DrawNow(result.handRectsFromLandmarks);
    }



    private void OnHandLandmarksOutput(object stream, OutputStream<List<NormalizedLandmarkList>>.OutputEventArgs eventArgs)
    {

      var packet = eventArgs.packet;

      var value = packet == null ? default : packet.Get(NormalizedLandmarkList.Parser);

      //Get the land marks coordinates
      if (value != null)
      {

        var ringFingerBaseLandmark = value[0].Landmark[14];

        OnHandLandmarksOutputEvent?.Invoke(value, ringFingerBaseLandmark);

      }

      _handLandmarksAnnotationController.DrawLater(value);
    }





    private void OnHandednessOutput(object stream, OutputStream<List<ClassificationList>>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(ClassificationList.Parser);
      _handLandmarksAnnotationController.DrawLater(value);
    }



    private void OnPalmDetectionsOutput(object stream, OutputStream<List<Detection>>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(Detection.Parser);
      _palmDetectionsAnnotationController.DrawLater(value);
    }

    private void OnHandRectsFromPalmDetectionsOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
      _handRectsFromPalmDetectionsAnnotationController.DrawLater(value);
    }

    private void OnHandRectsFromLandmarksOutput(object stream, OutputStream<List<NormalizedRect>>.OutputEventArgs eventArgs)
    {
      var packet = eventArgs.packet;
      var value = packet == null ? default : packet.Get(NormalizedRect.Parser);
      _handRectsFromLandmarksAnnotationController.DrawLater(value);
    }

  }
}
