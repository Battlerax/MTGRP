using System;

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
                parameterinfo = new string[] {};

            Parameters = parameterinfo;
        }
    }
}
