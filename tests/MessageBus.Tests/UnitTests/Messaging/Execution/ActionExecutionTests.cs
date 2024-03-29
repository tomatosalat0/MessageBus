﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MessageBus.Messaging.InProcess.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Messaging.Tests.UnitTests.Execution
{
    [TestClass]
    public class ActionExecutionTests
    {
        [TestMethod]
        public void TryWaitForWorkReturnsFalseWhenEmptyExecutable()
        {
            ActionExecution executor = new ActionExecution(new EmptyExecutable());
            Assert.IsFalse(executor.HasWork());
            Assert.IsFalse(executor.TryWaitForWork(TimeSpan.FromSeconds(1), CancellationToken.None, out _));
        }

        [TestMethod]
        public void TryWaitForWorkReturnsFalseWhenCompletedExecutable()
        {
            ActionExecution executor = new ActionExecution(new CompletedExecutable());
            Assert.IsFalse(executor.HasWork());
            Assert.IsFalse(executor.TryWaitForWork(TimeSpan.FromSeconds(1), CancellationToken.None, out _));
        }

        [TestMethod]
        public void ActionCollectorWillExecuteWork()
        {
            NeverEmptyExecutable collectable = new NeverEmptyExecutable();
            ActionExecution executor = new ActionExecution(collectable);
            Assert.IsTrue(executor.HasWork());
            Assert.IsTrue(executor.TryWaitForWork(TimeSpan.FromSeconds(1), CancellationToken.None, out var workToExecute));
            Assert.IsNotNull(workToExecute);
            workToExecute.Execute();
            Assert.AreEqual(1, collectable.Executions);
        }

        private class EmptyExecutable : IExecutable
        {
            public bool IsCompleted => false;

            public bool HasExecutables => false;

            public bool TryTake([NotNullWhen(true)] out Action action, TimeSpan timeout, CancellationToken cancellationToken)
            {
                action = null;
                return false;
            }
        }

        private class CompletedExecutable : IExecutable
        {
            public bool IsCompleted => true;

            public bool HasExecutables => false;

            public bool TryTake([NotNullWhen(true)] out Action action, TimeSpan timeout, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException();
            }
        }

        private class NeverEmptyExecutable : IExecutable
        {
            public int Executions { get; private set; }

            public bool IsCompleted => false;

            public bool HasExecutables => true;

            public bool TryTake([NotNullWhen(true)] out Action action, TimeSpan timeout, CancellationToken cancellationToken)
            {
                action = () => { Executions++; };
                return true;
            }
        }
    }
}
