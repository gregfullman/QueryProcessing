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
                        new JProperty("IsEmployed", true),
                        new JProperty("Job",
                            new JObject(
                                new JProperty("Employer", "Tyler Technologies"),
                                new JProperty("Position", "Engineer"),
                                new JProperty("Salary", "50000")
                            )
                        ),
                        new JProperty("Addresses", new JArray(
                            new JObject(
                                new JProperty("Street", "12 Jameco Mill Road"),
                                new JProperty("City", "Scarborough"),
                                new JProperty("State", "ME"),
                                new JProperty("ZipCode", "04074"),
                                new JProperty("PreviousOwners",
                                    new JArray(
                                        new JObject(
                                            new JProperty("Name", "Bob")
                                        ),
                                        new JObject(
                                            new JProperty("Name", "Chris")
                                        )
                                    )
                                )
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
                        new JProperty("IsEmployed", true),
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
                        new JProperty("IsEmployed", null),
                        new JProperty("Job",
                            new JObject(
                                new JProperty("Employer", "Waste Management"),
                                new JProperty("Position", "Engineer"),
                                 new JProperty("Salary", "70000")
                            )
                        )
                    ),
                    new JObject(
                        new JProperty("Name", null),
                        new JProperty("DOB", null),
                        new JProperty("NetWorth", -150)
                    ),
                    new JObject(
                    )
                );

            var queryable = dataArray.AsQueryable();
            var parsingConfig = ParsingConfig.Default;
            parsingConfig.CustomTypeProvider = new CustomTypeProvider();

            #region Count
            var simpleCount = queryable.Count();
            #endregion

            #region Filtering

            // String(value) handles null values no problem
            var filteredByString = queryable.ApplyPropertiesExistFilter("Name").Where("String(Name) == @0", "Greg");
            var stringFilteringCount = filteredByString.Count();

            var filteredByStringContains = queryable.ApplyPropertiesExistFilter("Name").Where("(Name.Type == @1 ? String.Empty : String(Name)).Contains(@0)", "Greg", JTokenType.Null);
            var filteredByStringContainsCount = filteredByStringContains.Count();

            // DateTime(value) does not handle null values. Need to check for null
            var filteredByDate = queryable.ApplyPropertiesExistFilter("DOB").Where("(DOB.Type == @1 ? DateTime.MinValue : DateTime(DOB)) <= @0", new DateTime(1987, 7, 27), JTokenType.Null);
            var dateFilteredCount = filteredByDate.Count();

            // Convert.ToDecimal converts null to zero. So we need to account for null specifically
            var filteredByNumber = queryable.ApplyPropertiesExistFilter("NetWorth").Where("(NetWorth.Type == @1 ? Decimal.MinValue : Convert.ToDecimal(NetWorth)) <= @0", Convert.ToDecimal(0), JTokenType.Null);
            var numberFilteredCount = filteredByNumber.Count();

            // TimeSpans are not stored as a CLR type in JSON objects, they're stored as strings. So we need to cast as string and then parse using TimeSpan.Parse
            var filteredByTimeSpan = queryable.ApplyPropertiesExistFilter("TimeElapsed").Where("(TimeElapsed.Type == @1 ? TimeSpan.MinValue : TimeSpan.Parse(String(TimeElapsed))) >= @0", new TimeSpan(18, 20, 0), JTokenType.Null);
            var timeSpanFilteringCount = filteredByTimeSpan.Count();

            var filteredByBool = queryable.ApplyPropertiesExistFilter("IsEmployed").Where("(IsEmployed.Type == @1 ? false : Boolean(IsEmployed)) == @0", true, JTokenType.Null);
            var filteredByBoolCount = filteredByBool.Count();

            // Filtering on a nested one-to-one field
            var filteringNestedOneToOne = queryable.ApplyPropertiesExistFilter("Job.Position").Where("String(Job.Position) == @0", "Engineer");
            var filteringNestedOneToOneCount = filteringNestedOneToOne.Count();

            // Filtering on a nested one-to-many field
            var filteringNestedOneToMany = queryable.ApplyPropertiesExistFilter("Addresses.State").Where("Addresses.Any(String(State) == @0)", "ME");
            var filteringNestedOneToManyCount = filteringNestedOneToMany.Count();

            // FIltering on multiple nested sequence operators
            var multipleNestedAny = queryable.ApplyPropertiesExistFilter("Addresses.PreviousOwners").Where("Addresses.Any(PreviousOwners.Any())");
            var multipleNestedAnyCount = multipleNestedAny.Count();

            var multipleNestedPropertyFilter = queryable.ApplyPropertiesExistFilter("Addresses.PreviousOwners.Name").Where("Addresses.Any(PreviousOwners.Any(String(Name) == @0))", "Chris");
            var multipleNestedPropertyFilterCount = multipleNestedPropertyFilter.Count();
            #endregion

            #region Grouping

            var groupingByTopLevelBool = queryable.ApplyPropertiesExistFilter("IsEmployed").GroupBy("IsEmployed");
            var distinctGroupsCount = groupingByTopLevelBool.Count(); // == 2

            var groupingByOneToOneField = queryable.ApplyPropertiesExistFilter("Job.Position").GroupBy("Job.Position");
            var distinctPositionGroupsCount = groupingByOneToOneField.Count(); // == 2

            var groupingByOneToManyField = queryable.ApplyPropertiesExistFilter("Addresses.State")
                                                    .SelectMany("Addresses.Select(it)",     // Hacky workaround to force JToken that is a JArray to be recognized as an enumerable
                                                                "new(Outer as Parent, Inner as Address)", 
                                                                "Outer", 
                                                                "Inner")
                                                    .GroupBy("Address.State");

            var distinctAddressStateGroupCount = groupingByOneToManyField.Count(); // == 2

            #endregion

            #region Selection

            // Using the SelectProperties extension method in conjunction with the NameProjection class allow us to handle missing fields, getting them converted to null
            var firstNameSelector = queryable.SelectProperties(new NameProjection("Name"));
            var nestedOneToOneSelector = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("DOB"), new NameProjection("Job.Position", "Position"));
            var nestedOneToManySelector = queryable.SelectProperties(new NameProjection("Name"), 
                                                                     new NameProjection("Job.Position", "Position"),
                                                                     new NameProjection("Addresses", new List<NameProjection>
                                                                     {
                                                                         new NameProjection("State"),
                                                                         new NameProjection("City"),
                                                                         new NameProjection("PreviousOwners", new List<NameProjection>
                                                                         {
                                                                             new NameProjection("Name")
                                                                         })
                                                                     }));

            var rootLevelCalculation = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("TimeElapsed"), new NameProjection("NetWorth"))
                                                // Calculate NetWorth/TimeElapsed (dollars per hour?)
                                                // Check for null or JTokenType.Null, since the SelectProperties logic adds actual nulls (TODO: need to do this check everywhere)
                                                .Select("new(Name as Name, (NetWorth == null || TimeElapsed == null || NetWorth.Type == @0 || TimeElapsed.Type == @0 ? null : (Decimal(NetWorth) / Decimal(TimeSpan.Parse(String(TimeElapsed)).TotalHours))) as DollarsPerHour)", JTokenType.Null);

            // Calculation with SQL-style null handling
            var nestedOneToOneCalculationSqlNullHandling = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("NetWorth"), new NameProjection("Job.Salary", "Salary"))
                                                                    .Select("new(Name as Name, (NetWorth == null || Salary == null || NetWorth.Type == @0 || Salary.Type == @0 ? null : (Decimal(NetWorth) + Decimal(Salary))) as Total)", JTokenType.Null);

            var nestedOneToOneCalculationLogicalNulHandling = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("NetWorth"), new NameProjection("Job.Salary", "Salary"))
                                                                        .Select(@"new(
                                                                                        (
                                                                                            (NetWorth == null || NetWorth.Type == @0) && (Salary == null || Salary.Type == @0)
                                                                                        ) ? null : 
                                                                                        (
                                                                                            (
                                                                                                (NetWorth == null || NetWorth.Type == @0) ? 0 : Decimal(NetWorth)
                                                                                            ) + 
                                                                                            (
                                                                                                (Salary == null || Salary.Type == @0) ? 0 : Decimal(Salary)
                                                                                            )
                                                                                        ) as Total
                                                                                    )", JTokenType.Null);

            var nestedOneToManyAggregationSqlNullHandling = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("Addresses"))
                                                                     .Select("new(Name as Name, ((Addresses == null || Addresses.Type == @0) ? null : Addresses.Count()) as AddressCount)", JTokenType.Null);

            var nestedOneToManyAggregationLogicalNullHandling = queryable.SelectProperties(new NameProjection("Name"), new NameProjection("Addresses"))
                                                                         .Select("new(Name as Name, ((Addresses == null || Addresses.Type == @0) ? 0 : Addresses.Count()) as AddressCount)", JTokenType.Null);

            #endregion



            var i = 0;
        }
    }
}