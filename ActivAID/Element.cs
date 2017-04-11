using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    public struct Element
    {

        public bool isText;
        public string data;
        public string name;
        public Element(bool isText, string data, string name)
        {
            this.isText = isText;
            this.data = data;
            this.name = name;
        }
    }
}
