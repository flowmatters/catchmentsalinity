using System;
using PerfectGWLag.Models.GWLag;
using RiverSystem;
using RiverSystem.Catchments.Models.CatchmentModels;
using TIME.Core;
using TIME.Core.Metadata;

namespace CatchmentSalinity
{
    public class SubcatchmentSalinityBalance : AbstractCatchmentModel
    {
        [Parameter,Aka("Name of Salt Constituent")]
        public string ConstituentName { get; set; }

        [Output, Aka("Salt Export from Groundwater"),CalculationUnits(CommonUnits.kilograms)]
        public double GroundwaterSaltExport { get; private set; }

        [Output, CalculationUnits(CommonUnits.kilograms)]
        public double SaltDischarge { get; set; }

        [Output,Aka("Groundwater Flow"),CalculationUnits(CommonUnits.cubicMetres)]
        public double GroundwaterFlow { get; private set; }

        [State]
        [Aka("Groundwater discharge to river (m3/timestep)")]
        public double GroundwaterDischargeToRiver { get { return groundwaterModel.groundwaterDischargeToRiver; } }

        [State]
        [Aka("Lateral flow discharge to river (m3/timestep)")]
        public double LateralFlowDischargeToRiver {
            get { return groundwaterModel.lateralFlowDischargeToRiver; }
        }

        [State, CalculationUnits(CommonUnits.kilograms)]
        public double SaltStore { get; set; }

        [Input]
        public double SurfaceWaterSaltExport { get; set; }

        public SubcatchmentSalinityBalance()
        {
            groundwaterModel = new GWlag();
        }

        private GWlag groundwaterModel;

        [
            Parameter,
            Minimum(0.0),
            Maximum(double.MaxValue),
            DecimalPlaces(1),
            Aka("Calibration parameter Alpha (groundwater delay)"),
            CalculationUnits(CommonUnits.none)
        ]
        public double Alpha
        {
            get { return groundwaterModel.alpha; }
            set { groundwaterModel.alpha = value; }
        }

        [
            Parameter,
            Minimum(1.0),
            Maximum(double.MaxValue),
            DecimalPlaces(1),
            Aka("Scalling factor gamma"),
            CalculationUnits(CommonUnits.none)
        ]
        public double Gamma
        {
            get { return groundwaterModel.gamma; }
            set { groundwaterModel.gamma = value; }
        }

        [
            Parameter,
            Minimum(0.00),
            Maximum(double.MaxValue),
            DecimalPlaces(4),
            Aka("Remove delta groundwater"),
            CalculationUnits(CommonUnits.cubicMetres)
        ]
        public double Delta
        {
            get { return groundwaterModel.delta; }
            set { groundwaterModel.delta = value; }
        }

        [
            Parameter,
            Minimum(0.0000),
            Maximum(1.0),
            DecimalPlaces(4),
            Aka("Storativity"),
            CalculationUnits(CommonUnits.none)
        ]
        public double Storativity
        {
            get { return groundwaterModel.storativity; }
            set { groundwaterModel.storativity = value; }
        }

        [
            Parameter,
            Minimum(0.0000),
            Maximum(double.MaxValue),
            DecimalPlaces(4),
            Aka("Groundwater flow length (m)"),
            CalculationUnits(CommonUnits.metres)
        ]
        public double GroundwaterFlowLength
        {
            get { return groundwaterModel.groundwaterFlowLength; }
            set { groundwaterModel.groundwaterFlowLength = value; }
        }

        [
            Parameter,
            Minimum(0.0000),
            Maximum(double.MaxValue),
            DecimalPlaces(4),
            Aka("Saturated hydraulic conductivity (m/day)"),
            CalculationUnits(CommonUnits.metresPerDay)
        ]
        public double SaturatedHydraulConductivity
        {
            get { return groundwaterModel.saturatedHydraulicConductivity; }
            set { groundwaterModel.saturatedHydraulicConductivity = value; }
        }

        [
            Parameter,
            Minimum(0.0000),
            Maximum(double.MaxValue),
            DecimalPlaces(4),
            Aka("Change in elevation of land surface (m) "),
            CalculationUnits(CommonUnits.metres)
        ]
        public double ChangeInElevation
        {
            get { return groundwaterModel.changeInElevation; }
            set { groundwaterModel.changeInElevation = value; }
        }

        [
            Parameter,
            Minimum(0.0000),
            Maximum(double.MaxValue),
            DecimalPlaces(4),
            Aka("Aquifer thickness (m)"),
            CalculationUnits(CommonUnits.metres)
        ]
        public double AquiferThickness
        {
            get { return groundwaterModel.aquiferThickness; }
            set { groundwaterModel.aquiferThickness = value; }
        }

        [Parameter, Minimum(0.0), Maximum(2.0),Aka("Deep Drainage Scaling Factor (P)")]
        public double P { get; set; }

        [Parameter, Minimum(0.0), Maximum(1000.0), CalculationUnits(CommonUnits.kgPerM3),Aka("Groundwater Salt Concentration")]
        public double ConcentrationGroundwater { get; set; }

        [Parameter, CalculationUnits(CommonUnits.kgPerM3)]
        public double DPR { get; set; }

        [
            Ignore,
            Minimum(0.00),
            Maximum(double.MaxValue),
            DecimalPlaces(2),
            Aka("Area (m2)"),
            CalculationUnits(CommonUnits.squareMetres)
        ]
        public double Area
        {
            get { return groundwaterModel.area; }
            private set { groundwaterModel.area = value; }
        }

        [Ignore] public double BasementSlope { get { return groundwaterModel.basementSlope; } }
        [Ignore] public double TimeScale { get { return groundwaterModel.TimeScale; } }

        public override void runTimeStep(DateTime Now, double theTimeStepInSeconds)
        {
            groundwaterModel.recharge = P * Subcatchment.Slowflow; // Equation 1
            groundwaterModel.runTimeStep(); // All GW Lag Stuff

            GroundwaterFlow = groundwaterModel.groundwaterDischargeToRiver; // Simplified Eq 10
        }

        public void RunSaltBalance()
        {
            GroundwaterSaltExport = GroundwaterFlow*ConcentrationGroundwater; // Eq 11
            SaltStore = SaltStore + GroundwaterSaltExport + SurfaceWaterSaltExport;
            double CatchmentDischarge = (Subcatchment.Quickflow + Subcatchment.Slowflow)*UnitConversion.SECONDS_IN_ONE_DAY;
            SaltDischarge = Math.Min(CatchmentDischarge*DPR, SaltStore);
            SaltStore = SaltStore - SaltDischarge;
        }

        public override void reset()
        {
            groundwaterModel.reset();
            base.reset();
            Area = Subcatchment.characteristics.areaInSquareMeters;
            SaltStore = 0;
        }
    }
}
