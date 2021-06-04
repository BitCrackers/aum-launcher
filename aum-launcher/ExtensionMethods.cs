using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Semver;
using System.IO;

namespace aum_launcher
{
    public static class ExtensionMethods
    {
        public static string ToSafeFileName(this SemVersion semver)
        {
            string semVerString = semver.ToString();
            semVerString = semVerString.Replace('.', '_');
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                semVerString.Replace(invalidChar, '_');

            return semVerString;
        }
    }
}
