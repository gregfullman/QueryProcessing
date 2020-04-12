using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace QueryProcessing
{
    [DynamicLinqType]
    public static class ExpandoObjectHelper
    {
        public static bool HasProperty(object obj, string key)
        {
            if (obj is JObject jObj)
            {
                var tempObj = jObj;
                var split = key.Split('.');
                for (int i = 0; i < split.Length; i++)
                {
                    if (!tempObj.ContainsKey(split[i]))
                        return false;
                    if (i + 1 < split.Length)
                    {
                        var nestedObj = tempObj[split[i]];
                        if (nestedObj is JObject)
                            tempObj = nestedObj as JObject;
                        else if(nestedObj is JArray nestedObjArray)
                        {
                            return nestedObjArray.Any(x => HasProperty(x, string.Join(".", split.Skip(i + 1))));
                        }
                        else
                            // TODO: not sure what this scenario is.
                            return false;
                    }
                }
                return true;
            }
            return true;
        }

        public static JToken SelectProperty(object obj, NameProjection propertyProjection)
        {
            if (obj is JObject jObj)
            {
                var tempObj = jObj;
                var split = propertyProjection.SourcePropertyName.Split('.');
                for (int i = 0; i < split.Length; i++)
                {
                    if (!tempObj.ContainsKey(split[i]))
                        return null;
                    var nestedObj = tempObj[split[i]];
                    if (i + 1 < split.Length)
                    {
                        if (nestedObj is JObject)
                            tempObj = nestedObj as JObject;
                        else
                            // TODO: not sure what this scenario is.
                            return null;
                    }
                    else if(nestedObj is JArray nestedObjArray && propertyProjection.ChildProjections?.Any() == true)
                    {
                        var resultArray = new JArray();
                        foreach(var item in nestedObjArray)
                        {
                            var arrayObj = new JObject();
                            foreach(var projection in propertyProjection.ChildProjections)
                            {
                                var projectionResult = SelectProperty(item, projection);
                                arrayObj.Add(projection.TargetPropertyName, projectionResult);
                            }
                            resultArray.Add(arrayObj);
                        }
                        return resultArray;
                    }
                    else
                    {
                        return nestedObj;
                    }
                }
            }
            return null;
        }
    }
}
