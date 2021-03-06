﻿namespace Nivaes.DataTestGenerator.Xunit
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using global::Xunit.Sdk;

    [DataDiscoverer("Nivaes.DataTestGenerator.Xunit.GenericGeneratorDataDiscoverer", "Nivaes.DataTestGenerator.Xunit")]
    [XunitTestCaseDiscoverer("Nivaes.DataTestGenerator.Xunit.GenerateStringCaseDiscoverer", "Nivaes.DataTestGenerator.Xunit")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class GenerateStringInlineDataAttribute
        : DataAttribute
    {
        public int DataNumber { get; set; } = 1;

        public int MaxSize { get; set; } = 1;

        public int MinSize { get; set; } = 1;

        public GenerateStringInlineDataAttribute()
        { }

        public GenerateStringInlineDataAttribute(int dataNumber)
        {
            DataNumber = dataNumber;
        }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            for (int i = 0; i < DataNumber; i++)
            {
                yield return new[] { GenericGenerator.Instance.GenerateString(MinSize, MaxSize) };
            }
        }
    }
}
