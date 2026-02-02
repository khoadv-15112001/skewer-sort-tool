using UnityEngine;
using System.IO;
using System.IO.Pipes;
using System;

public class PipeClient : MonoBehaviour
{
    public void SendCommandToB(string command)
    {
        try
        {
            using (var client = new NamedPipeClientStream(".", "B_PIPE", PipeDirection.Out))
            {
                client.Connect(2000); // timeout 2 giây

                using (StreamWriter writer = new StreamWriter(client))
                {
                    writer.AutoFlush = true;
                    writer.WriteLine(command);
                }
            }

            Debug.Log("[A] Sent command: " + command);
        }
        catch (Exception e)
        {
            Debug.LogError("[A] Cannot connect to B pipe: " + e.Message);
        }
    }
}
