using System;

namespace Zenject
{
    [Serializable]
    [ZenjectAllowDuringValidation]
    public class ZenjectSettings
    {
        public static ZenjectSettings Default = new ZenjectSettings();

        // Setting this to Log can be more useful because it will print out
        // multiple validation errors at once so you can fix multiple problems before
        // attempting validation again
        public ValidationErrorResponses ValidationErrorResponse = ValidationErrorResponses.Log;

        // Settings this to true will ensure that every binding in the container can be
        // instantiated with all its dependencies, and not just those bindings that will be
        // constructed as part of the object graph generated from the nonlazy bindings
        public bool ResolveOnlyRootsDuringValidation = true;

        public bool DisplayWarningWhenResolvingDuringInstall = true;

        public enum ValidationErrorResponses
        {
            Log,
            Throw,
        }
    }
}
