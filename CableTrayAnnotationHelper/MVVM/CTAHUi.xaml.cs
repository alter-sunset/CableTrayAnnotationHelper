using Autodesk.Revit.DB;
using System.Windows;
using System.ComponentModel;
using System.Collections.Generic;
using CableTrayAnnotationHelper.Events;

namespace CableTrayAnnotationHelper.MVVM
{
    public partial class ViewCTAH : Window
    {
        public ViewCTAH(EventHandlerCTAH eventHandler, Dictionary<Family, List<FamilySymbol>> families)
        {
            InitializeComponent();
            DataContext = new ViewModelCTAH(eventHandler, families);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            DataContext = null;
            base.OnClosing(e);
        }
    }
}