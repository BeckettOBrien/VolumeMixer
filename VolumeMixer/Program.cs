using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using CSCore;
using CSCore.CoreAudioAPI;
using System.Diagnostics;
using CSCore.Win32;

namespace VolumeMixer
{
    class Program
    {
        static void Main(string[] args)
        {

            String targetProcessName = "";
            if (String.Equals(args[0], "focused"))
            {
                targetProcessName = Process.GetProcessById(GetActiveProcessID()).ProcessName;
            }
                else
            {
                targetProcessName = args[0];
            }

            float amount = Convert.ToSingle(args[1]);

            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                        using (var sessionControl = session.QueryInterface<AudioSessionControl2>())
                        {
                            String sessionName = Process.GetProcessById(sessionControl.ProcessID).ProcessName;
                            if (String.Equals(sessionName, targetProcessName))
                            {
                                Guid e = Guid.NewGuid();
                                if (amount == 0)
                                {
                                    NativeBool muted;
                                    simpleVolume.GetMuteInternal(out muted);
                                    simpleVolume.SetMuteNative(!muted, e);
                                } 
                                    else
                                {
                                    float currentVolume = simpleVolume.MasterVolume;
                                    float newVolume = Clamp(currentVolume + amount, 0.0f, 1.0f);
                                    simpleVolume.SetMasterVolumeNative(newVolume, e);
                                }
                            }
                        }
                    }
                }
            }

        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    //Console.WriteLine("DefaultDevice: " + device.FriendlyName);
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static int GetActiveProcessID()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            return (int)pid;
        }

        private static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

    }

}