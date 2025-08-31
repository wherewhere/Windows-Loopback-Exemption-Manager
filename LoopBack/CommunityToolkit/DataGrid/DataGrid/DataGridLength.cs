// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.Controls.DataGridInternals;
using CommunityToolkit.WinUI.Utilities;
using System;
using System.Globalization;

namespace CommunityToolkit.WinUI.Controls
{
    /// <summary>
    /// DataGridLengthUnitType
    /// </summary>
    /// <remarks>
    /// These aren't flags.
    /// </remarks>
    public enum DataGridLengthUnitType
    {
        /// <summary>
        /// Auto DataGridLengthUnitType
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Pixel DataGridLengthUnitType
        /// </summary>
        Pixel = 1,

        /// <summary>
        /// SizeToCells DataGridLengthUnitType
        /// </summary>
        SizeToCells = 2,

        /// <summary>
        /// SizeToHeader DataGridLengthUnitType
        /// </summary>
        SizeToHeader = 3,

        /// <summary>
        /// Star DataGridLengthUnitType
        /// </summary>
        Star = 4
    }

    /// <summary>
    /// Represents the lengths of elements within the <see cref="DataGrid"/> control.
    /// </summary>
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "CommunityToolkit.WinUI.Controls.DataGridLength.ConvertFromString")]
    public readonly struct DataGridLength : IEquatable<DataGridLength>
    {
        // static instances of value invariant DataGridLengths
        private static readonly DataGridLength _auto = new(DATAGRIDLENGTH_DefaultValue, DataGridLengthUnitType.Auto);
        private static readonly DataGridLength _sizeToCells = new(DATAGRIDLENGTH_DefaultValue, DataGridLengthUnitType.SizeToCells);
        private static readonly DataGridLength _sizeToHeader = new(DATAGRIDLENGTH_DefaultValue, DataGridLengthUnitType.SizeToHeader);

        private const string _starSuffix = "*";
        private static readonly string[] _valueInvariantUnitStrings = ["auto", "sizetocells", "sizetoheader"];
        private static readonly DataGridLength[] _valueInvariantDataGridLengths = [Auto, SizeToCells, SizeToHeader];

        private readonly double _desiredValue; // desired value storage
        private readonly double _displayValue; // display value storage
        private readonly double _unitValue;    //  unit value storage
        private readonly DataGridLengthUnitType _unitType; //  unit type storage

        internal const double DATAGRIDLENGTH_DefaultValue = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridLength"/> struct based on a numerical value.
        /// </summary>
        /// <param name="value">numerical length</param>
        public DataGridLength(double value)
            : this(value, DataGridLengthUnitType.Pixel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridLength"/> struct based on a numerical value and a type.
        /// </summary>
        /// <param name="value">The value to hold.</param>
        /// <param name="type">The unit of <c>value</c>.</param>
        /// <remarks>
        ///     <c>value</c> is ignored unless <c>type</c> is
        ///     <c>DataGridLengthUnitType.Pixel</c> or
        ///     <c>DataGridLengthUnitType.Star</c>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     If <c>value</c> parameter is <c>double.NaN</c>
        ///     or <c>value</c> parameter is <c>double.NegativeInfinity</c>
        ///     or <c>value</c> parameter is <c>double.PositiveInfinity</c>.
        /// </exception>
        public DataGridLength(double value, DataGridLengthUnitType type)
            : this(value, type, type == DataGridLengthUnitType.Pixel ? value : double.NaN, type == DataGridLengthUnitType.Pixel ? value : double.NaN)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridLength"/> struct based on a numerical value and a unit.
        /// </summary>
        /// <param name="value">The value to hold.</param>
        /// <param name="type">The unit of <c>value</c>.</param>
        /// <param name="desiredValue">The desired value.</param>
        /// <param name="displayValue">The display value.</param>
        /// <remarks>
        ///     <c>value</c> is ignored unless <c>type</c> is
        ///     <c>DataGridLengthUnitType.Pixel</c> or
        ///     <c>DataGridLengthUnitType.Star</c>
        /// </remarks>
        /// <exception cref="ArgumentException">
        ///     If <c>value</c> parameter is <c>double.NaN</c>
        ///     or <c>value</c> parameter is <c>double.NegativeInfinity</c>
        ///     or <c>value</c> parameter is <c>double.PositiveInfinity</c>.
        /// </exception>
        public DataGridLength(double value, DataGridLengthUnitType type, double desiredValue, double displayValue)
        {
            if (double.IsNaN(value))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToNAN(nameof(value));
            }

            if (double.IsInfinity(value))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToInfinity(nameof(value));
            }

            if (double.IsInfinity(desiredValue))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToInfinity(nameof(desiredValue));
            }

            if (double.IsInfinity(displayValue))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToInfinity(nameof(displayValue));
            }

            if (value < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo(nameof(value), "value", 0);
            }

            if (desiredValue < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo(nameof(desiredValue), "desiredValue", 0);
            }

            if (displayValue < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo(nameof(displayValue), "displayValue", 0);
            }

            if (type != DataGridLengthUnitType.Auto &&
                type != DataGridLengthUnitType.SizeToCells &&
                type != DataGridLengthUnitType.SizeToHeader &&
                type != DataGridLengthUnitType.Star &&
                type != DataGridLengthUnitType.Pixel)
            {
                throw DataGridError.DataGridLength.InvalidUnitType(nameof(type));
            }

            _desiredValue = desiredValue;
            _displayValue = displayValue;
            _unitValue = (type == DataGridLengthUnitType.Auto) ? DATAGRIDLENGTH_DefaultValue : value;
            _unitType = type;
        }

        /// <summary>
        /// Gets a <see cref="DataGridLength"/> structure that represents the standard automatic sizing mode.
        /// </summary>
        /// <returns>
        /// A <see cref="DataGridLength"/> structure that represents the standard automatic sizing mode.
        /// </returns>
        public static DataGridLength Auto => _auto;

        /// <summary>
        /// Gets a <see cref="DataGridLength"/> structure that represents the cell-based automatic sizing mode.
        /// </summary>
        /// <returns>
        /// A <see cref="DataGridLength"/> structure that represents the cell-based automatic sizing mode.
        /// </returns>
        public static DataGridLength SizeToCells => _sizeToCells;

        /// <summary>
        /// Gets a <see cref="DataGridLength"/> structure that represents the header-based automatic sizing mode.
        /// </summary>
        /// <returns>
        /// A <see cref="DataGridLength"/> structure that represents the header-based automatic sizing mode.
        /// </returns>
        public static DataGridLength SizeToHeader => _sizeToHeader;

        /// <summary>
        /// Gets the desired value of this instance.
        /// </summary>
        public double DesiredValue => _desiredValue;

        /// <summary>
        /// Gets the display value of this instance.
        /// </summary>
        public double DisplayValue => _displayValue;

        /// <summary>
        /// Gets a value indicating whether this DataGridLength instance holds an absolute (pixel) value.
        /// </summary>
        public bool IsAbsolute => _unitType == DataGridLengthUnitType.Pixel;

        /// <summary>
        /// Gets a value indicating whether this DataGridLength instance is automatic (not specified).
        /// </summary>
        public bool IsAuto => _unitType == DataGridLengthUnitType.Auto;

        /// <summary>
        /// Gets a value indicating whether this DataGridLength instance is to size to the cells of a column or row.
        /// </summary>
        public bool IsSizeToCells => _unitType == DataGridLengthUnitType.SizeToCells;

        /// <summary>
        /// Gets a value indicating whether this DataGridLength instance is to size to the header of a column or row.
        /// </summary>
        public bool IsSizeToHeader => _unitType == DataGridLengthUnitType.SizeToHeader;

        /// <summary>
        /// Gets a value indicating whether this DataGridLength instance holds a weighted proportion of available space.
        /// </summary>
        public bool IsStar => _unitType == DataGridLengthUnitType.Star;

        /// <summary>
        /// Gets the <see cref="DataGridLengthUnitType"/> that represents the current sizing mode.
        /// </summary>
        public DataGridLengthUnitType UnitType => _unitType;

        /// <summary>
        /// Gets the absolute value of the <see cref="DataGridLength"/> in pixels.
        /// </summary>
        /// <returns>
        /// The absolute value of the <see cref="DataGridLength"/> in pixels.
        /// </returns>
        public double Value => _unitValue;

        /// <summary>
        /// Converts a string into a <see cref="DataGridLength"/> instance.
        /// </summary>
        /// <param name="value">string to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static DataGridLength ConvertFromString(string value)
        {
            return ConvertFrom(null, value);
        }

        /// <summary>
        /// Converts an object into a <see cref="DataGridLength"/> instance.
        /// </summary>
        /// <param name="culture">optional culture to use for conversion.</param>
        /// <param name="value">object to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static DataGridLength ConvertFrom(CultureInfo culture, object value)
        {
            if (value == null)
            {
                throw DataGridError.DataGridLengthConverter.CannotConvertFrom("(null)");
            }

            if (value is string stringValue)
            {
                stringValue = stringValue.Trim();

                if (stringValue.EndsWith(_starSuffix, StringComparison.Ordinal))
                {
                    string stringValueWithoutSuffix = stringValue[..^_starSuffix.Length];

                    double starWeight;
                    if (string.IsNullOrEmpty(stringValueWithoutSuffix))
                    {
                        starWeight = 1;
                    }
                    else
                    {
                        starWeight = Convert.ToDouble(stringValueWithoutSuffix, culture ?? CultureInfo.CurrentCulture);
                    }

                    return new DataGridLength(starWeight, DataGridLengthUnitType.Star);
                }

                for (int index = 0; index < _valueInvariantUnitStrings.Length; index++)
                {
                    if (stringValue.Equals(_valueInvariantUnitStrings[index], StringComparison.OrdinalIgnoreCase))
                    {
                        return _valueInvariantDataGridLengths[index];
                    }
                }
            }

            // Conversion from numeric type
            double doubleValue = Convert.ToDouble(value, culture ?? CultureInfo.CurrentCulture);
            if (double.IsNaN(doubleValue))
            {
                return Auto;
            }
            else
            {
                return new DataGridLength(doubleValue);
            }
        }

        /// <summary>
        /// Converts a <see cref="DataGridLength"/> instance into a string.
        /// </summary>
        /// <param name="culture">optional culture to use for conversion.</param>
        /// <param name="value">value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static string ConvertToString(CultureInfo culture, DataGridLength value)
        {
            // Convert dataGridLength to a string
            return value.UnitType switch
            {
                // for Auto print out "Auto". value is always "1.0"
                DataGridLengthUnitType.Auto => "Auto",
                DataGridLengthUnitType.SizeToHeader => "SizeToHeader",
                DataGridLengthUnitType.SizeToCells => "SizeToCells",
                // Star has one special case when value is "1.0".
                // in this case drop value part and print only "Star"
                DataGridLengthUnitType.Star => DoubleUtil.AreClose(1.0, value.Value)
                                        ? _starSuffix
                                        : Convert.ToString(value.Value, culture ?? CultureInfo.CurrentCulture) + _starSuffix,
                _ => Convert.ToString(value.Value, culture ?? CultureInfo.CurrentCulture),
            };
        }

        /// <summary>
        /// Overloaded operator, compares 2 DataGridLength's.
        /// </summary>
        /// <param name="gl1">first DataGridLength to compare.</param>
        /// <param name="gl2">second DataGridLength to compare.</param>
        /// <returns>true if specified DataGridLength have same value,
        /// unit type, desired value, and display value.</returns>
        public static bool operator ==(DataGridLength gl1, DataGridLength gl2)
        {
            return gl1.UnitType == gl2.UnitType &&
                   gl1.Value == gl2.Value &&
                   gl1.DesiredValue == gl2.DesiredValue &&
                   gl1.DisplayValue == gl2.DisplayValue;
        }

        /// <summary>
        /// Overloaded operator, compares 2 DataGridLength's.
        /// </summary>
        /// <param name="gl1">first DataGridLength to compare.</param>
        /// <param name="gl2">second DataGridLength to compare.</param>
        /// <returns>true if specified DataGridLength have either different value,
        /// unit type, desired value, or display value.</returns>
        public static bool operator !=(DataGridLength gl1, DataGridLength gl2)
        {
            return gl1.UnitType != gl2.UnitType ||
                   gl1.Value != gl2.Value ||
                   gl1.DesiredValue != gl2.DesiredValue ||
                   gl1.DisplayValue != gl2.DisplayValue;
        }

        /// <summary>
        /// Compares this instance of DataGridLength with another instance.
        /// </summary>
        /// <param name="other">DataGridLength length instance to compare.</param>
        /// <returns><c>true</c> if this DataGridLength instance has the same value
        /// and unit type as gridLength.</returns>
        public bool Equals(DataGridLength other)
        {
            return this == other;
        }

        /// <summary>
        /// Compares this instance of DataGridLength with another object.
        /// </summary>
        /// <param name="obj">Reference to an object for comparison.</param>
        /// <returns><c>true</c> if this DataGridLength instance has the same value
        /// and unit type as oCompare.</returns>
        public override bool Equals(object obj)
        {
            DataGridLength? dataGridLength = obj as DataGridLength?;
            if (dataGridLength.HasValue)
            {
                return this == dataGridLength;
            }

            return false;
        }

        /// <summary>
        /// Returns a unique hash code for this DataGridLength
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return (int)_unitValue + (int)_unitType + (int)_desiredValue + (int)_displayValue;
        }
    }
}