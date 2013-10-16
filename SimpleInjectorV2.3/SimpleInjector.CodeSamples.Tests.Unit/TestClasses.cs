﻿namespace SimpleInjector.CodeSamples.Tests.Unit
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public interface ILogger
    {
        void Log(string message);
    }

    public interface ICommand
    {
        void Execute();
    }

    public interface IValidator<T>
    {
        void Validate(T instance);
    }

    public sealed class InjectAttribute : Attribute
    {
    }

    public class CommandWithDependency : ICommand
    {
        public CommandWithDependency(ILogger logger)
        {
        }

        public void Execute()
        {
        }
    }

    public class ConcreteCommand : ICommand
    {
        public ConcreteCommand()
        {
        }

        public void Execute()
        {
        }
    }

    public sealed class NullLogger : ILogger
    {
        public void Log(string message)
        {
        }
    }

    public class NullValidator<T> : IValidator<T>
    {
        public void Validate(T instance)
        {
        }
    }

    public sealed class ListLogger : List<string>, ILogger
    {
        public void Log(string message)
        {
            this.Add(message);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this);
        }
    }
}