using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace CableTrayAnnotationHelper
{
    public class FamilyComparer : IEqualityComparer<Family>
    {
        public bool Equals(Family f1, Family f2)
        {
            if (f1 is null || f2 is null) return false;
            if (ReferenceEquals(f1, f2)) return true;
            return f1.Name.Equals(f2.Name);
        }
        public int GetHashCode(Family fam) => fam.Name.GetHashCode();
    }
}