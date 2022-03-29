using System.ComponentModel;

namespace Core.Enum
{
    public enum CurrencyType
    {
        Default = 0,
        
        [Description("Gil")]
        Gil = 1,
        
        [Description("Allegan Tomestones Of Poetics")]
        TomestonesOfPoetics = 2,
        
        [Description("Company Seals")]
        Seals = 3
    }
}