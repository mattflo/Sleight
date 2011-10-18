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

        Dictionary<string, object> values;

        Dictionary<string, Execution> lastExecution;

        public Mock()
        {
            values = new Dictionary<string, object>();

            lastExecution = new Dictionary<string, Execution>();
        }

        public Mock WithParameters(string value)
        {
            return this;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = values.ValueOrNull(binder.Name);

            RecordExecution(binder.Name, args, result, TypeArguments(binder));

            return true;
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

            values[member] = null;

            return this;
        }

        public Execution ExecutionFor(string member)
        {
            return lastExecution.ValueOrNull(member);
        }

        public void Returns(object value)
        {
            values[currentMember] = value;
        }

        private Type[] TypeArguments(InvokeMemberBinder binder)
        {
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");

            return (csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as IList<Type>).ToArray();
        }
    }
}