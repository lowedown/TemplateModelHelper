using System.Collections.Generic;

namespace TemplateModelHelper
{
    public interface ITemplateModelHelper<I>
    {
        /// <summary>
        /// Returns all children which inherit the template of the specified type.
        /// If no items are found, an empty collection is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<T> Children<T>(I item) where T : class;

        /// <summary>
        /// Returns all descendants which inherit the template of the specified type.
        /// If no items are found, an empty collection is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<T> Descendants<T>(I item) where T : class;

        /// <summary>
        /// Returns all ancestors (parent, grandparent,...) which inherit the template of the specified type.
        /// If no items are found, an empty collection is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        IEnumerable<T> Ancestors<T>(I item) where T : class;

        /// <summary>
        /// Returns the parent item if it inherits the template of the specified type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        T Parent<T>(I item) where T : class;

        /// <summary>
        /// Returns the first child which inherits the template of the specified type.
        /// If no items are found, an empty collection is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        T FirstChild<T>(I item) where T : class;
    }
}