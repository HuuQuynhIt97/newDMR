using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ScalingMachineService.Helpers
{
   public class StringFormat
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>Trả về đơn vị đo khối lượng</returns>
        public static string UnitOfMass(string unit)
        {
            string value;
            if (unit == null || unit == string.Empty)
            {
               return string.Empty;
            }
            value = unit;
            string result = new Regex(@"[a-zA-Z]").Match(unit).Value.ToString();
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitValue"></param>
        /// <returns>Trả về giá trị đơn vị đo khối lượng</returns>
        public static float UnitOfMassValue(string unitValue)
        {
            string value;
            if (unitValue == null || unitValue == string.Empty)
            {
                return 0;
            }
            value = unitValue;
            float result = new Regex(@"[+-]?([0-9]*[.])?[0-9]+").Match(unitValue).Value.ToFloat();
            return result;
        }
    }
}
