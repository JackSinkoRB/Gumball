using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class OrdinalString
    {

        public static string ToOrdinalString(this int number)
        {
            if (number <= 0)
                return number.ToString();

            string suffix;

            int lastTwoDigits = number % 100;
            int lastDigit = number % 10;

            //handle the "teen" numbers
            if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
            {
                suffix = "th";
            }
            else
            {
                //assign suffix based on the last digit
                suffix = lastDigit switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };
            }

            return number + suffix;
        }
        
    }
}
