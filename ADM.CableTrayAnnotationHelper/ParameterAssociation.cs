using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADM.CableTrayAnnotationHelper
{
    public enum ParameterType { String, Double, Id }
    public class ParameterAssociation
    {
        public string ParameterIn { get; set; }
        public string ParameterOut { get; set; }
        public ParameterType ParameterType { get; set; }
    }
}
