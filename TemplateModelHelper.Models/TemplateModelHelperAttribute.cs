using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TemplateModelHelper.Models
{
    /// <summary>
    /// Attribute used on interfaces used with TemplateModelHelper
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class TemplateModelHelperAttribute : Attribute
    {
        public string TemplateId { get; set; }

        public string BaseTemplates { get; set; }

        /// <summary>
        /// Returns templateId as parsed guid.
        /// If not set or default(Guid) is returned
        /// </summary>
        /// <returns></returns>
        public Guid GetTemplateId()
        {
            Guid id;
            if (Guid.TryParse(TemplateId, out id))
            {
                return id;
            }

            return default(Guid);
        }

        /// <summary>
        /// Returns base templates as parsed Guids
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Guid> GetBaseTemplates()
        {
            return _convertToGuidList(BaseTemplates);
        }

        /// <summary>
        /// Returns TemplateModelHelperAttribute data on a type. If the type doesn't have the TemplateModelHelperAttribute, null is returned.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static TemplateModelHelperAttribute GetAttribute(Type t)
        {
            var templateAttrib = t.GetCustomAttributes(
                typeof (TemplateModelHelperAttribute), true
                ).FirstOrDefault() as TemplateModelHelperAttribute;
            if (templateAttrib != null)
            {
                return templateAttrib;
            }
            return null;
        }

        /// <summary>
        /// Returns all types in the specified assembly which have the TemplateModelHelperAttribute
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetApplicapleTypes(Assembly a)
        {
            return a.GetTypes().Where(type => type.GetCustomAttributes(typeof (TemplateModelHelperAttribute), true).Length > 0);
        }

        /// <summary>
        /// Returns a dictionary which maps templateIds to all it's child template IDs
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Dictionary<Guid, IList<Guid>> BuildInherintanceMap(Assembly assembly)
        {
            var inheritanceMap = new Dictionary<Guid, IList<Guid>>();

            foreach (var theType in GetApplicapleTypes(assembly))
            {
                var templateData = GetAttribute(theType);
                if (templateData != null)
                {
                    Guid templateId = templateData.GetTemplateId();

                    if (templateId == default(Guid))
                    {
                        continue;
                    }

                    var baseTemplates = templateData.GetBaseTemplates();

                    foreach (var baseTemplate in baseTemplates)
                    {
                        if (inheritanceMap.ContainsKey(baseTemplate))
                        {
                            inheritanceMap[baseTemplate].Add(templateId);
                        }
                        else
                        {
                            inheritanceMap.Add(baseTemplate, new List<Guid> { templateId });
                        }
                    }
                }
            }

            return inheritanceMap;
        }

        /// <summary>
        /// Returns a dictionary which maps types to their corresponding templateId
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Dictionary<Type, Guid> BuildTypeToIdMap(Assembly assembly)
        {
            var map = new Dictionary<Type, Guid>();

            foreach (var theType in GetApplicapleTypes(assembly))
            {
                if (!map.ContainsKey(theType))
                {
                    var data = GetAttribute(theType);

                    // Only add data, if the TypeHelperAttribute has been defined for this type
                    if (data != null && !string.IsNullOrEmpty(data.TemplateId))
                    {
                        Guid id;
                        if (Guid.TryParse(data.TemplateId, out id))
                        {
                            map.Add(theType, id);
                        }
                    }
                }
            }

            return map;
        }

        private static IEnumerable<Guid> _convertToGuidList(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return new List<Guid>();
            }

            var idList = new List<Guid>();

            foreach (var id in ids.Split(','))
            {
                Guid guid;
                if (Guid.TryParse(id, out guid))
                {
                    idList.Add(guid);
                }
            }

            return idList;
        }

    }
}
