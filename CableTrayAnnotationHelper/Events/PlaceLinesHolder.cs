using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace CableTrayAnnotationHelper.Events
{
    public class PlaceLinesHolder
    {
        public Document Document { get; set; }
        public RevitLinkInstance LinkInstance { get; set; }
        public View View { get; set; }
        public BuiltInCategory BuiltInCategory { get; set; }
        public FamilyInstance[] ExistingDetailLines { get; set; }
        public FamilySymbol FamilySymbol { get; set; }
        public ParameterAssociation[] Parameters { get; set; }
    }
}