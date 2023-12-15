using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADM.CableTrayAnnotationHelper
{
    public class FamilyComparer : IEqualityComparer<Family>
    {
        public bool Equals(Family f1, Family f2)
        {
            if (f1 is null || f2 is null)
            {
                return false;
            }
            if (f1.Name == f2.Name)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Family fam) => fam.Name.GetHashCode();
    }
}
