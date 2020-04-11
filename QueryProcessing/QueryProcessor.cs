using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
//using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace QueryProcessing
{
    public class QueryProcessor
    {
        public void Test()
        {
            var dataArray = new JArray(
                    new JObject(
                        new JProperty("Name", "Greg"),
                        new JProperty("DOB", new DateTime(1987, 7, 27)),
                        new JProperty("NetWorth", 300),
                        new JProperty("TimeElapsed", new TimeSpan(2, 12, 20, 30)),
                        new JProperty("Addresses", new JArray(
                            new JObject(
                                new JProperty("Street", "12 Jameco Mill Road"),
                                new JProperty("City", "Scarborough"),
                                new JProperty("State", "ME"),
                                new JProperty("ZipCode", "04074")
                            ),
                            new JObject(
                                new JProperty("Street", "2218 Schoharie Turnpike"),
                                new JProperty("City", "Duanesburg"),
                                new JProperty("State", "NY"),
                                new JProperty("ZipCode", "12056")
                            )
                        ))
                    ),
                    new JObject(
                        new JProperty("Name", "Meg"),
                        new JProperty("DOB", new DateTime(1987, 7, 28)),
                        new JProperty("NetWorth", 50.3),
                        new JProperty("TimeElapsed", new TimeSpan(12, 20, 30)),
                        new JProperty("Addresses", new JArray(
                            new JObject(
                                new JProperty("Street", "12 Jameco Mill Road"),
                                new JProperty("City", "Scarborough"),
                                new JProperty("State", "ME"),
                                new JProperty("ZipCode", "04074")
                            ),
                            new JObject(
                                new JProperty("Street", "32 Paddock Place"),
                                new JProperty("City", "South Portland"),
                                new JProperty("State", "ME"),
                                new JProperty("ZipCode", "04105")
                            )
                        ))
                    ),
                    new JObject(
                        new JProperty("Name", "Bob"),
                        new JProperty("DOB", null),
                        new JProperty("NetWorth", null),
                        new JProperty("TimeElapsed", null)
                    ),
                    new JObject(
                        new JProperty("Name", null),
                        new JProperty("DOB", null),
                        new JProperty("NetWorth", -150)
                    )
                );

            var expConverter = new ExpandoObjectConverter();
            var dynamicArray = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataArray.ToString(), expConverter);

            var queryable = dynamicArray.ToDynamicList().AsQueryable() as IQueryable;

            #region Count
            var simpleCount = queryable.Count();
            #endregion

            #region Filtering

            // String(value) handles null values no problem
            var stringFilteringCount = queryable.Where("String(Name) == @0", "Greg").Count();

            // DateTime(value) does not handle null values. Need to check for null
            var dateFilteredCount = queryable.Where("(DOB == null ? DateTime.MinValue : DateTime(it.DOB)) <= DateTime(1987, 7, 27)").Count();

            // Convert.ToDecimal converts null to zero. So we need to account for null specifically
            var numberFilteredCount = queryable.Where("(NetWorth == null ? Decimal.MinValue : Convert.ToDecimal(NetWorth)) <= @0", Convert.ToDecimal(0)).Count();

            var timeSpanFilteringCount = queryable.Where("(TimeElapsed == null ? TimeSpan.MinValue : TimeSpan.Parse(TimeElapsed)) >= @0", new TimeSpan(18, 20, 0)).Count();

            #endregion

            var i = 0;
        }
    }
}