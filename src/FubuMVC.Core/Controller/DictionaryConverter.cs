using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FubuMVC.Core.Controller
{
    public class ConvertProblem
    {
        public object Item { get; set; }
        public PropertyInfo Property { get; set; }
        public object Value { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return
@"Item type:       {0}
Property:        {1}
Property Type:   {2}
Attempted Value: {3}
Exception:
{4} 
"
                    .ToFormat(
                    ((Item != null) ? Item.GetType().FullName : "(null)"),
                    Property.Name,
                    Property.PropertyType,
                    Value,
                    Exception);
        }
    }

    public static class DictionaryConverter
    {
        public static bool CanCreateType(Type itemType)
        {
            return itemType.IsClass && itemType.GetConstructor(Type.EmptyTypes) != null;
        }

        public static TItem CreateAndPopulate<TItem>(IDictionary<string, object> values, out ICollection<ConvertProblem> problems) where TItem : class, new()
        {
            return (TItem) CreateAndPopulate(typeof (TItem), values, out problems);
        }

        public static object CreateAndPopulate(Type itemType, IDictionary<string, object> values, out ICollection<ConvertProblem> problems)
        {
            var item = Activator.CreateInstance(itemType);

            Populate(values, item, out problems);

            return item;
        }

        public static void Populate(IDictionary<string, object> values, object item, out ICollection<ConvertProblem> problems)
        {
            problems = new List<ConvertProblem>();

            if (values != null)
            {
                foreach (var propInfo in TypeDescriptorRegistry.GetPropertiesFor(item.GetType()).Values)
                {
                    if (propInfo.PropertyType.IsArray)
                    {
                        propInfo.SetValue(item, retrieveArrayValues(propInfo, values, problems), null);
                    }
                    else
                    {
                        object value;
                        if (values.TryGetValue(propInfo.Name, out value))
                        {
                            setPropFromValue(value, item, propInfo, problems);
                        }
                    }
                }
            }
        }

        private static object retrieveArrayValues(PropertyInfo arrayProp, IDictionary<string, object> values, ICollection<ConvertProblem> problems)
        {
            Type elemType = arrayProp.PropertyType.GetElementType();
            bool anyValuesFound = true;
            int idx = 0;
            var elements = (IList) Activator.CreateInstance(typeof (List<>).MakeGenericType(elemType));

            var properties = TypeDescriptorRegistry.GetPropertiesFor(elemType).Values;

            while (anyValuesFound)
            {
                object curElement = null;
                anyValuesFound = false; // false until proven otherwise
				
                foreach (var prop in properties)
                {
                    var key = string.Format("_{0}{1}_{2}", idx, arrayProp.Name, prop.Name);
                    object value;
					
                    if( values.TryGetValue(key, out value))
                    {
                        anyValuesFound = true;

                        if (curElement == null)
                        {
                            curElement = Activator.CreateInstance(elemType);
                            elements.Add(curElement);
                        }

                        setPropFromValue(value, curElement, prop, problems);
                    }
                }

                idx++;
            }

            var elementArray = Array.CreateInstance(elemType, elements.Count);
            elements.CopyTo(elementArray, 0);

            return elementArray;
        }

        private static void setPropFromValue(object value, object item, PropertyInfo prop, ICollection<ConvertProblem> problems)
        {
            writeToProperty(item, prop, value, problems);
        }

        private static void writeToProperty(object item, PropertyInfo prop, object value, ICollection<ConvertProblem> problems)
        {
            try
            {

                if (value != null && !Equals(value, ""))
                {
                    Type destType = prop.PropertyType;

                    if( destType == typeof(bool) && Equals(value, prop.Name) )
                    {
                        value = true;
                    }

                    if (prop.PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        prop.SetValue(item, value, null);
                        return;
                    }

                    if (prop.PropertyType.IsNullableOfT())
                    {
                        destType = prop.PropertyType.GetGenericArguments()[0];
                    }

                    prop.SetValue(item, Convert.ChangeType(value, destType), null);
                }
            }
            catch(Exception ex)
            {
                problems.Add(new ConvertProblem{Item = item, Property = prop, Value = value, Exception = ex});
            }
        }

        public static TItem SafeCreateAndPopulate<TItem>(IDictionary<string, object> requestData)
            where TItem : class, new()
        {
            ICollection<ConvertProblem> problems;
            var item = CreateAndPopulate<TItem>(requestData, out problems);
            
            if (problems.Count > 0)
            {
                var counter = 0;
                var builder = new StringBuilder();
                builder.Append("Could not convert all input values into their expected types:");
                builder.Append(Environment.NewLine);
                foreach( var prob in problems )
                {
                    builder.AppendFormat("-----Problem[{0}]---------------------", counter++);
                    builder.Append(Environment.NewLine);
                    builder.Append(prob);
                    builder.Append(Environment.NewLine);
                }
                throw new InvalidOperationException(builder.ToString());
            }

            return item;
        }
    }
}