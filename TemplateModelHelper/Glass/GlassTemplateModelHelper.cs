using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Glass.Mapper.Sc;
using Sitecore.Data.Items;
using TemplateModelHelper.Models;

namespace TemplateModelHelper.Glass
{
    public class GlassTemplateModelHelper : ITemplateModelHelper<IGlassBase>
    {
        private readonly ISitecoreContext _context;

        // Contains the relation TemplateID => Child Template IDs
        protected static ConcurrentDictionary<string, Dictionary<Guid, IList<Guid>>> InheritanceMaps = new ConcurrentDictionary<string, Dictionary<Guid, IList<Guid>>>();

        // Map of all Types with the TemplateModelHelperAttribute to Template ID
        protected static ConcurrentDictionary<string, Dictionary<Type, Guid>> TypeToIdMaps = new ConcurrentDictionary<string, Dictionary<Type, Guid>>();

        public GlassTemplateModelHelper(ISitecoreContext context)
        {
            _context = context;
        }

        public IEnumerable<T> Children<T>(IGlassBase item) where T : class
        {
            if (item == null)
            {
                return new List<T>();
            }

            var type = typeof (T);

            // IF I is queried, bypass all filtering
            if (type == typeof (IGlassBase))
            {
                return _getChildren<T>(item, null, false);
            }

            Guid template = TemplateId<T>();

            if (template == default(Guid))
            {
                throw new Exception("Unable to read the templateID attribute from the specified type: " + type.FullName);
            }

            var applicableTemplateIds = _getChildTemplateIds(type).ToList();
            applicableTemplateIds.Add(template);

            return _getChildren<T>(item, applicableTemplateIds, true);
        }

        public Guid TemplateId<T>() where T : class
        {
            return _templateId(typeof(T));
        }

        private Guid _templateId(Type t)
        {
            return _getTemplateId(t);
        }

        private Guid _getTemplateId(Type type)
        {
            // 1. Read or build type to ID map
            var key = type.Assembly.FullName;
            var map = TypeToIdMaps.GetOrAdd(key, f => TemplateModelHelperAttribute.BuildTypeToIdMap(type.Assembly));

            if (map.ContainsKey(type))
            {
                return map[type];
            }

            return default(Guid);
        }

        public IEnumerable<T> Descendants<T>(IGlassBase item) where T : class
        {
            if (item == null)
            {
                return new List<T>();
            }

            var type = typeof(T);

            // IF I is queried, bypass all filtering
            if (type == typeof(IGlassBase))
            {
                return _getDescendants<T>(item, null, false);
            }

            Guid template = TemplateId<T>();

            if (template == default(Guid))
            {
                throw new Exception("Unable to read the templateID attribute from the specified type: " + type.FullName);
            }

            var applicableTemplateIds = _getChildTemplateIds(type).ToList();
            applicableTemplateIds.Add(template);

            return _getDescendants<T>(item, applicableTemplateIds, true);
        }

        protected Item _getItemFromTypedItem(IGlassBase typedItem)
        {
            return typedItem.Item;
        }

        /// <summary>
        /// Returns all ancestors which can be mapped to the specified type.
        /// Not mappable items will be skipped.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public IEnumerable<T> Ancestors<T>(IGlassBase item) where T : class
        {
            if (item == null)
            {
                return new List<T>();
            }

            var type = typeof(T);

            // IF I is queried, bypass all filtering
            if (type == typeof(IGlassBase))
            {
                return _getAncestors<T>(item, null, false);
            }

            Guid template = TemplateId<T>();

            if (template == default(Guid))
            {
                throw new Exception("Unable to read the templateID attribute from the specified type: " + type.FullName);
            }

            var applicableTemplateIds = _getChildTemplateIds(type).ToList();
            applicableTemplateIds.Add(template);

            return _getAncestors<T>(item, applicableTemplateIds, true);
        }

        /// <summary>
        /// Returns the parent item with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public T Parent<T>(IGlassBase item) where T : class
        {
            if (item == null)
            {
                return null;
            }

            var scItem = _getItemFromTypedItem(item);

            if (scItem == null)
            {
                return null;
            }

            return _context.Cast<T>(scItem.Parent);
        }

