using Autodesk.Revit.DB;
using LearnRevitAPI.Lib;
using STRBeam.Revit.FirstButton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace STRBeam.Revit
{
    /// <summary>
    /// Interaction logic for JoinBeamWithFloorWindow.xaml
    /// </summary>
    public partial class JoinBeamWithFloorWindow : Window
    {
        private JoinBeamWithFloorViewModel _viewModel;
        private TransactionGroup transG;
        public JoinBeamWithFloorWindow(JoinBeamWithFloorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;

            DataContext = _viewModel;

            transG = new TransactionGroup(_viewModel.Doc);
        }

        public bool DialogResult { get; private set; }

        internal bool ShowDailog()
        {
            throw new NotImplementedException();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            if (transG.HasStarted())
            {
                transG.RollBack();
                MessageBox.Show("Progress is Cancel!", "Stop Progress", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            #region Get all elements to run

            // Get floor to join with beam
            var allFloors = new List<Element>();

            if (_viewModel.IsEntireProject)
            {
                allFloors = new FilteredElementCollector(_viewModel.Doc)
                                .OfClass(typeof(Floor))
                                .WhereElementIsNotElementType()
                                .ToList();
            }
            else if (_viewModel.IsCurrentView)
            {
                allFloors = new FilteredElementCollector(_viewModel.Doc, _viewModel.Doc.ActiveView.Id)
                    .OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .ToList();
            }
            else
            {
                allFloors = new FilteredElementCollector(_viewModel.Doc, _viewModel.UiDoc.Selection.GetElementIds())
                    .OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .ToList();
            }
            #endregion

            // Setup progress bar
            var ProgressBar = new ProgressBar();
            ProgressBar.Maximum = allFloors.Count();
            double valuePercent = 0;

            transG.Start("Run Process");

            foreach (Element floor in allFloors)
            {
                if (transG.HasStarted())
                {
                    valuePercent += 1;
                    _viewModel.Percent = valuePercent / ProgressBar.Maximum * 100;

                    // Change number when run program
                    ProgressBar.Dispatcher.Invoke(() => ProgressBar.Value = valuePercent, DispatcherPriority.Background);

                    #region Get all beams touch floor on active view
                    var structuralFraming = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);

                    var floorBox = floor.get_BoundingBox(_viewModel.Doc.ActiveView);
                    var floorOutline = new Outline(floorBox.Min, floorBox.Max);

                    // Get all elements touch floor's bounding box
                    var bbFilter = new BoundingBoxIntersectsFilter(floorOutline);

                    // Get structural framing elements touch floor's bounding box
                    var beamTouchFloor = new LogicalAndFilter(structuralFraming, bbFilter);

                    var allBeams = new FilteredElementCollector(_viewModel.Doc, _viewModel.Doc.ActiveView.Id)
                                                 .WherePasses(beamTouchFloor)
                                                 .ToList();
                    #endregion

                    using (Transaction trans = new Transaction(_viewModel.Doc))
                    {
                        trans.Start("Join Beam With Floor");

                        // Delete warning
                        var warningSuper = new DeleteWarningSuper();
                        var failOpt = trans.GetFailureHandlingOptions();
                        failOpt.SetFailuresPreprocessor(warningSuper);
                        trans.SetFailureHandlingOptions(failOpt);

                        foreach (Element beam in allBeams)
                        {
                            if (_viewModel.PriorityCategory.Equals("BEAM"))
                            {
                                if (JoinGeometryUtils.AreElementsJoined(_viewModel.Doc, beam, floor) == false)
                                {
                                    JoinGeometryUtils.JoinGeometry(_viewModel.Doc, beam, floor);

                                    if (JoinGeometryUtils.IsCuttingElementInJoin(_viewModel.Doc, beam, floor) == false)
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(_viewModel.Doc, beam, floor);
                                    }
                                }
                                else
                                {
                                    if (JoinGeometryUtils.IsCuttingElementInJoin(_viewModel.Doc, beam, floor) == false)
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(_viewModel.Doc, beam, floor);
                                    }
                                }
                            }
                            else
                            {
                                if (JoinGeometryUtils.AreElementsJoined(_viewModel.Doc, floor, beam) == false)
                                {
                                    JoinGeometryUtils.JoinGeometry(_viewModel.Doc, floor, beam);

                                    if (JoinGeometryUtils.IsCuttingElementInJoin(_viewModel.Doc, floor, beam) == false)
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(_viewModel.Doc, floor, beam);
                                    }
                                }
                                else
                                {
                                    if (JoinGeometryUtils.IsCuttingElementInJoin(_viewModel.Doc, floor, beam) == false)
                                    {
                                        JoinGeometryUtils.SwitchJoinOrder(_viewModel.Doc, floor, beam);
                                    }
                                }
                            }

                        }

                        trans.Commit();
                    }
                }
                else
                {
                    break;
                }
            }

            if (transG.HasStarted())
            {
                transG.Commit();
                DialogResult = true;

                MessageBox.Show("Auto Join Beam with Floor was successful!", "Auto Join Successful",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void JoinBeamWithFloorWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (transG.HasStarted())
            {
                transG.RollBack();
                MessageBox.Show("Progress is Cancel!", "Stop Progress", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }
    }
}