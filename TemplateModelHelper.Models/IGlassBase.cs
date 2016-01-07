using Glass.Mapper.Sc.Configuration.Attributes;
using Sitecore.Data.Items;

namespace TemplateModelHelper.Models
{
    public interface IGlassBase
    {
        [SitecoreItem]
        Item Item { get; }

        //... your own attributes
    }
}
