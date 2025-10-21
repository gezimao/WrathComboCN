﻿using System;
using WrathCombo.Combos;

namespace WrathCombo.Attributes;

/// <summary> Attribute documenting required combo relationships. </summary>
[AttributeUsage(AttributeTargets.Field)]
internal class BozjaParentAttribute : Attribute
{
    /// <summary> Initializes a new instance of the <see cref="BozjaParentAttribute"/> class. </summary>
    /// <param name="parentPresets"> Presets that require the given combo to be enabled. </param>
    internal BozjaParentAttribute(params Preset[] parentPresets) => ParentPresets = parentPresets;

    /// <summary> Gets the display name. </summary>
    public Preset[] ParentPresets { get; }
}