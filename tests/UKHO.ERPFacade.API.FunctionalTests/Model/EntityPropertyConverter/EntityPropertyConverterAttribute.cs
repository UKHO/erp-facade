﻿using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.FunctionalTests.Model.EntityPropertyConverter
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityPropertyConverterAttribute : Attribute
    {
        public Type ConvertToType;

        public EntityPropertyConverterAttribute()
        {

        }
        public EntityPropertyConverterAttribute(Type convertToType)
        {
            ConvertToType = convertToType;
        }
    }
}
