using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Linq;

namespace BatteryWarnings
{
    class Program
    {
        static uint GetMinimumBatteryLevel()
        {
            const uint kMin = 1;
            const uint kMax = 50;
            Console.Write("Input the minimum battery level between {0} and {1}.\n", kMin, kMax);
            uint minimum;

            string result = Console.ReadLine();

            while (!uint.TryParse(result, out minimum) || (minimum < kMin) || (minimum > kMax))
            {
                Console.Write("Not a valid number, try again.\n");

                result = Console.ReadLine();
            }
            return minimum;
        }

        static uint GetMaximumBatteryLevel()
        {
            const uint kMin = 51;
            const uint kMax = 100;
            Console.Write("Input the maximum battery level between {0} and {1}.\n", kMin, kMax);
            uint maximum;

            string result = Console.ReadLine();

            while (!uint.TryParse(result, out maximum) || (maximum < kMin) || (maximum > kMax))
            {
                Console.Write("Not a valid number, try again.\n");

                result = Console.ReadLine();
            }
            return maximum;
        }

        static Battery GetBattery()
        {
            // For now, exit if system isn't Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Write("This system is not currently supported, but there are plans to support it in the future. :)\n");
                Console.Write("The application will exit once you press Enter.\n");
                Console.ReadLine();
                Environment.Exit(0);
            }

            // If statement here for the future
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var query = new ObjectQuery("Select * FROM Win32_Battery");
                var searcher = new ManagementObjectSearcher(query);
                var collection = searcher.Get();

                var result = collection.OfType<ManagementObject>().First();
                Battery battery = new();
                battery.SetChargeLevel(int.Parse(result.Properties["EstimatedChargeRemaining"].Value.ToString()));
                battery.SetStatus((BatteryStatuses)int.Parse(result.Properties["BatteryStatus"].Value.ToString()));

                return battery;
            }

            // This will never be reached
            Battery bat = new();
            return bat;

            //// Linux
            //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            //    {
            //        process.StartInfo.FileName = "cat";
            //        process.StartInfo.Arguments = @"/sys/class/power_supply/BAT0/capacity";
            //        process.StartInfo.UseShellExecute = false;
            //        process.StartInfo.CreateNoWindow = true;
            //        process.StartInfo.RedirectStandardOutput = true;
            //        process.Start();
            //        string BatteryLevel = process.StandardOutput.ReadToEnd();
            //        process.WaitForExit();

            //        Console.Write("Linux: {0}\n", BatteryLevel);
            //        return float.Parse(BatteryLevel);
            //    }
            //}

            //// MacOS (sad) :(
            //else
            //{
            //    return 0.0f;
            //}
        }

        static void MonitorBatteryLevel(uint kMin, uint kMax)
        {
            Console.Write("Battery is now being monitored. You can minimise this window.\nThe system will beep if the charge needs to be plugged in or unplugged.\n");
            Console.Write("Minimum: {0}\nMaximum: {1}", kMin, kMax);
            while (true)
            {
                var battery = GetBattery();

                if (battery.ChargeLevel < kMin && battery.Status != BatteryStatuses.PluggedIn)
                {
                    BeepMinimum = true;
                    AudioBeep();
                }
                else if (battery.ChargeLevel > kMax && battery.Status == BatteryStatuses.PluggedIn)
                {
                    BeepMaximum = true;
                    AudioBeep();
                }
                else
                {
                    BeepMaximum = false;
                    BeepMinimum = false;
                }

                System.Threading.Thread.Sleep(15000);
            }

        }

        static bool BeepMinimum = false;
        static bool BeepMaximum = false;

        static void AudioBeep()
        {
            var beepTimes = 0;
            if (BeepMinimum)
            {
                beepTimes = 2;
                for (int i = 0; i < beepTimes; i++)
                {
                    Console.Beep();
                    System.Threading.Thread.Sleep(400);
                }
                BeepMinimum = false;
            }
            else if (BeepMaximum)
            {
                beepTimes = 3;
                for (int i = 0; i < beepTimes; i++)
                {
                    Console.Beep();
                    System.Threading.Thread.Sleep(400);
                }
                BeepMaximum = false;
            }
        }

        enum BatteryStatuses
        {
            Battery = 1,
            PluggedIn
        }

        struct Battery
        {
            private int chargeLevel;
            private BatteryStatuses status;
            private string stringStatus;

            public readonly int ChargeLevel
            {
                get { return chargeLevel; }
            }
            public readonly BatteryStatuses Status
            {
                get { return status; }
            }

            public readonly string StringStatus
            {
                get { return stringStatus; }
            }

            public void SetChargeLevel(int level)
            {
                chargeLevel = level;
            }

            public void SetStatus(BatteryStatuses status)
            {
                this.status = status;
                
                switch (status)
                {
                    case BatteryStatuses.Battery:
                        {
                            stringStatus = "Battery";
                            break;
                        }
                    case BatteryStatuses.PluggedIn:
                        {
                            stringStatus = "Plugged In";
                            break;
                        }
                    default:
                        break;
                }
            }

            public override string ToString()
            {
                return String.Format("Charge Level: {0}\nStatus: {1} ({2})", ChargeLevel, StringStatus, (int)Status);
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Battery Warnings - Monitor the laptop's battery level and beep when the battery falls or reaches a certain level.\n\n");

            var min = GetMinimumBatteryLevel();
            var max = GetMaximumBatteryLevel();
            Console.Clear();
            MonitorBatteryLevel(min, max);
            Environment.Exit(0);
        }
    }
}
