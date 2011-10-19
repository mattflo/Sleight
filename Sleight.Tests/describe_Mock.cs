using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Sleight.Tests
{
    class describe_Mock : nspec
    {
        Mock mock;

        dynamic asType;

        object result;

        void before_each()
        {
            mock = new Mock();

            asType = mock;
        }

        void calling_methods()
        {
            context["stubbing at the method level"] = () =>
            {
                act = () => result = asType.SayHello("Jane Doe");

                context["method has not been stubbed out"] = () =>
                {
                    it["returns null"] = () =>
                        result.should_be(null);
                };

                context["method has been stubbed"] = () =>
                {
                    before = () => mock.Stub("SayHello").Returns("Hello");

                    it["returns stubbed value"] = () =>
                        result.should_be("Hello");

                    context["subsequent stub of method"] = () =>
                    {
                        before = () => mock.Stub("SayHello").Returns("Hi");

                        it["redefines method to return new value"] = () =>
                            result.should_be("Hi");
                    };
                };
            };

            context["stubbing at the parameter level"] = () =>
            {
                before = () =>
                {
                    mock.Stub("SayHello").Returns("Hello");

                    mock.Stub("SayHello").WithParameters("John Doe").Returns("Hello John");
                };

                it["returns the value based on parameter granularity"] = () =>
                {
                    (asType.SayHello("John Doe") as object).should_be("Hello John");

                    (asType.SayHello("Anything Else") as object).should_be("Hello");
                };

                context["method is called without arguments"] = () =>
                {
                    it["defaults to the method level stub"] = () =>
                        (asType.SayHello() as object).should_be("Hello");
                };

                context["method level stub is redefined"] = () =>
                {
                    before = () => mock.Stub("SayHello").Returns("Hi");

                    it["returns redefined method level stub and original parameter stub"] = () =>
                    {
                        (asType.SayHello("John Doe") as object).should_be("Hello John");

                        (asType.SayHello("Anything Else") as object).should_be("Hi");
                    };
                };
            };
        }

        void recording_method_calls()
        {
            it["return null for last execution if method was never called"] = () =>
                    mock.ExecutionFor("SayHi").should_be(null);

            context["method called with one parameter"] = () => 
            {
                act = () => asType.SayHello("Jane Doe");

                it["records parameters used with method"] = () =>
                    mock.ExecutionFor("SayHello").Parameter.should_be("Jane Doe");

                context["method is stubbed"] = () =>
                {
                    before = () => mock.Stub("SayHello").Returns("Hello");

                    it["records value returned"] = () =>
                        mock.ExecutionFor("SayHello").ReturnValue.should_be("Hello");
                };

                context["subsquent call to method is preformed"] = () =>
                {
                    act = () => asType.SayHello("John Doe");

                    it["records the last call"] = () =>
                        mock.ExecutionFor("SayHello").Parameter.should_be("John Doe");
                };
            };

            context["method called with no parameters"] = () =>
            {
                act = () => asType.SayHello();

                it["recorded parameter is null"] = () =>
                    mock.ExecutionFor("SayHello").Parameter.should_be(null);
            };

            context["method called with multiple parameters"] = () =>
            {
                act = () => asType.SayHello("Jane", "Doe");

                it["recorded parameter is the first parameter"] = () =>
                    mock.ExecutionFor("SayHello").Parameter.should_be("Jane");

                it["recorded parameters has each parameter that was used"] = () =>
                {
                    mock.ExecutionFor("SayHello").Parameters[0].should_be("Jane");

                    mock.ExecutionFor("SayHello").Parameters[1].should_be("Doe");
                };
            };

            context["method is called with type arguments"] = () =>
            {
                act = () => asType.SayHello<string>("Jane");

                it["records type argument"] = () => 
                    mock.ExecutionFor("SayHello").TypeArgument.should_be(typeof(string));
            };

            context["method is called with multiple type arguments"] = () =>
            {
                act = () => asType.SayHello<string, int>("Jane");

                it["records type arguments"] = () =>
                {
                    mock.ExecutionFor("SayHello").TypeArguments[0].should_be(typeof(string));

                    mock.ExecutionFor("SayHello").TypeArguments[1].should_be(typeof(int));
                };
            };
        }
    }
}
