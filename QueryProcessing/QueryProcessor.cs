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
using System.Text;

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
                        new JProperty("Job",
                            new JObject(
                                new JProperty("Employer", "Tyler Technologies"),
                                new JProperty("Position", "Engineer")
                            )
                        ),
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
                        new JProperty("TimeElapsed", null),
                        new JProperty("Job",
                            new JObject(
                                new JProperty("Employer", "Waste Management"),
                                new JProperty("Position", "Engineer")
                            )
                        )
                    ),
                    new JObject(
                        new JProperty("Name", null),
                        new JProperty("DOB", null),
                        new JProperty("NetWorth", -150)
                    )
                );

            var queryable = dataArray.AsQueryable();

            #region Count
            var simpleCount = queryable.Count();
            #endregion

            #region Filtering

            // String(value) handles null values no problem
            var filteredByString = queryable.ApplyPropertiesExistFilter("Name").Where("String(Name) == @0", "Greg");
            var stringFilteringCount = filteredByString.Count();

            // DateTime(value) does not handle null values. Need to check for null
            var filteredByDate = queryable.ApplyPropertiesExistFilter("DOB").Where("(DOB.Type == @1 ? DateTime.MinValue : DateTime(DOB)) <= @0", new DateTime(1987, 7, 27), JTokenType.Null);
            var dateFilteredCount = filteredByDate.Count();

            // Convert.ToDecimal converts null to zero. So we need to account for null specifically
            var filteredByNumber = queryable.ApplyPropertiesExistFilter("NetWorth").Where("(NetWorth.Type == @1 ? Decimal.MinValue : Convert.ToDecimal(NetWorth)) <= @0", Convert.ToDecimal(0), JTokenType.Null);
            var numberFilteredCount = filteredByNumber.Count();

            // TimeSpans are not stored as a CLR type in JSON objects, they're stored as strings. So we need to cast as string and then parse using TimeSpan.Parse
            var filteredByTimeSpan = queryable.ApplyPropertiesExistFilter("TimeElapsed").Where("(TimeElapsed.Type == @1 ? TimeSpan.MinValue : TimeSpan.Parse(String(TimeElapsed))) >= @0", new TimeSpan(18, 20, 0), JTokenType.Null);
            var timeSpanFilteringCount = filteredByTimeSpan.Count();

            // Filtering on a nested one-to-one field
            var filteringNestedOneToOne = queryable.ApplyPropertiesExistFilter("Job.Position").Where("String(Job.Position) == @0", "Engineer");
            var filteringNestedOneToOneCount = filteringNestedOneToOne.Count();

            // Filtering on a nested one-to-many field
            var filteringNestedOneToMany = queryable.ApplyPropertiesExistFilter("Addresses.State").Where("Addresses.Any(String(State) == @0)", "ME");
            var filteringNestedOneToManyCount = filteringNestedOneToMany.Count();
            #endregion

            var i = 0;
        }
    }
}