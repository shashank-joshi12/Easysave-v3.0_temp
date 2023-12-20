using System;
using System.Collections.Generic;
using System.Text;

namespace Easysave_v2._0.model

{
    class FileComparison: System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
    {
        public FileComparison() { }

        public bool Equals(System.IO.FileInfo f1, System.IO.FileInfo f2)
        {
           return (f1.Name == f2.Name && f1.Length == f2.Length);
        }


        public int GetHashCode(System.IO.FileInfo fi) // Function to retrieve the hash of files
        {
           string s = $"{fi.Name}{fi.Length}";
           return s.GetHashCode(); // Return a hash that reflects the comparison criteria.  
        }
        
    }
}
