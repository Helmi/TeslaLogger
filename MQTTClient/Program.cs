﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaLogger;
using uPLibrary.Networking.M2Mqtt;

namespace MQTTClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string clientid = Guid.NewGuid().ToString();

            MqttClient client = null;
            try
            {
                Tools.Log("MqttClient Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

                if (Properties.Settings.Default.MQTTHost.Length == 0)
                {
                    Tools.Log("No MQTTHost settings -> MQTT disabled!");
                    return;
                }

                if (Properties.Settings.Default.Topic.Length == 0)
                {
                    Tools.Log("No Topic settings -> MQTT disabled!");
                    return;
                }

                client = new MqttClient(Properties.Settings.Default.MQTTHost);

                if (Properties.Settings.Default.Name.Length > 0 && Properties.Settings.Default.Password.Length > 0)
                {
                    Tools.Log("Connecting with credentials: " + Properties.Settings.Default.MQTTHost);
                    client.Connect(clientid, Properties.Settings.Default.Name, Properties.Settings.Default.Password);
                }
                else
                {
                    Tools.Log("Connecting without credentials: " + Properties.Settings.Default.MQTTHost);
                    client.Connect(clientid);
                }
                Tools.Log("Connected!");
            }
            catch (Exception ex)
            {
                Tools.Log(ex.Message);
            }

            string lastjson = "-";

            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(5000);

                    if (!client.IsConnected)
                    {
                        Tools.Log("Reconnect");
                        client.Connect(clientid);
                    }

                    string temp = System.IO.File.ReadAllText("/etc/teslalogger/current_json.txt");
                    if (temp != lastjson)
                    {
                        lastjson = temp;
                        client.Publish(Properties.Settings.Default.Topic, Encoding.UTF8.GetBytes(lastjson));
                    }
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(30000);
                    Tools.Log(ex.ToString());
                }
            }
        }
    }
}
