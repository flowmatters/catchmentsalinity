using RiverSystem.Constituents.Providers;
using TIME.Core.Metadata;

namespace CatchmentSalinity
{
    [WorksWith(typeof(SubcatchmentSalinityBalance))]
    public class SubcatchmentSalinityConstituentOutput : BaseCatchmentModelConstituentOutput
    {
    }
}
