using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Netframe.Tool
{
    public class Tool
    {
        public static string GetWeather(string city)
        {
            Weather.WeatherWebServiceSoapClient w = new Weather.WeatherWebServiceSoapClient("WeatherWebServiceSoap");

            string[] weatherInfo = w.getWeatherbyCityName(city);
            string ret = "";
            foreach (var item in weatherInfo)
            {
                ret += item.ToString();

            }
            return ret;
        }

    }
}
