using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace CableTrayAnnotationHelper
{
    [Transaction(TransactionMode.Manual)]
    public class CTAH : IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                App.ThisApp.ShowFormCTAH(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    public class CTAHCommand_Availability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication uiApp, CategorySet categorySet) => false;
    }
}