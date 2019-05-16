﻿namespace FerramAerospaceResearch.FARGUI.FAREditorGUI.Simulation
{
    internal class StabilityDerivExportVariables
    {
        public double craftmass;

        public double envpressure; // in kPa
        public double envtemperature; // in Kelvin
        public double envdensity;
        public double envsoundspeed;
        public double envg;

        public double sitmach;
        public double sitdynpres;
        public double siteffg; // local gravity corrected for speed
    }

    internal class StabilityDerivExportOutput
    {
        public StabilityDerivOutput outputvals;
        public StabilityDerivExportVariables exportvals;

        public StabilityDerivExportOutput(StabilityDerivOutput outputvalues, StabilityDerivExportVariables exportvalues)
        {
            outputvals = outputvalues;
            exportvals = exportvalues;
        }
    }
}
