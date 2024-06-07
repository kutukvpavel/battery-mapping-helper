using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace battery_mapping_helper
{
    class Program
    {
        static void WriteOutputFile(string path, List<double> contents, int roundingDigits)
        {
            File.WriteAllLines(Path.Combine(Environment.CurrentDirectory, path), contents.Select(x => x.ToString($"F{roundingDigits}", CultureInfo.InvariantCulture) + ","));
        }

        static void Main(string[] args)
        {
            List<double> chargeVolts = new ();
            List<double> chargeFracts = new ();
            List<double> dischargeVolts = new();
            List<double> dischargeFracts = new();

            var source = File.ReadLines(@"C:\Users\Photo\Documents\LiFePO4_600mAh_mapping.csv").Skip(1);
            foreach (var line in source)
            {
                string[] cells = line.Split(';');
                if (cells[0].Length > 0 && cells[1].Length > 0)
                {
                    dischargeVolts.Add(double.Parse(cells[0], NumberStyles.Float));
                    dischargeFracts.Add(double.Parse(cells[1], NumberStyles.Float));
                }
                if (cells[2].Length > 0 && cells[3].Length > 0)
                {
                    chargeVolts.Add(double.Parse(cells[2], NumberStyles.Float));
                    chargeFracts.Add(double.Parse(cells[3], NumberStyles.Float));
                }
            }

            UniformVoltageStepChargeCurve chargeCurve = new()
            {
                Direction = ChargeDirection.Charge,
                StartV = 2.700,
                EndV = 3.723,
                StepV = 0.001,
                StartFraction = 0,
                EndFraction = 1,
                RoundingDigitsV = 3
            };
            UniformVoltageStepChargeCurve dischargeCurve = new()
            {
                Direction = ChargeDirection.Charge,
                StartV = 2.700,
                EndV = 3.723,
                StepV = 0.001,
                StartFraction = 0,
                EndFraction = 1,
                RoundingDigitsV = 3
            };

            var chargeRes = chargeCurve.CalculateFractions(chargeVolts, chargeFracts).Select(x => x >= 0 ? 100 * x : 0).ToList();
            chargeRes.Sort();
            var dischargeRes = dischargeCurve.CalculateFractions(dischargeVolts, dischargeFracts).Select(x => x >= 0 ? 100 * x : 0).ToList();
            dischargeRes.Sort();

            WriteOutputFile("charge.csv", chargeRes, 4);
            WriteOutputFile("discharge.csv", dischargeRes, 4);
        }
    }
}
