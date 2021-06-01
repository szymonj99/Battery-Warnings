using System;
using System.Management;
using System.Runtime.InteropServices;

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

        static float GetBatteryLevel()
        {
            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining FROM Win32_Battery");
                object BatteryLevel = null;

                if (mos.Get().Count == 1)
                {
                    foreach (var mo in mos.Get())
                    {
                        BatteryLevel = mo["EstimatedChargeRemaining"];
                    }
                }
                Console.Write("Windows: {0}\n", BatteryLevel);
                return float.Parse(BatteryLevel.ToString());
            }

            // Linux
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "cat";
                    process.StartInfo.Arguments = @"/sys/class/power_supply/BAT0/capacity";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    string BatteryLevel = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    Console.Write("Linux: {0}\n", BatteryLevel);
                    return float.Parse(BatteryLevel);
                }
            }

            // MacOS (sad) :(
            else
            {
                return 0.0f;
            }
        }

        static void MonitorBatteryLevel(uint kMin, uint kMax)
        {            
            while (true)
            {
                var BatteryLevel = GetBatteryLevel();
                Console.Write("Float: {0}", BatteryLevel);

                if (BatteryLevel < kMin && !ToggledMinimum)
                {
                    ToggledMinimum = true;
                    ToggledMaximum = true;
                    BeepMinimum = true;
                    AudioBeep();
                }
                else if (BatteryLevel > kMax && !ToggledMaximum)
                {
                    ToggledMaximum = true;
                    ToggledMinimum = false;
                    BeepMaximum = true;
                    AudioBeep();
                }

                if (BatteryLevel > kMin && BatteryLevel < kMax)
                {
                    BeepMaximum = false;
                    BeepMinimum = false;
                    ToggledMaximum = false;
                    ToggledMinimum = false;
                }

                System.Threading.Thread.Sleep(60000);
            }

        }

        static bool ToggledMinimum = false;
        static bool ToggledMaximum = false;

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
                }
                BeepMinimum = false;
            }
            else if (BeepMaximum)
            {
                beepTimes = 3;
                for (int i = 0; i < beepTimes; i++)
                {
                    Console.Beep();
                }
                BeepMaximum = false;
            }
        }


        static void Main(string[] args)
        {
            Console.Write("Battery Warnings - Monitor the laptop's battery level and beep when the battery falls or reaches a certain level.\n\n");

            var min = GetMinimumBatteryLevel();
            var max = GetMaximumBatteryLevel();
            MonitorBatteryLevel(min, max);
        }
    }
}
