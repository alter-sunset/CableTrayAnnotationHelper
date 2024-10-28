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

            ParameterAssociation[] paramsTable = EventHelper.GetParameterAssociations;
            RevitLinkInstance[] links = mainDocument.GetLinkedDocuments();

            if (links is null || links.Length == 0)
            {
                TaskDialog.Show("Ошибка", "Связанные модели не найдены");
                return;
            }

            Family familyDetail = viewModel.SelectedFamily;
            bool includeConduit = viewModel.IncludeConduit;
            bool includeCableTray = viewModel.IncludeCableTray;

            FamilySymbol symbolConduit = includeConduit ? viewModel.SelectedConduit : null;
            FamilySymbol symbolCableTray = includeCableTray ? viewModel.SelectedCableTray : null;

            FamilyInstance[] existingDetailLinesConduit = includeConduit
                ? mainDocument.GetExistingDetailLines(view, familyDetail, symbolConduit)
                : [];
            FamilyInstance[] existingDetailLinesCableTray = includeCableTray
                ? mainDocument.GetExistingDetailLines(view, familyDetail, symbolCableTray)
                : [];

            PlaceLinesHolder conduitHolder = new()
            {
                Document = mainDocument,
                View = view,
                BuiltInCategory = BuiltInCategory.OST_Conduit,
                ExistingDetailLines = existingDetailLinesConduit,
                FamilySymbol = symbolConduit,
                Parameters = paramsTable,
            };
            PlaceLinesHolder cableTrayHolder = new()
            {
                Document = mainDocument,
                View = view,
                BuiltInCategory = BuiltInCategory.OST_CableTray,
                ExistingDetailLines = existingDetailLinesCableTray,
                FamilySymbol = symbolCableTray,
                Parameters = paramsTable,
            };

            try
            {
                using Transaction transaction = new(mainDocument);
                transaction.Start("Размещение аннотаций лотков и коробов");

                symbolConduit?.Activate();
                symbolCableTray?.Activate();

                foreach (RevitLinkInstance link in links)
                {
                    conduitHolder.LinkInstance = link;
                    if (includeConduit && conduitHolder.TryPlaceLines()) continue;

                    cableTrayHolder.LinkInstance = link;
                    if (includeCableTray && cableTrayHolder.TryPlaceLines()) continue;
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