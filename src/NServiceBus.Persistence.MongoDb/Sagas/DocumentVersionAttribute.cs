using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NServiceBus.Persistence.MongoDB
{
    /// <summary>
    /// Used to specify that a saga property should be unique across all saga instances. 
    /// This will ensure that 2 saga instances don't get persisted when using the property to correlate between multiple message types
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class DocumentVersionAttribute :Attribute
    {
        public static void SetPropertyValue(object objectWithADocumentVersionDecoratedProperty, int value)
        {
            GetDocumentVersionProperty(objectWithADocumentVersionDecoratedProperty.GetType()).SetValue(objectWithADocumentVersionDecoratedProperty, value);
        }

        public static KeyValuePair<string, int> GetProperty(object objectWithADocumentVersionDecoratedProperty)
        {
            var prop = GetDocumentVersionProperty(objectWithADocumentVersionDecoratedProperty.GetType());
            return new KeyValuePair<string, int>(prop.Name, (int)prop.GetValue(objectWithADocumentVersionDecoratedProperty));
        }

        public static int GetPropertyValue(object objectWithADocumentVersionDecoratedProperty)
        {
            return (int)GetDocumentVersionProperty(objectWithADocumentVersionDecoratedProperty.GetType()).GetValue(objectWithADocumentVersionDecoratedProperty);
        }

        /// <summary>
        /// Gets a single property that is marked with the <see cref="DocumentVersionAttribute"/> for a <see cref="IContainSagaData"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to evaluate.</param>
        /// <returns>A <see cref="PropertyInfo"/> of the property marked with a <see cref="DocumentVersionAttribute"/> or null if not used.</returns>
        public static PropertyInfo GetDocumentVersionProperty(Type type)
        {
            var properties = GetDocumentVersionProperties(type)
                .ToList();

            if (properties.Count > 1)
            {
                var message = string.Format("More than one DocumentVersionAttribute property was found on the type '{0}'. However, only one property is supported.", type.FullName);
                throw new InvalidOperationException(message);
            }

            var property = properties.Single();
            if (property.PropertyType != typeof (Int32))
            {
                var message = string.Format("The DocumentVersionAttribute property found on the type '{0}' can only be of type Int32.", type.FullName);
                throw new InvalidOperationException(message);
            }

            return property;
        }

        /// <summary>
        /// Gets all the properties that are marked with the <see cref="DocumentVersionAttribute"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="PropertyInfo"/>.</returns>
        public static IEnumerable<PropertyInfo> GetDocumentVersionProperties(Type type)
        {
            return type.GetProperties()
                .Where(p => p.CanRead && GetCustomAttribute(p, typeof(DocumentVersionAttribute)) != null);
        }
    }
}
