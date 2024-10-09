using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace CableTrayAnnotationHelper
{
    public class FamilyComparer : IEqualityComparer<Family>
    {
        public bool Equals(Family f1, Family f2) => !(f1 is null || f2 is null || f1.Name != f2.Name);
        public int GetHashCode(Family fam) => fam.Name.GetHashCode();
    }
}