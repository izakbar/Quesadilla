using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quesadilla.Classes.Configuration
{
    public enum PopupLocationType
    {
        Center,
        [Display(Name = "Top Left")]
        TopLeft,
        [Display(Name = "Top Center")]
        TopCenter,
        [Display(Name = "Top Right")]
        TopRight,
        [Display(Name = "Bottom Left")]
        BottomLeft,
        [Display(Name = "Bottom Center")]
        BottomCenter,
        [Display(Name = "Bottom Right")]
        BottomRight,
    }
}
