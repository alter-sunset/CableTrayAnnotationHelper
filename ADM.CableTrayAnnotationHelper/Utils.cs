using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using View = Autodesk.Revit.DB.View;
using Document = Autodesk.Revit.DB.Document;

namespace ADM.CableTrayAnnotationHelper
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
            using (FilteredElementCollector collector = new FilteredElementCollector(linkedDocument, view.Id))
            {
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
                    {
                        continue;
                    }
                    Line line = GetTheLine(element);
                    DetailCurve detailLine = mainDocument.Create.NewDetailCurve(view, line);
                    Line baseLine = detailLine.GeometryCurve as Line;

                    FamilyInstance insertNew = mainDocument.Create
                    .NewFamilyInstance(baseLine, familySymbol, view);

                    foreach (ParameterAssociation param in paramsTable)
                    {
                        Parameter valueIn = element.LookupParameter(param.ParameterIn);
                        Parameter valueOut = insertNew.LookupParameter(param.ParameterOut);
                        if (valueIn is null || valueOut is null)
                        {
                            continue;
                        }
                        switch (param.ParameterType)
                        {
                            case ParameterType.String:
                                if (valueIn.StorageType == StorageType.String)
                                {
                                    string valueString = valueIn.AsString();
                                    valueOut.Set(valueString);
                                }
                                break;
                            case ParameterType.Double:
                                if (valueIn.StorageType == StorageType.Double)
                                {
                                    double valueDouble = valueIn.AsDouble();
                                    valueOut.Set(valueDouble);
                                }
                                break;
                            case ParameterType.Id:
                                valueOut.Set(element.UniqueId);
                                break;
                        }
                    }
                    mainDocument.Delete(detailLine.Id);
                }
            }
        }

        public static Line GetTheLine(Element element)
        {
            Line line = (element.Location as LocationCurve).Curve as Line;
            return line;
        }

        public static bool AreAligned(Element element, Element detailLineInstance)
        {
            //doesn't work
            Line elementLine = GetTheLine(element);
            Line detailLine = GetTheLine(detailLineInstance);

            SetComparisonResult result = elementLine.Intersect(detailLine);

            if (result == SetComparisonResult.Overlap)
            {
                return true;
            }
            return false;
        }

        public static void LogThreadInfo(string name = "")
        {
            Thread th = Thread.CurrentThread;
            Debug.WriteLine($"Task Thread ID: {th.ManagedThreadId}, Thread Name: {th.Name}, Process Name: {name}");
        }

        public static void HandleError(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.Source);
            Debug.WriteLine(ex.StackTrace);
        }

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

        public static List<FamilyInstance> ExistingDetailLines(Document document, View view, Family family, FamilySymbol symbol)
        {
            List<FamilyInstance> existingDetailLines = new FilteredElementCollector(document, view.Id)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_DetailComponents)
                .Where(e => e != null)
                .Select(e => e as FamilyInstance)
                .Where(e => e.Symbol.FamilyName == family.Name && e.Name == symbol.Name)
                .ToList();

            return existingDetailLines;
        }

        public static List<Document> GetLinkedDocuments(Document document)
        {
            List<Document> links = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Select(l => l as RevitLinkInstance)
                .Select(l => l.GetLinkDocument())
                .Where(d => d != null)
                .ToList();

            return links;
        }
    }
}
