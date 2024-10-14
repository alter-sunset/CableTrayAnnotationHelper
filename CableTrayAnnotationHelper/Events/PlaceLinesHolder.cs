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
        public List<FamilyInstance> ExistingDetailLines { get; set; }
        public FamilySymbol FamilySymbol { get; set; }
        public List<ParameterAssociation> Parameters { get; set; }
    }
}