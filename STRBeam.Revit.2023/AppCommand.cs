using Autodesk.Revit.UI;
using STRBeam.Revit.FirstButton;
using System;
using System.Linq;

namespace STRBeam.Revit
{
    internal class AppCommand : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Create Tab Name
                application.CreateRibbonTab("STRBeam");
            }
            catch (Exception)
            {
                // Ingored
            }

            // Create Panel Name
            var ribbonPanel = application.GetRibbonPanels("STRBeam").FirstOrDefault(x => x.Name == "STRBeam") ??
                application.CreateRibbonPanel("STRBeam", "Structure");

            //Create buttons
            JoinBeamWithFloorCmd.CreateButton(ribbonPanel);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
