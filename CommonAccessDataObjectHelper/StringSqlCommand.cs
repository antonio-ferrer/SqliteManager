using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommonAccessDataObjectHelper
{
    public class StringSqlCommand : IEnumerable<PackedParameter>
    {
        private readonly Dictionary<string, PackedParameter> parameters = new Dictionary<string, PackedParameter>(StringComparer.OrdinalIgnoreCase);
        private readonly string commandText;
        public static string DefaultParameterIdentifier
        {
            get;
            set;
        }
        public static bool DoNotUseDistinctWhenCatchParametersFromCommand
        {
            get;
            set;
        }
        static StringSqlCommand()
        {
            DefaultParameterIdentifier = "@";
            DoNotUseDistinctWhenCatchParametersFromCommand = true;
        }
        public object this[string parameterName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(parameterName))
                    throw new ArgumentException("invalid parameter name");
                if (parameters.ContainsKey(parameterName))
                    return parameters[parameterName].Value;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(parameterName))
                    throw new ArgumentException("invalid parameter name");
                if (parameters.ContainsKey(parameterName))
                {
                    parameters[parameterName].Value = value ?? Convert.DBNull;
                }
                else
                {
                    parameters.Add(parameterName, new PackedParameter { Name = parameterName, Value = value ?? Convert.DBNull });
                }
            }
        }

        public StringSqlCommand AddOrUpdate(string parameterName, object value)
        {
            this[parameterName] = value;
            return this;
        }

        public IEnumerator<PackedParameter> GetEnumerator()
        {
            return parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return parameters.Values.GetEnumerator();
        }

        private StringSqlCommand() { }

        public StringSqlCommand(string commantText, params object[] sequencedValues)
        {
            this.commandText = commantText;
            if (sequencedValues.Length > 0)
            {
                Regex rxParameters = new Regex($@"{DefaultParameterIdentifier}\w+");
                var parameterData = rxParameters.Matches(commantText).Cast<Match>()
                    .Select(m => m.Value.ToLower());
                string[] parametersNames;
                if (DoNotUseDistinctWhenCatchParametersFromCommand)
                {
                    parametersNames = parameterData.ToArray();
                }
                else
                {
                    parametersNames = parameterData.Distinct().ToArray();
                }

                if (parametersNames.Length != sequencedValues.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(sequencedValues), "the quantity of parameters must be the same size of the parameters values");
                }
                string key;
                for (int i = 0; i < sequencedValues.Length; i++)
                {
                    key = parametersNames[i];
                    parameters.Add(key, new PackedParameter { Name = key, Value = sequencedValues[i] });
                }
            }
        }

        public T[] ConvertParameters<T>(Func<PackedParameter, T> parse)
        {
            return parameters.Values.Select(p => parse(p)).ToArray();
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

        public PackedParameter GetParameter(string name) => (parameters.ContainsKey(name)) ? parameters[name] : null;

        public bool HasParameter => parameters.Count > 0;

    }
}
