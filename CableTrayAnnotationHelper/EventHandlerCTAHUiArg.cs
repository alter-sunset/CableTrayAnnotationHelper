using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace CableTrayAnnotationHelper
{
    public class EventHandlerCTAHUiArg : RevitEventWrapper<CTAHUi>
    {
        public override void Execute(UIApplication uiApp, CTAHUi ui)
        {
            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document mainDocument = uIDocument.Document;
            View view = uIDocument.ActiveGraphicalView;

            Family familyDetail = ui.Family;
            FamilySymbol symbolConduit = ui.SymbolConduit;
            FamilySymbol symbolCableTray = ui.SymbolCableTray;

            List<ParameterAssociation> paramsTable =
            [
                new(){ParameterIn = "Марка", ParameterOut = "ADSK_Примечание", ParameterType = ParameterType.Id},
                new(){ParameterIn = "Этаж", ParameterOut = "Этаж", ParameterType = ParameterType.String},
                new(){ParameterIn = "Параметры фильтрации", ParameterOut = "Параметры фильтрации", ParameterType = ParameterType.String},
                new(){ParameterIn = "Высота", ParameterOut = "ADSK_Размер_Высота", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Длина", ParameterOut = "ADSK_Размер_Длина", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Ширина", ParameterOut = "ADSK_Размер_Ширина", ParameterType = ParameterType.Double}
            ];

            List<Document> links = Utils.GetLinkedDocuments(mainDocument);

            List<FamilyInstance> existingDetailLinesCableTray
                = Utils.ExistingDetailLines(mainDocument, view, familyDetail, symbolCableTray);

            List<FamilyInstance> existingDetailLinesConduit
                = Utils.ExistingDetailLines(mainDocument, view, familyDetail, symbolConduit);

            try
            {
                //problem with viewId, it seems i need to provide a view from a linked document, not main document
                using Transaction transaction = new(mainDocument);
                transaction.Start("Размещение аннотаций лотков и коробов");

                symbolConduit.Activate();
                symbolCableTray.Activate();

                foreach (Document linkedDocument in links)
                {
                    if (linkedDocument is null)
                        continue;

                    if ((bool)ui.CheckBoxConduit.IsChecked)
                        Utils.PlaceTheLines(mainDocument, linkedDocument, view,
                            BuiltInCategory.OST_Conduit, existingDetailLinesConduit, symbolConduit,
                            paramsTable);

                    if ((bool)ui.CheckBoxCableTray.IsChecked)
                        Utils.PlaceTheLines(mainDocument, linkedDocument, view,
                            BuiltInCategory.OST_CableTray, existingDetailLinesCableTray, symbolCableTray,
                            paramsTable);

                    if (!(bool)ui.CheckBoxConduit.IsChecked && !(bool)ui.CheckBoxCableTray.IsChecked)
                        transaction.RollBack();
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.ToString());
            }
        }
    }
}