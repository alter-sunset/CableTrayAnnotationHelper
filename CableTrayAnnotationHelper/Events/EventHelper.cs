using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System.Collections.Generic;
using CableTrayAnnotationHelper.MVVM;

namespace CableTrayAnnotationHelper.Events
{
    public static class EventHelper
    {
        public static bool IsEverythingFilled(this ViewModelCTAH viewModel) =>
            !(viewModel.SelectedFamily is null
            || !viewModel.IncludeConduit && !viewModel.IncludeCableTray
            || viewModel.IncludeConduit && viewModel.SelectedConduit is null
            || viewModel.IncludeCableTray && viewModel.SelectedCableTray is null);
        public static List<RevitLinkInstance> GetLinkedDocuments(this Document document) =>
            new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfType<RevitLinkInstance>()
                .ToList();
        public static List<FamilyInstance> GetExistingDetailLines(this Document document, View view, Family family, FamilySymbol symbol) =>
            new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Cast<FamilyInstance>()
                .Where(e => e.Symbol.FamilyName == family.Name && e.Name == symbol.Name)
                .ToList();
        public static bool TryPlaceLines(this PlaceLinesHolder linesHolder)
        {
            if (linesHolder.FamilySymbol is null) return false;
            linesHolder.PlaceTheLines(out bool noLinkedView);
            return noLinkedView;
        }
        private static void PlaceTheLines(this PlaceLinesHolder linesHolder, out bool noLinkedView)
        {
            Document mainDocument = linesHolder.Document;
            RevitLinkInstance link = linesHolder.LinkInstance;
            View view = linesHolder.View;
            BuiltInCategory builtInCategory = linesHolder.BuiltInCategory;
            List<FamilyInstance> existingDetailLines = linesHolder.ExistingDetailLines;
            FamilySymbol familySymbol = linesHolder.FamilySymbol;
            List<ParameterAssociation> paramsTable = linesHolder.Parameters;

            noLinkedView = false;
            Document linkedDocument = link.GetLinkDocument();
            ElementId linkedDocId = link.GetTypeId();

            RevitLinkGraphicsSettings linkGraphicsSettings = view.GetLinkOverrides(linkedDocId);
            if (linkGraphicsSettings is null) return;
            ElementId linkedViewId = linkGraphicsSettings.LinkedViewId;

            HashSet<string> detailLineIds = existingDetailLines
                .Select(i => i.LookupParameter(paramsTable
                    .First(p => p.ParameterType == ParameterType.Id)
                    .ParameterOut)?
                    .AsString())
                .Where(id => id != null)
                .Distinct()
                .ToHashSet();
            List<Element> elements;
            try
            {//TODO: add check for modified trays
                elements = new FilteredElementCollector(linkedDocument, linkedViewId)
                   .OfCategory(builtInCategory)
                   .Where(e => e != null && !detailLineIds.Contains(e.UniqueId))
                   .ToList();
            }
            catch
            {
                noLinkedView = true;
                TaskDialog.Show("Ошибка!", "Нет связанного вида");
                return;
            }

            if (elements.Count == 0) return;

            foreach (Element element in elements)
            {
                Line line = element.GetTheLine();
                DetailCurve detailLine = mainDocument.Create.NewDetailCurve(view, line);
                Line baseLine = detailLine.GeometryCurve as Line;

                FamilyInstance insertNew = mainDocument.Create.NewFamilyInstance(baseLine, familySymbol, view);

                foreach (ParameterAssociation param in paramsTable)
                {
                    Parameter valueIn = element.LookupParameter(param.ParameterIn);
                    Parameter valueOut = insertNew.LookupParameter(param.ParameterOut);

                    if (valueIn is null || valueOut is null) continue;
                    switch (param.ParameterType)
                    {
                        case ParameterType.String when valueIn.StorageType == StorageType.String:
                            valueOut.Set(valueIn.AsString());
                            break;
                        case ParameterType.Double when valueIn.StorageType == StorageType.Double:
                            valueOut.Set(valueIn.AsDouble());
                            break;
                        case ParameterType.Id:
                            valueOut.Set(element.UniqueId);
                            break;
                    }
                }
                mainDocument.Delete(detailLine.Id);
            }
        }
        private static Line GetTheLine(this Element element) => (element.Location as LocationCurve).Curve as Line;
    }
}