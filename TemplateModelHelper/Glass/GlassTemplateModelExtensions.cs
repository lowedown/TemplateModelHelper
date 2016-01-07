using System.Collections.Generic;
using Glass.Mapper.Sc;

namespace TemplateModelHelper.Glass
{
    /// <summary>
    /// Contains extension methods for simplified access to the GlassTemplateModelHelper
    /// </summary>
    public static class GlassTemplateModelExtensions
    {
        /// <summary>
        /// Set this to a specific implementation of a helper class to override the default
        /// </summary>
        public static ITemplateModelHelper<IGlassBase> Helper;

        public static ITemplateModelHelper<IGlassBase> HelperFactory()
        {
            if (Helper != null)
            {
                return Helper;
            }

            return new GlassTemplateModelHelper(new SitecoreContext());
        }

        public static IEnumerable<T> Children<T>(this IGlassBase item) where T : class
        {
            return HelperFactory().Children<T>(item);
        }
        public static IEnumerable<T> Descendants<T>(this IGlassBase item) where T : class
        {
            return HelperFactory().Descendants<T>(item);
        }
        public static IEnumerable<T> Ancestors<T>(this IGlassBase item) where T : class
        {
            return HelperFactory().Ancestors<T>(item);
        }
        public static T Parent<T>(this IGlassBase item) where T : class
        {
            return HelperFactory().Parent<T>(item);
        }
        public static T FirstChild<T>(this IGlassBase item) where T : class
        {
            return HelperFactory().FirstChild<T>(item);
        }
    }
}
