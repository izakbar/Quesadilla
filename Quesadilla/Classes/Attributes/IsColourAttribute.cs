using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Attributes
{
    public sealed class IsColourAttribute : Attribute
    {
        private bool isColourAttribute;
        public IsColourAttribute(bool isColourAttribute)
        {
            this.isColourAttribute = isColourAttribute;
        }

        public bool IsColour
        {
            get { return isColourAttribute; }
            set { isColourAttribute = value; }
        }

        public override bool IsDefaultAttribute()
        {
            return isColourAttribute == false;
        }

    }
}
