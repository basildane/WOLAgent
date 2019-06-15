using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Xml;

namespace Agent
{
    partial class WOL_Agent : ServiceBase
    {
        private BackgroundWorker backgroundworker;
        private int sendPort, listenPort;
        private string sendAddress, listenAddress;
        const string app = "WOL_Agent";

        public WOL_Agent()
        {
            InitializeComponent();

            string filename = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                "\\Aquila Technology\\Agent\\config.xml";
            XmlTextReader reader = new XmlTextReader(filename);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);
            reader.Close();

            XmlElement agent = xmlDoc["agent"];
            XmlNode send = agent.SelectSingleNode("send");
            sendPort = int.Parse(send["port"].InnerText);
            sendAddress = send["address"].InnerText;

            XmlNode listen = agent.SelectSingleNode("listen");
            listenPort = int.Parse(listen["port"].InnerText);
            listenAddress = listen["address"].InnerText;

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = app;
                eventLog.WriteEntry("WOL agent listening on " + listenAddress + ":" + listenPort,
                    EventLogEntryType.Information, 101);
            }

            backgroundworker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            backgroundworker.DoWork += new DoWorkEventHandler(DoWork);
        }

        protected override void OnStart(string[] args)
        {
            backgroundworker.RunWorkerAsync();
        }

        protected override void OnStop()
        {
            backgroundworker.CancelAsync();
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            UdpClient udpSender = new UdpClient();

            IPEndPoint listenEP = new IPEndPoint(IPAddress.Parse(listenAddress), listenPort);
            UdpClient udpReceiver = new UdpClient(listenEP);

            Dictionary<string, DateTime> list = new Dictionary<string, DateTime>();
            DateTime lastTime;
            string mac;

            udpSender.Connect(sendAddress, sendPort);
            udpSender.EnableBroadcast = true;

            while (worker.CancellationPending == false)
            {
                try
                {
                    Byte[] receiveBytes = udpReceiver.Receive(ref listenEP);

                    // verify that WOL packet

                    if (validPacket(receiveBytes))
                    {
                        // we have a valid packet
                        using (EventLog eventLog = new EventLog("Application"))
                        {
                            eventLog.Source = app;
                            eventLog.WriteEntry("WOL packet received from " + listenEP.Address.ToString(),
                                EventLogEntryType.Information, 101);
                        }

                        mac = BitConverter.ToString(receiveBytes, 6, 6);

                        if (list.TryGetValue(mac, out lastTime))
                        {
                            if ((DateTime.UtcNow - lastTime).TotalSeconds > 2)
                            {
                                list.Remove(mac);
                                list.Add(key: mac, value: DateTime.UtcNow);
                                udpSender.Send(receiveBytes, receiveBytes.Length);
                            }
                        }
                        else
                        {
                            list.Add(key: mac, value: DateTime.UtcNow);
                            udpSender.Send(receiveBytes, receiveBytes.Length);
                        }
                    }
                }

                catch (Exception ex)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = app;
                        eventLog.WriteEntry("WOL Agent error: " + ex.ToString(), EventLogEntryType.Error, 201, 1);
                    }
                }
            }

            e.Cancel = true;
            return;
        }

        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            IStructuralEquatable eqa1 = a1;
            return eqa1.Equals(a2, StructuralComparisons.StructuralEqualityComparer);
        }

        private bool validPacket(byte[] receiveBytes)
        {
            if (receiveBytes.Length < 102)
                return false;

            for (int i = 0; i < 5; i++)
                if (receiveBytes[i] != 0xff) return false;

            for (int i = 1; i < 15; i++)
                for (int j = 0; j < 5; j++)
                    if (receiveBytes[6 + j] != receiveBytes[6 + j + (i * 6)]) return false;

            return true;
        }

    }
}
