using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Document = Autodesk.Revit.DB.Document;
using View = Autodesk.Revit.DB.View;

namespace ADM.CableTrayAnnotationHelper
{
    public class EventHandlerCTAHUiArg : RevitEventWrapper<CTAHUi>
    {
        public override void Execute(UIApplication uiApp, CTAHUi ui)
        {
            Application application = uiApp.Application;
            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document mainDocument = uIDocument.Document;
            View view = uIDocument.ActiveGraphicalView;

            Family familyDetail = ui.Family;
            FamilySymbol symbolConduit = ui.SymbolConduit;
            FamilySymbol symbolCableTray = ui.SymbolCableTray;

            string paramIdName = "ADSK_Марка";

            List<Document> links = new FilteredElementCollector(mainDocument)
                .OfClass(typeof(RevitLinkInstance))
                .Select(l => l as RevitLinkInstance)
                .Select(l => l.GetLinkDocument())
                .Where(d => d != null)
                .ToList();

            List<Element> existingDetailLines = new FilteredElementCollector(mainDocument, view.Id)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e != null)
                .Where(e => e.Name == symbolCableTray.Name || e.Name == symbolConduit.Name)
                .ToList();

            using (Transaction transaction = new Transaction(mainDocument))
            {
                transaction.Start("Размещение аннотаций лотков и коробов");

                symbolConduit.Activate();
                symbolCableTray.Activate();

                foreach (Document linkedDocument in links)
                {
                    if (linkedDocument is null)
                    {
                        continue;
                    }

                    if ((bool)ui.CheckBoxConduit.IsChecked)
                    {
                        using (FilteredElementCollector collector = new FilteredElementCollector(linkedDocument, view.Id))
                        {
                            List<Element> conduits = collector
                                .OfCategory(BuiltInCategory.OST_Conduit)
                                .Where(e => e != null)
                                .ToList();

                            foreach (Element conduit in conduits)
                            {
                                if (conduit is null
                                    || existingDetailLines.
                                        Any(e => e.LookupParameter(paramIdName).AsString() == conduit.UniqueId))
                                {
                                    continue;
                                }

                                Utils.PlaceNewDetailLine(mainDocument, view, symbolConduit, conduit, paramIdName);
                            }
                        }
                    }

                    if ((bool)ui.CheckBoxCableTray.IsChecked)
                    {
                        using (FilteredElementCollector collector1 = new FilteredElementCollector(linkedDocument, view.Id))
                        {
                            List<Element> cableTrays = collector1
                                .OfCategory(BuiltInCategory.OST_CableTray)
                                .Where(e => e != null)
                                .ToList();

                            foreach (Element cableTray in cableTrays)
                            {
                                if (cableTray is null
                                    || existingDetailLines.
                                        Any(e => e.LookupParameter(paramIdName).AsString() == cableTray.UniqueId))
                                {
                                    continue;
                                }

                                Utils.PlaceNewDetailLine(mainDocument, view, symbolCableTray, cableTray, paramIdName);
                            }
                        }
                    }
                    if (!(bool)ui.CheckBoxConduit.IsChecked && !(bool)ui.CheckBoxCableTray.IsChecked)
                    {
                        transaction.RollBack();
                    }
                }

                transaction.Commit();
            }
        }
    }
}
