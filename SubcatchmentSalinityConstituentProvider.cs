using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiverSystem;
using RiverSystem.Catchments.Models.CatchmentModels;
using RiverSystem.Constituents;
using RiverSystem.Constituents.Providers;
using TIME.Core;
using TIME.Core.Metadata;
using TIME.ManagedExtensions;

namespace CatchmentSalinity
{
    [WorksWith(typeof(SubcatchmentSalinityBalance))]
    public class SubcatchmentSalinityConstituentProvider : CatchmentModelConstituentProvider
    {
        public SubcatchmentSalinityConstituentProvider(SubcatchmentSalinityBalance model, SystemConstituentsConfiguration constituentsConfig, ElementConstituentsData processor) : base(model, constituentsConfig, processor)
        {
            SalinityModel = model;
        }

        public SubcatchmentSalinityBalance SalinityModel { get; private set; }

        public override void ProcessLumped(CatchmentConstituentProvider parentProvider, DateTime now,
            double timeStepInSeconds)
        {
            Constituent c =
                RecordedData.List.First(co => co.Constituent.Name == SalinityModel.ConstituentName).Constituent;
            double loadFromSurfaceWater = LoadFromSurfaceWater(c, parentProvider.FunctionalUnitProviders);
            ZeroSurfaceWaterLoad(c, parentProvider.FunctionalUnitProviders);
            SalinityModel.SurfaceWaterSaltExport = loadFromSurfaceWater;
            SalinityModel.RunSaltBalance();
            base.ProcessLumped(parentProvider, now, timeStepInSeconds);

            BaseCatchmentModelConstituentOutput outputs = RecordedData.Get<SubcatchmentSalinityConstituentOutput>(c);
            outputs.LocalSlowflowAdded = SalinityModel.SaltDischarge / UnitConversion.SECONDS_IN_ONE_DAY;
        }

        private void ZeroSurfaceWaterLoad(Constituent c, List<FunctionalUnitConstituentProvider> functionalUnitProviders)
        {
            foreach (FunctionalUnitConstituentProvider fup in functionalUnitProviders)
            {
                FunctionalUnitConstituentOutput relevantOutput =
                    fup.RecordedData.List
                        .FirstOrDefault(co => co.Constituent == c) as
                        FunctionalUnitConstituentOutput;

                relevantOutput.QuickFlowMass = 0;
                relevantOutput.SlowFlowMass = 0;
                relevantOutput.TotalFlowConcentration = 0;
                relevantOutput.SlowFlowConcentration = 0;
                relevantOutput.QuickFlowConcentration = 0;
            }
        }

        private double LoadFromSurfaceWater(Constituent c, List<FunctionalUnitConstituentProvider> functionalUnitProviders)
        {
            double total = 0.0;
            foreach(FunctionalUnitConstituentProvider fup in functionalUnitProviders)
            {
                FunctionalUnitConstituentOutput relevantOutput =
                    fup.RecordedData.List
                        .FirstOrDefault(co => co.Constituent == c) as FunctionalUnitConstituentOutput;

                total += relevantOutput.TotalFlowMass;
            }
            return total * UnitConversion.SECONDS_IN_ONE_DAY;
        }
    }
}
