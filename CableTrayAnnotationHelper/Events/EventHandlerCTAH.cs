using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;
using System.Collections.Generic;
using CableTrayAnnotationHelper.MVVM;

namespace CableTrayAnnotationHelper.Events
{
    public class EventHandlerCTAH : RevitEventWrapper<ViewModelCTAH>
    {
        public override void Execute(UIApplication uiApp, ViewModelCTAH viewModel)
        {
            if (!viewModel.IsEverythingFilled())
            {
                MessageBox.Show("Заполните необходимые поля!");
                return;
            }

            UIDocument uIDocument = uiApp.ActiveUIDocument;
            Document mainDocument = uIDocument.Document;
            View view = uIDocument.ActiveGraphicalView;

            //TODO: move parameters initialization to viewModel
            List<ParameterAssociation> paramsTable =
            [
                new(){ParameterIn = "Марка", ParameterOut = "ADSK_Примечание", ParameterType = ParameterType.Id},
                new(){ParameterIn = "Этаж", ParameterOut = "Этаж", ParameterType = ParameterType.String},
                new(){ParameterIn = "Параметры фильтрации", ParameterOut = "Параметры фильтрации", ParameterType = ParameterType.String},
                new(){ParameterIn = "Высота", ParameterOut = "ADSK_Размер_Высота", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Длина", ParameterOut = "ADSK_Размер_Длина", ParameterType = ParameterType.Double},
                new(){ParameterIn = "Ширина", ParameterOut = "ADSK_Размер_Ширина", ParameterType = ParameterType.Double}
            ];

            List<RevitLinkInstance> links = mainDocument.GetLinkedDocuments();

            if (links is null)
            {
                TaskDialog.Show("Error", "There are no links in the model");
                return;
            }

            Family familyDetail = viewModel.SelectedFamily;
            FamilySymbol symbolConduit = null;
            List<FamilyInstance> existingDetailLinesConduit = [];
            FamilySymbol symbolCableTray = null;
            List<FamilyInstance> existingDetailLinesCableTray = [];

            bool includeConduit = viewModel.IncludeConduit;
            if (includeConduit)
            {
                symbolConduit = viewModel.SelectedConduit;
                existingDetailLinesConduit =
                    mainDocument.GetExistingDetailLines(view, familyDetail, symbolConduit);
            }

            bool includeCableTray = viewModel.IncludeCableTray;
            if (includeCableTray)
            {
                symbolCableTray = viewModel.SelectedCableTray;
                existingDetailLinesCableTray =
                    mainDocument.GetExistingDetailLines(view, familyDetail, symbolCableTray);
            }

            try
            {
                using Transaction transaction = new(mainDocument);
                transaction.Start("Размещение аннотаций лотков и коробов");

                symbolConduit?.Activate();
                symbolCableTray?.Activate();

                foreach (RevitLinkInstance link in links)
                {
                    if (link is null) continue;

                    if (includeConduit
                        && !mainDocument.TryPlaceLines(link, view, BuiltInCategory.OST_Conduit,
                            existingDetailLinesConduit, symbolConduit, paramsTable))
                        continue;

                    if (includeCableTray //TODO: Create class to hold parameters
                        && !mainDocument.TryPlaceLines(link, view, BuiltInCategory.OST_CableTray,
                            existingDetailLinesCableTray, symbolCableTray, paramsTable))
                        continue;
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", $"An error occurred: {ex.Message}");
            }
        }
    }
}