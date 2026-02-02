using System.Collections;
using System.Collections.Generic;

using Sonat.Debugger;
using Sonat.FirebaseModule.Analytic;
using UnityEngine;
#if using_newtonsoft
using Newtonsoft.Json;
#endif
#if using_networking_transport
using Unity.Networking.Transport;
#endif

namespace Sonat.DebugViewModule
{
    public class DebugLogClient : MonoBehaviour
    {
#if using_networking_transport
        NetworkDriver m_Driver;
        NetworkConnection m_Connection;
#endif
        private bool connected = false;
        private bool disconnected;
        private bool lostFocus;
        private List<EventLogData> logWaitQueue = new();

        // Start is called before the first frame update
        void Start()
        {
            TryConnect();
        }

        public void TryConnect()
        {
            if (connected) return;
#if using_networking_transport
            m_Driver = NetworkDriver.Create();

            var endpoint = NetworkEndpoint.Parse(SonatDebugView.ip, SonatDebugView.port);
            m_Connection = m_Driver.Connect(endpoint);
#endif
        }

        void OnDestroy()
        {
#if using_networking_transport
            m_Driver.Dispose();
#endif
        }

#if using_networking_transport
        // Update is called once per frame
        void Update()
        {
            m_Driver.ScheduleUpdate().Complete();

            if (!m_Connection.IsCreated)
            {
                return;
            }

            Unity.Collections.DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    SonatDebugType.Development.Log("Connected to the server!");

                    connected = true;
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    uint value = stream.ReadUInt();
                    SonatDebugType.Development.Log($"Got the value {value} back from the server!");

                    m_Connection.Disconnect(m_Driver);
                    m_Connection = default;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    SonatDebugType.Development.Log("Client got disconnected from server!");
                    m_Connection = default;
                    disconnected = true;
                    connected = false;
                }
            }
        }
#endif

        public void SendData(EventLogData data)
        {
#if using_networking_transport
            if (!m_Connection.IsCreated || disconnected) return;

            if (lostFocus)
            {
                logWaitQueue.Add(data);
                return;
            }
#if using_newtonsoft
            string json = JsonConvert.SerializeObject(data);
            SonatDebugType.Development.Log($"Send Json Length: {json.Length}");
            if(json.Length > 512) return;
            
            m_Driver.BeginSend(m_Connection, out var writer);
            writer.WriteFixedString512(json);
            m_Driver.EndSend(writer);
#endif
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            lostFocus = !hasFocus;
            if (!lostFocus)
            {
                StartCoroutine(SendQueue());
            }
        }

        IEnumerator SendQueue()
        {
            yield return new WaitForEndOfFrame();
            foreach (var eventLogData in logWaitQueue)
            {
                SendData(eventLogData);
                yield return new WaitForSeconds(0.1f);
            }

            logWaitQueue.Clear();
        }
    }
}