using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using View = Autodesk.Revit.DB.View;

namespace ADM.CableTrayAnnotationHelper
{
    public class EventHandlerCTAHUiArg : RevitEventWrapper<CTAHUi>
    {
        public override void Execute(UIApplication uiApp, CTAHUi ui)
        {
            DateTime start = DateTime.Now;

            Application application = uiApp.Application;
            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document document = uIDocument.Document;
            View view = uIDocument.ActiveGraphicalView;

            List<Element> cableTrays = new FilteredElementCollector(document, view.Id)
                .OfCategory(BuiltInCategory.OST_CableTray)
                .Where(e => e != null)
                .ToList();

            //missing conduit methods

            string paramIdName = "ADSK_Марка";
            string familySymbolName = "Test_shit";

            FamilySymbol familySymbol = new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e != null)
                .FirstOrDefault(e => e.Name == familySymbolName) as FamilySymbol;

            List<Element> existingDetailLines = new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e.Name == familySymbolName)
                .Where(e => e != null)
                .ToList();

            /*using (Transaction transactionCheck = new Transaction(document))
            {
                transactionCheck.Start("Check existing");

                foreach (Element cableTray in cableTrays)
                {
                    if (cableTray is null)
                    {
                        continue;
                    }

                    if (existingDetailLines.
                            Any(e => e.LookupParameter(paramIdName).AsString() == cableTray.UniqueId))
                    {
                        Element existingDetailLine = existingDetailLines
                                .FirstOrDefault(e => e.LookupParameter(paramIdName).AsString() == cableTray.UniqueId);

                        if (!Utils.AreAligned(cableTray, existingDetailLine))
                        {
                            //doesn't work
                            document.Delete(existingDetailLine.Id);
                        }
                    }
                }

                transactionCheck.Commit();
            }*/

            using (Transaction transaction = new Transaction(document))
            {
                transaction.Start("Add annotations");

                familySymbol.Activate();

                foreach (Element cableTray in cableTrays)
                {
                    if (cableTray is null
                        || existingDetailLines.
                            Any(e => e.LookupParameter(paramIdName).AsString() == cableTray.UniqueId))
                    {
                        continue;
                    }

                    Utils.PlaceNewDetailLine(document, view, familySymbol, cableTray, paramIdName);
                }

                transaction.Commit();
            }

            MessageBox.Show($"Готово! Затраченное время:{DateTime.Now - start}");
        }
    }
}
