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
            if (obj is ExpandoObject expando)
            {
                var tempExpando = expando;
                var split = key.Split('.');
                for(int i = 0; i < split.Length; i++)
                {
                    // check if the current property exists
                    if (!(tempExpando as IDictionary<string, object>).ContainsKey(split[i]))
                        return false;
                    if(i+1 < split.Length)
                    {
                        // Check for the nested property
                        var nestedObj = (tempExpando as IDictionary<string, object>)[split[i]];
                        if (nestedObj is ExpandoObject)
                            tempExpando = nestedObj as ExpandoObject;
                        else if (nestedObj is IEnumerable<object> nestedObjArray)
                        {
                            var expandObjArray = nestedObjArray.Cast<ExpandoObject>();
                            return expandObjArray.Any(x => HasProperty(x, string.Join(".", split.Skip(i + 1))));
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
