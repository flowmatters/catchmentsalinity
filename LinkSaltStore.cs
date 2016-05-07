using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiverSystem;
using TIME.Core;
using TIME.Core.Metadata;

namespace CatchmentSalinity
{
    public class LinkSaltStore : LinkSourceSinkModel
    {
        [Parameter,CalculationUnits(CommonUnits.kilograms)] public double InitialStore { get; set; }
        [Parameter,CalculationUnits(CommonUnits.kgPerM3)] public double DepletionRate { get; set; }
        [State, CalculationUnits(CommonUnits.kilograms)] public double Store { get; private set; }

        public override void runTimeStep(DateTime Now, double theTimeStepInSeconds)
        {
            Total_Constituent_Out = DepletionRate * CatchmentInflowVolume;
            Store -= Total_Constituent_Out;
        }

        public override void reset()
        {
            Store = InitialStore;
            base.reset();
        }
    }
}
