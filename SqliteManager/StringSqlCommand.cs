using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqliteManager
{
    public class StringSqlCommand : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private readonly string commandText;
        public static string DefaultParameterIdentifier
        {
            get;
            set;
        }
        static StringSqlCommand()
        {
            DefaultParameterIdentifier = "@";
        }
        public object this[string parameterName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(parameterName))
                    throw new ArgumentException("invalid parameter name");
                if(parameters.ContainsKey(parameterName))
                    return parameters[parameterName];
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(parameterName))
                    throw new ArgumentException("invalid parameter name");
                if (parameters.ContainsKey(parameterName))
                {
                    parameters[parameterName] = value ?? Convert.DBNull;
                }
                else
                {
                    parameters.Add(parameterName, value ?? Convert.DBNull);
                }
            }
        }

        public StringSqlCommand AddOrUpdate(string parameterName, object value)
        {
            this[parameterName] = value;
            return this;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        private StringSqlCommand() { }

        public StringSqlCommand(string commantText, params object[] sequencedValues)
        {
            this.commandText = commantText;
            if (sequencedValues.Length > 0)
            {
                Regex rxParameters = new Regex($@"{DefaultParameterIdentifier}\w+");
                var parametersNames = rxParameters.Matches(commantText).Cast<Match>()
                    .Select(m => m.Value.ToLower()).Distinct().ToArray();
                if (parametersNames.Length != sequencedValues.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(sequencedValues), "the quantity of parameters must be the same size of the parameters values");
                }
                for (int i = 0; i < sequencedValues.Length; i++)
                {
                    parameters.Add(parametersNames[i], sequencedValues[i]);
                }
            }
        }

        public T[] ConvertParameters<T>(Func<KeyValuePair<string,object>, T> parse)
        {
            return parameters.Select(p => parse(p)).ToArray();
        }

        public static implicit operator string(StringSqlCommand cmd)
        {
            return cmd.commandText;
        }

        public static implicit operator StringSqlCommand(string cmd)
        {
            return new StringSqlCommand(cmd);
        }

        public override string ToString()
        {
            return commandText;
        }

        public bool HasParameter => parameters.Count > 0;

    }
}
