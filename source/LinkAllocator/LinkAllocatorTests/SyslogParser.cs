using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkAllocator;
using System.IO;
using System.Collections.Generic;

namespace LinkAllocatorTests
{
    public class SyslogParser
    {
        public static Topology CreateTopology(string fileName)
        {
            Topology t = new Topology();

            bool readingDevices = false;
            bool readingLinks = false;
            string currDeviceKey = String.Empty;

            Dictionary<string, List<string>> devicesData = new Dictionary<string, List<string>>();
            List<string[]> linksData = new List<string[]>();
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("ENDED PRINTING LOGICAL LINKS!"))
                    {
                        readingLinks = false;
                    }
                    if (line.Contains("FINISHED CONFIGURATION STRUCTURE PRINTING!"))
                    {
                        readingDevices = false;
                    }

                    if (readingDevices)
                    {
                        int cutTo = line.IndexOf("[Rp3Configuration::showConfigurationStructure]") + "[Rp3Configuration::showConfigurationStructure]".Length;
                        string cutLine = line.Substring(cutTo);
                        if (cutLine.EndsWith(":\t"))
                        {
                            currDeviceKey = cutLine.Substring(0, cutLine.Length - 3).TrimStart(new char[] { ' ', '\t' });
                        }
                        else
                        {
                            if (!devicesData.ContainsKey(currDeviceKey))
                            {
                                devicesData.Add(currDeviceKey, new List<string>());
                            }

                            //add only uniqie - there is some bug in topology (fake rx are connected many times)
                            string connectedDevice = cutLine.TrimStart(new char[] { ' ', '\t' });
                            if (!devicesData[currDeviceKey].Exists(x => x == connectedDevice))
                                devicesData[currDeviceKey].Add(connectedDevice);
                        }
                    }
                    if (readingLinks)
                    { //TODO: support for multi receiver/transmitter
                        int cutTo = line.IndexOf("NF/CellP/BBConf,") + "NF/CellP/BBConf,".Length;
                        string cutLine = line.Substring(cutTo);
                        linksData.Add(cutLine.Split(new char[] { ';' }));
                    }

                    if (line.Contains("PRINTING LOGICAL LINKS:"))
                    {
                        readingLinks = true;
                        linksData.Clear(); // so only last data will be taken
                    }
                    if (line.Contains("PRINTING CONFIGURATION STRUCTURE"))
                    {
                        readingDevices = true;
                        devicesData.Clear(); // so only last data will be taken
                    }
                }
            }

            //DEVICES
            SortedSet<string> devices = new SortedSet<string>();
            foreach (var deviceData in devicesData)
            {
                string dev = GetName(deviceData.Key);
                devices.Add(dev);
                foreach (string value in deviceData.Value)
                {
                    string dev2 = GetName(value);
                    devices.Add(dev2);
                }
            }
            foreach (string dev in devices)
            {
                t.AddDevice(dev);
            }

            //CONNECTIONS
            int nextConnNo = 1;
            foreach (var deviceData in devicesData)
            {
                foreach (string value in deviceData.Value)
	            {

                    string fromDev = GetName(deviceData.Key);
                    string toDev = GetName(value);
                    t.AddConnection(nextConnNo.ToString(), fromDev, toDev,
                        Math.Min(GetSpeed(deviceData.Key), GetSpeed(value)) * 10);
                    nextConnNo++;
	            }
            }

            //LOGICAL LINKS
            foreach (var linkData in linksData)
            {
                t.AddLink(linkData[0], linkData[1].Substring(1, linkData[1].Length - 2),
                    linkData[2].Substring(1, linkData[2].Length - 2), Convert.ToInt32(linkData[3]));
            }

            return t;
        }

        private static string GetName(string input)
        {
            return input.Substring(0, input.Length - " (8,UNDEF) ".Length);
        }

        private static int GetSpeed(string input)
        {
            string numberString = input.Substring(input.Length - "8,UNDEF) ".Length, 1);
            return Convert.ToInt32(numberString);
        }

    }
}
