using System;

namespace Zenject
{
    public class SubContainerCreatorBindInfo
    {
        // Null = means no custom default parent
        public string DefaultParentName
        {
            get; set;
        }
    }
}
