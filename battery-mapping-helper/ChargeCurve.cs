using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace battery_mapping_helper
{
    public enum ChargeDirection
    {
        Discharge,
        Charge
    }

    public class UniformVoltageStepChargeCurve
    {
        public double StartV { get; set; }
        public double EndV { get; set; }
        public double StepV { get; set; }
        public int RoundingDigitsV { get; set; }

        public double StartFraction { get; set; }
        public double EndFraction { get; set; }

        public ChargeDirection Direction { get; set; }

        public List<double> CalculateFractions(List<double> knownVolts, List<double> knownFractions)
        {
            if (knownVolts.Count != knownFractions.Count) throw new ArgumentOutOfRangeException();

            knownVolts.Sort();
            knownFractions.Sort();
            knownVolts[0] = Math.Round(knownVolts[0], RoundingDigitsV);
            for (int i = 1; i < knownVolts.Count; i++)
            {
                knownVolts[i] = Math.Round(knownVolts[i], RoundingDigitsV);
                double diff = knownVolts[i] - knownVolts[i - 1];

                if (Math.Abs(diff) >= StepV) continue;

                knownVolts.RemoveAt(i);
                knownFractions.RemoveAt(i);
                i--;
            }

            double minKnownVolts = knownVolts.First();
            double maxKnownVolts = knownVolts.Last();
            double minKnownFracts = knownFractions.First();
            double maxKnownFracts = knownFractions.Last();

            List<double> fractions = new((int)((EndV - StartV) / StepV) + 1);

            double currentVolts = StartV;
            while (currentVolts < minKnownVolts)
            {
                fractions.Add(StartFraction + (minKnownFracts - StartFraction) * (currentVolts - StartV) / (minKnownVolts - StartV));
                currentVolts += StepV;
            }

            for (int i = 0; i < knownVolts.Count - 1; i++)
            {
                double stepStartV = knownVolts[i];
                double stepEndV = knownVolts[i + 1];
                double stepStartFract = knownFractions[i];
                double stepEndFract = knownFractions[i + 1];

                while (currentVolts < stepEndV)
                {
                    fractions.Add(stepStartFract + (stepEndFract - stepStartFract) * (currentVolts - stepStartV) / (stepStartV - stepEndV));

                    currentVolts += StepV;
                }
            }

            while (currentVolts < EndV)
            {
                fractions.Add(EndFraction - (EndFraction - maxKnownFracts) * (EndV - currentVolts) / (EndV - maxKnownVolts));
                currentVolts += StepV;
            }

            switch (Direction)
            {
                case ChargeDirection.Discharge:
                    fractions.Reverse();
                    break;
                default:
                    break;
            }

            return fractions;
        }
    }
}
