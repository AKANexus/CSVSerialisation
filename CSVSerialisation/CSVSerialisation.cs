using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CSVSerialisation.Exceptions;

namespace CSVSerialisation
{
    public class CSVDeserialiser<T> where T : new()
    {
        public IEnumerable<T> Deserialise(string csvString, bool hasHeaders, char divider, CultureInfo culture)
        {
            List<string[]> linhas = new();
            List<PropertyInfo> columns = new();
            Type t = typeof(T);
            List<T> resultado = new();
            var propriedadesDeT = t.GetProperties();

            foreach (string s in csvString.Split(Environment.NewLine))
            {
                linhas.Add(s.Split(divider));
            }

            if (hasHeaders)
            {
                columns.AddRange(linhas[0].Select(header => propriedadesDeT.FirstOrDefault(x => x.Name == header.TrimEnd(divider))).Where(prop => prop is not null));
                linhas.RemoveAt(0);
            }

            foreach (string[] linha in linhas)
            {
                T novoT = new();
                for (int i = 0; i < linha.Length; i++)
                {
                    if (columns[i].PropertyType == typeof(string))
                        columns[i].SetValue(novoT, linha[i].TrimEnd(divider));
                    else if (columns[i].PropertyType == typeof(DateTime))
                    {
                        if (!DateTime.TryParse(linha[i].TrimEnd(divider), culture, DateTimeStyles.AllowWhiteSpaces,
                            out DateTime dateTime))
                        {
                            throw new TypeMismatchException(columns[i], linha[i].TrimEnd(divider));
                        }
                        columns[i].SetValue(novoT, dateTime);
                    }
                    else if (columns[i].PropertyType == typeof(decimal))
                    {
                        if (!decimal.TryParse(linha[i].TrimEnd(divider), NumberStyles.Any, culture, out decimal decResult))
                        {
                            throw new TypeMismatchException(columns[i], linha[i].TrimEnd(divider));
                        }
                        columns[i].SetValue(novoT, decResult);

                    }
                    else if (columns[i].PropertyType == typeof(double))
                    {
                        if (!double.TryParse(linha[i].TrimEnd(divider), NumberStyles.Any, culture, out double douResult))
                        {
                            throw new TypeMismatchException(columns[i], linha[i].TrimEnd(divider));
                        }
                        columns[i].SetValue(novoT, douResult);
                    }
                    else if (columns[i].PropertyType == typeof(float))
                    {
                        if (!float.TryParse(linha[i].TrimEnd(divider), NumberStyles.Any, culture, out float floResult))
                        {
                            throw new TypeMismatchException(columns[i], linha[i].TrimEnd(divider));
                        }
                        columns[i].SetValue(novoT, floResult);
                    }
                }
                resultado.Add(novoT);
            }

            return resultado;
        }

        public string Serialise(List<T> listObj, char divider, CultureInfo culture)
        {
            StringBuilder sb = new();
            Type t = typeof(T);
            var propriedadesDeT = t.GetProperties();
            foreach (PropertyInfo propertyInfo in propriedadesDeT)
            {
                sb.Append(propertyInfo.Name);
                sb.Append(';');
            }

            sb.Length--;
            sb.Append(Environment.NewLine);
            foreach (T entry in listObj)
            {
                foreach (PropertyInfo propertyInfo in propriedadesDeT)
                {
                    sb.Append(propertyInfo.GetValue(entry));
                    sb.Append(divider);
                }

                sb.Length--;
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }

}

namespace CSVSerialisation.Exceptions
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(PropertyInfo property, string readValue) : base($"Entry could not be converted. Expected type: {property.PropertyType}; provided value: {readValue}")
        {

        }

    }

    public class UnsupportedTypeException : Exception

    {
        public UnsupportedTypeException(PropertyInfo property) : base($"Provided property is not currently supported: {property.PropertyType}")
        {

        }
    }
}
