using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
    }
}
