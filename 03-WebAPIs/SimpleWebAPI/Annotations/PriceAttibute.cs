using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

public class PriceAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null) return true; // Let [Required] handle nulls

        if (value is decimal price)
        {
            // Check if price is non-negative
            if (price < 0)
            {
                ErrorMessage = "Price must be a positive value.";
                return false;
            }

            // Check for max 2 decimal places
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
            if (decimalPlaces > 2)
            {
                ErrorMessage = "Price cannot have more than 2 decimal places.";
                return false;
            }

            return true;
        }

        return false;
    }
}