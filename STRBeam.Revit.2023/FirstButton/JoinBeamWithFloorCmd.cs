using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using STRBeam.Revit.Utilities;
using System.Reflection;

namespace STRBeam.Revit.FirstButton
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    internal class JoinBeamWithFloorCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;

            using (TransactionGroup transGr = new TransactionGroup(doc))
            {
                transGr.Start("Auto join Beam with Floor");

                var viewModel = new JoinBeamWithFloorViewModel(uidoc);
                var window = new JoinBeamWithFloorWindow(viewModel);

                bool? showDialog = window.ShowDialog();

                if (showDialog == null || showDialog == false)
                {
                    transGr.RollBack();
                    return Result.Cancelled;
                }
                transGr.Assimilate();
                return Result.Succeeded;
            }
        }

        public static void CreateButton(RibbonPanel panel)
        {
            var assembly = Assembly.GetExecutingAssembly();

            panel.AddItem(
                new PushButtonData(
                    MethodBase.GetCurrentMethod().DeclaringType?.Name,
                    "STRBeam",
                    assembly.Location,
                    MethodBase.GetCurrentMethod().DeclaringType?.FullName)
                {
                    ToolTip = "Join beam with floor",
                    LargeImage = ImageUtils.LoadImage(assembly, "32x32.strbeam.png")
                });
        }
    }
}