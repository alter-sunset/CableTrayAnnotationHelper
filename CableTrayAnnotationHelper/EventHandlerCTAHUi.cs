using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace CableTrayAnnotationHelper
{
    public class EventHandlerCTAHUi : RevitEventWrapper<CTAHUi>
    {
        public override void Execute(UIApplication uiApp, CTAHUi ui)
        {
            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document mainDocument = uIDocument.Document;
            View view = uIDocument.ActiveGraphicalView;

            Family familyDetail = ui.Family;
            FamilySymbol symbolConduit = ui.SymbolConduit;
            FamilySymbol symbolCableTray = ui.SymbolCableTray;

            //TODO: move parameters initialization to ui
            List<ParameterAssociation> paramsTable =
            [
                new(){ParameterIn = "Марка", ParameterOut = "ADSK_Примечание", ParameterType = ParameterType.Id},
                new(){ParameterIn = "Этаж", ParameterOut = "Этаж", ParameterType = ParameterType.String},
                new(){ParameterIn = "Параметры фильтрации", ParameterOut = "Параметры фильтрации", ParameterType = ParameterType.String},
                new(){ParameterIn = "Высота", ParameterOut = "ADSK_Размер_Высота", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Длина", ParameterOut = "ADSK_Размер_Длина", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Ширина", ParameterOut = "ADSK_Размер_Ширина", ParameterType = ParameterType.Double}
            ];

            List<RevitLinkInstance> links = Utils.GetLinkedDocuments(mainDocument);

            if (links is null)
            {
                TaskDialog.Show("Error", "There are no links in the model");
                return;
            }

            List<FamilyInstance> existingDetailLinesCableTray
                = Utils.ExistingDetailLines(mainDocument, view, familyDetail, symbolCableTray);

            List<FamilyInstance> existingDetailLinesConduit
                = Utils.ExistingDetailLines(mainDocument, view, familyDetail, symbolConduit);

            try
            {
                using Transaction transaction = new(mainDocument);
                transaction.Start("Размещение аннотаций лотков и коробов");

                symbolConduit.Activate();
                symbolCableTray.Activate();

                bool isConduitChecked = (bool)ui.CheckBoxConduit.IsChecked;
                bool isCableTrayChecked = (bool)ui.CheckBoxCableTray.IsChecked;

                foreach (RevitLinkInstance link in links)
                {
                    if (link is null) continue;

                    if (!isConduitChecked && !isCableTrayChecked)
                        transaction.RollBack();

                    if (isConduitChecked
                        && !TryPlaceLines(mainDocument, link, view,
                        BuiltInCategory.OST_Conduit, existingDetailLinesConduit, symbolConduit, paramsTable))
                        continue;

                    if (isCableTrayChecked
                        && !TryPlaceLines(mainDocument, link, view,
                        BuiltInCategory.OST_CableTray, existingDetailLinesCableTray, symbolCableTray, paramsTable))
                        continue;
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", $"An error occurred: {ex.Message}");
            }
        }
        private static bool TryPlaceLines(Document mainDocument, RevitLinkInstance link, View view, BuiltInCategory category, List<FamilyInstance> existingLines, FamilySymbol symbol, List<ParameterAssociation> paramsTable)
        {
            Utils.PlaceTheLines(mainDocument, link, view, category, existingLines, symbol, paramsTable, out bool noLinkedView);
            return !noLinkedView;
        }
    }
}