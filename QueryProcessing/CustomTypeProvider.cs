using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace QueryProcessing
{
    public class CustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        public override HashSet<Type> GetCustomTypes()
        {
            var baseHashSet = base.GetCustomTypes();
            baseHashSet.Add(typeof(JObject));
            baseHashSet.Add(typeof(JToken));
            baseHashSet.Add(typeof(JArray));
            return baseHashSet;
        }
    }
}