        public T FirstChild<T>(IGlassBase item) where T : class
        {
            if (item == null)
            {
                return null;
            }

            var type = typeof(T);

            // IF I is queried, bypass all filtering
            if (type == typeof(IGlassBase))
            {
                return _getFirstChild<T>(item, null, false);
            }

            Guid template = TemplateId<T>();

            if (template == default(Guid))
            {
                throw new Exception("Unable to read the templateID attribute from the specified type: " + type.FullName);
            }

            var applicableTemplateIds = _getChildTemplateIds(type).ToList();
            applicableTemplateIds.Add(template);

            return _getFirstChild<T>(item, applicableTemplateIds, true);
        }

        protected T _getFirstChild<T>(IGlassBase item, IEnumerable<Guid> templateIds, bool filterByTemplate) where T : class
        {
            if (item == null) return null;

            var scItem = _getItemFromTypedItem(item);

            if (scItem == null) return null;

            Item sourceItem;

            if (filterByTemplate)
            {
                sourceItem = scItem.Children.FirstOrDefault(t => templateIds.ToList().Contains(t.TemplateID.ToGuid()));
            }
            else
            {
                sourceItem = scItem.Children.FirstOrDefault();
            }

            return _context.Cast<T>(sourceItem);
        }

        protected IEnumerable<T> _getChildren<T>(IGlassBase item, IEnumerable<Guid> templateIds, bool filterByTemplate) where T : class
        {
            if (item == null) return new List<T>();

            var scItem = _getItemFromTypedItem(item);

            if (scItem == null) return new List<T>();

            IEnumerable<Item> sourceItems;

            if (filterByTemplate)
            {
                sourceItems = scItem.Children.Where(t => templateIds.ToList().Contains(t.TemplateID.ToGuid()));
            }
            else
            {
                sourceItems = scItem.Children;
            }

            return CastItems<T>(sourceItems);
        }

        protected IEnumerable<T> _getAncestors<T>(IGlassBase item, IEnumerable<Guid> templateIds, bool filterByTemplate) where T : class
        {
            if (item == null) return new List<T>();

            var scItem = _getItemFromTypedItem(item);

            if (scItem == null) return new List<T>();

            IEnumerable<Item> sourceItems;

            if (filterByTemplate)
            {
                sourceItems = scItem.Axes.GetAncestors().Where(t => templateIds.ToList().Contains(t.TemplateID.ToGuid()));
            }
            else
            {
                sourceItems = scItem.Axes.GetAncestors();
            }

            return CastItems<T>(sourceItems);
        }

        protected IEnumerable<T> _getDescendants<T>(IGlassBase item, IEnumerable<Guid> templateIds, bool filterByTemplate) where T : class
        {
            if (item == null) return new List<T>();

            var scItem = _getItemFromTypedItem(item);

            if (scItem == null) return new List<T>();

            IEnumerable<Item> sourceItems;

            if (filterByTemplate)
            {
                sourceItems = scItem.Axes.GetDescendants().Where(t => templateIds.ToList().Contains(t.TemplateID.ToGuid()));
            }
            else
            {
                sourceItems = scItem.Axes.GetDescendants();
            }

            return CastItems<T>(sourceItems);
        }

        /// <summary>
        /// Tries to cast a collection of sitecore items to the specified type.
        /// If casting fails, the failing item is ignored.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceItems"></param>
        /// <returns></returns>
        private IEnumerable<T> CastItems<T>(IEnumerable<Item> sourceItems) where T:class
        {
            var items = new List<T>();

            foreach (Item baseChild in sourceItems)
            {
                var x = _context.Cast<T>(baseChild);
                if (x != null)
                {
                    items.Add(x);
                }
            }

            return items;
        }

        private IEnumerable<Guid> _getChildTemplateIds(Type type)
        {
            return _getSubTypes(type);
        }

        private IEnumerable<Guid> _getSubTypes(Type type)
        {
            var templateId = _templateId(type);
            if (templateId == default(Guid))
            {
                return new List<Guid>();
            }

            // Get a map of all types with their sub types
            var idSubMap = _getInherintanceMap(type.Assembly);

            IList<Guid> subTypes;
            if (idSubMap.TryGetValue(templateId, out subTypes))
            {
                return subTypes;
            }

            return new List<Guid>();
        }

        private Dictionary<Guid, IList<Guid>> _getInherintanceMap(Assembly assembly)
        {
            var key = assembly.FullName;
            return InheritanceMaps.GetOrAdd(key, f => TemplateModelHelperAttribute.BuildInherintanceMap(assembly));
        }
    }
}