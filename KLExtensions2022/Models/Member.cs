using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLExtensions2022
{
    public class Member
    {
        public string name;
        public ITypeSymbol type;

        public Member(string name, ITypeSymbol type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
