using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PWSWebApi
{
    public class Util
    {
        public static string GetConvertedCharacter(string input)
        {
            switch (input)
            {
                case "1":
                    return "!";
                case "2":
                    return "@";
                case "3":
                    return "#";
                case "4":
                    return "$";
                case "5":
                    return "%";
                case "6":
                    return "^";
                case "7":
                    return "&";
                case "8":
                    return "*";
                default:
                    return string.Empty;
            } 
        }

        public static DateTime GetEasternDateTime()
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
        }

        public static string ReturnDecimalString(string value, int decimalPoint)
        {
            if(value == null)
            {
                return value;
            }
            decimal decimalValue;
            bool isNumber = Decimal.TryParse(value, out decimalValue);
            
            if(isNumber)
            {
                var r = Decimal.Round(decimalValue, decimalPoint).ToString();
                return r;
            }
            else
            {
                int intValue;
                isNumber = int.TryParse(value, out intValue);
                if (isNumber)
                {
                    return Decimal.Round(intValue, decimalPoint).ToString();
                }
            }
            return value;
        }
    }
}