using UnityEngine;

namespace Mediapipe.Unity.Tutorial
{
  public class HelloWorld : MonoBehaviour
  {
    private void Start()
    {

      var configText = @"
        input_stream: ""in""
        output_stream: ""out""
        node {
          calculator: ""PassThroughCalculator""
          input_stream: ""in""
          output_stream: ""out1""
        }
        node {
          calculator: ""PassThroughCalculator""
          input_stream: ""out1""
          output_stream: ""out""
        }
      ";

      var graph = new CalculatorGraph(configText);
      var poller = graph.AddOutputStreamPoller<string>("out");
      graph.StartRun();

      for (var i = 0; i < 10; i++)
      {
        var input = Packet.CreateStringAt("Hello World!", i);
        graph.AddPacketToInputStream("in", input);
      }

      graph.CloseInputStream("in");

      var output = new Packet<string>();
      while (poller.Next(output))
      {
        Debug.Log(output.Get());
      }

      graph.WaitUntilDone();

      poller.Dispose();
      graph.Dispose();
      output.Dispose();

      Debug.Log("Done");
    }
  }
}
