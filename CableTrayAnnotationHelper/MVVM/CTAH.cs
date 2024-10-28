using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using CableTrayAnnotationHelper.Events;

namespace CableTrayAnnotationHelper.MVVM
{
    [Transaction(TransactionMode.Manual)]
    public class CTAH : IExternalCommand
    {
        private static Window _view;
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                ShowForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
        private static void ShowForm(UIApplication uiApp)
        {
            CloseCurrentForm();
            EventHandlerCTAH eventHandler = new();

            Document document = uiApp.ActiveUIDocument.Document;

            Dictionary<Family, FamilySymbol[]> families = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e.get_Parameter(BuiltInParameter.FAMILY_LINE_LENGTH_PARAM) is not null)
                .Cast<FamilySymbol>()
                .GroupBy(e => e.Family, new FamilyComparer())
                .ToDictionary(e => e.Key, e => e.ToArray());

            _view = new ViewCTAH(eventHandler, families);
            _view.Show();
        }
        private static void CloseCurrentForm()
        {
            if (_view is null) return;
            _view.Close();
            _view = null;
        }
    }

    public class CTAHCommand_Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categorySet) => false;
    }
}