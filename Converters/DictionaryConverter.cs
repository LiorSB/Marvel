using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Marvel.Converters
{
    class DictionaryConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
            {
                return null;
            }

            IDictionary dictionary = values[0] as IDictionary;

            if (dictionary == null || !dictionary.Contains(values[1]))
            {
                return null;
            }

            return dictionary[values[1]];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
