using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiverSystem.Constituents.Providers;
using TIME.Core.Metadata;

namespace CatchmentSalinity
{
    [WorksWith(typeof(SubcatchmentSalinityBalance))]
    public class SubcatchmentSalinityConstituentOutput : BaseCatchmentModelConstituentOutput
    {
    }
}
