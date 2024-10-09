﻿using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CableTrayAnnotationHelper
{
    public static class Utils
    {
        public static void PlaceTheLines(Document mainDocument,
            Document linkedDocument,
            View view,
            BuiltInCategory builtInCategory,
            List<FamilyInstance> existingDetailLines,
            FamilySymbol familySymbol,
            List<ParameterAssociation> paramsTable)
        {
            //problem with viewId, it seems i need to provide a view from a linked document, not main document
            using FilteredElementCollector collector = new(linkedDocument, view.Id);
            List<Element> elements = collector
                .OfCategory(builtInCategory)
                .Where(e => e != null)
                .ToList();

            foreach (Element element in elements)
            {
                if (element is null
                    || existingDetailLines.
                        Any(e => e.LookupParameter(paramsTable
                                .First(p => p.ParameterType == ParameterType.Id)
                                .ParameterOut)
                            .AsString() == element.UniqueId))
                    continue;

                Line line = element.GetTheLine();
                DetailCurve detailLine = mainDocument.Create.NewDetailCurve(view, line);
                Line baseLine = detailLine.GeometryCurve as Line;

                FamilyInstance insertNew = mainDocument.Create.NewFamilyInstance(baseLine, familySymbol, view);

                foreach (ParameterAssociation param in paramsTable)
                {
                    Parameter valueIn = element.LookupParameter(param.ParameterIn);
                    Parameter valueOut = insertNew.LookupParameter(param.ParameterOut);
                    if (valueIn is null || valueOut is null)
                        continue;

                    switch (param.ParameterType)
                    {
                        case ParameterType.String:
                            if (valueIn.StorageType is StorageType.String)
                                valueOut.Set(valueIn.AsString());
                            break;
                        case ParameterType.Double:
                            if (valueIn.StorageType is StorageType.Double)
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

        public static BitmapSource GetEmbeddedImage(string name)
        {
            try
            {
                Assembly a = Assembly.GetExecutingAssembly();
                Stream s = a.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
        public static List<FamilyInstance> ExistingDetailLines(Document document, View view, Family family, FamilySymbol symbol) =>
            new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e != null)
                .Cast<FamilyInstance>()
                .Where(e => e.Symbol.FamilyName == family.Name && e.Name == symbol.Name)
                .ToList();
        public static List<Document> GetLinkedDocuments(Document document) =>
            new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Select(l => l.GetLinkDocument())
                .Where(d => d != null)
                .ToList();
    }
}