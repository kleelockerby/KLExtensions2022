using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLExtensions2022.Templates
{
    public static class CSharpTemplate
    {
        public static string ContentNoUsings = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace %NAMESPACE% 
{
    public class %FILENAME%
    {
        $
    }
}
";

        public static string ContentUsings = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace %NAMESPACE%; 

public class %FILENAME%
{
    $
}
";
    }
}
