using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mtgvrp.core.Help
{
    class Help : Attribute
    {
        public readonly HelpManager.CommandGroups Group;
        public readonly string Description;
        public readonly string[] Parameters;

        public Help(HelpManager.CommandGroups group, string description, string[] parameterinfo)
        {
            Group = group;
            Description = description;

            if (parameterinfo == null)
                parameterinfo = new[] {"None"};
            Parameters = parameterinfo;
        }
    }
}
