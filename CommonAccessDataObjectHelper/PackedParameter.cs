using System;
using System.Collections.Generic;
using System.Text;

namespace CommonAccessDataObjectHelper
{
    public class PackedParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsOutput { get; set; }
        public override string ToString()
        {
            return $"{Name}={Value}";
        }
        public static implicit operator KeyValuePair<string, object>(PackedParameter p)=>new KeyValuePair<string, object>(p.Name, p.Value);
        public static implicit operator PackedParameter(KeyValuePair<string, object> p) => new PackedParameter { Name = p.Key, Value = p.Value };
        public bool EqualName(string name) => StringComparer.OrdinalIgnoreCase.Equals(name, this.Name);
    }
}
