using System;
using System.Collections.Generic;

namespace Zenject
{
    public enum ScopeTypes
    {
        Unset,
        Transient,
        Singleton,
    }

    public enum ToChoices
    {
        Self,
        Concrete,
    }

    public enum InvalidBindResponses
    {
        Assert,
        Skip,
    }

    public enum BindingInheritanceMethods
    {
        None,
        CopyIntoAll,
        CopyDirectOnly,
        MoveIntoAll,
        MoveDirectOnly,
    }

    public class BindInfo
    {
        public BindInfo()
        {
            ContextInfo = null;
            Identifier = null;
            ContractTypes = new List<Type>();
            ToTypes = new List<Type>();
            Arguments = new List<TypeValuePair>();
            ToChoice = ToChoices.Self;
            BindingInheritanceMethod = BindingInheritanceMethods.None;
            OnlyBindIfNotBound = false;
            SaveProvider = false;
            ProviderIdentifier = null;

            // Change this to true if you want all dependencies to be created at the start
            NonLazy = false;

            MarkAsUniqueSingleton = false;
            MarkAsCreationBinding = true;

            Scope = ScopeTypes.Unset;
            InvalidBindResponse = InvalidBindResponses.Assert;
        }

        public bool MarkAsCreationBinding
        {
            get;
            set;
        }

        public bool MarkAsUniqueSingleton
        {
            get;
            set;
        }

        public object ProviderIdentifier
        {
            get;
            set;
        }

        public bool SaveProvider
        {
            get;
            set;
        }

        public bool OnlyBindIfNotBound
        {
            get;
            set;
        }

        public string ContextInfo
        {
            get;
            set;
        }

        public bool RequireExplicitScope
        {
            get;
            set;
        }

        public object Identifier
        {
            get;
            set;
        }

        public List<Type> ContractTypes
        {
            get;
            set;
        }

        public BindingInheritanceMethods BindingInheritanceMethod
        {
            get;
            set;
        }

        public InvalidBindResponses InvalidBindResponse
        {
            get;
            set;
        }

        public bool NonLazy
        {
            get;
            set;
        }

        public BindingCondition Condition
        {
            get;
            set;
        }

        public ToChoices ToChoice
        {
            get;
            set;
        }

        // Only relevant with ToChoices.Concrete
        public List<Type> ToTypes
        {
            get;
            set;
        }

        public ScopeTypes Scope
        {
            get;
            set;
        }

        public List<TypeValuePair> Arguments
        {
            get;
            set;
        }
    }
}
