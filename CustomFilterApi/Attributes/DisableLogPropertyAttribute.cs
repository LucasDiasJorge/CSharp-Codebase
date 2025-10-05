using System;

namespace CustomFilterApi.Attributes
{
    /// <summary>
    /// Attribute to disable the LogPropertyFilter execution on an action or controller.
    /// Use like: [DisableLogProperty(Ignore = true)] to skip logging properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DisableLogPropertyAttribute : Attribute
    {
        /// <summary>
        /// When true the filter should skip processing for the decorated action/controller.
        /// Defaults to true for convenience when just applying the attribute.
        /// </summary>
        public bool Ignore { get; set; } = true;

        public DisableLogPropertyAttribute()
        {
        }

        public DisableLogPropertyAttribute(bool ignore)
        {
            Ignore = ignore;
        }
    }
}
