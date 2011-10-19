using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Sleight
{
    public class Mock : DynamicObject
    {
        string currentMember;

        string currentParameter;

        Dictionary<string, object> methodStubs;

        List<dynamic> parameterStubs;

        Dictionary<string, Execution> lastExecution;

        public Mock()
        {
            methodStubs = new Dictionary<string, object>();

            lastExecution = new Dictionary<string, Execution>();

            parameterStubs = new List<dynamic>();
        }

        public Mock WithParameters(string value)
        {
            currentParameter = value;

            return this;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = FindReturnValue(binder, args);

            RecordExecution(binder.Name, args, result, TypeArguments(binder));

            return true;
        }

        object FindReturnValue(InvokeMemberBinder binder, object[] args)
        {
            var paramResult = parameterStubs.FirstOrDefault(s => s.MethodName == binder.Name && s.Parameter == args.FirstOrDefault());

            if (paramResult != null) return paramResult.ReturnValue;

            return methodStubs.ValueOrNull(binder.Name);
        }

        void RecordExecution(string name, object[] args, object returnValue, Type[] typeArguments)
        {
            lastExecution[name] = new Execution
            {
                Parameter = args.FirstOrDefault(),
                Parameters = args,
                ReturnValue = returnValue,
                TypeArgument = typeArguments.FirstOrDefault(),
                TypeArguments = typeArguments
            };
        }

        public Mock Stub(string member)
        {
            currentMember = member;

            return this;
        }

        public Execution ExecutionFor(string member)
        {
            return lastExecution.ValueOrNull(member);
        }

        public void Returns(object value)
        {
            if (IsParameterMode())
            {
                parameterStubs.Add(new
                {
                    MethodName = currentMember,
                    Parameter = currentParameter,
                    ReturnValue = value
                });
            }

            else methodStubs[currentMember] = value;

            ResetMode();
        }

        void ResetMode()
        {
            currentParameter = null;
        }

        bool IsParameterMode()
        {
            return !string.IsNullOrEmpty(currentParameter);
        }

        private Type[] TypeArguments(InvokeMemberBinder binder)
        {
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");

            return (csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as IList<Type>).ToArray();
        }
    }
}