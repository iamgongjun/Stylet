﻿using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class ExecuteTests
    {
        [SetUp]
        public void SetUp()
        {
            // Dont want this being previously set by anything and messing us around
            Execute.TestExecuteSynchronously = false;
        }

        [Test]
        public void OnUIThreadSyncExecutesUsingDispatcher()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Send(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThreadSync(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadSyncExecutesSynchronouslyIfDispatcherIsCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            sync.SetupGet(x => x.IsCurrent).Returns(true);

            bool actionCalled = false;
            Execute.OnUIThreadSync(() => actionCalled = true);

            Assert.IsTrue(actionCalled);
            sync.Verify(x => x.Send(It.IsAny<Action>()), Times.Never);
        }

        [Test]
        public void PostToUIThreadExecutesUsingDispatcher()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.PostToUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void PostToUIThreadAsyncExecutesUsingDispatcher()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            var task = Execute.PostToUIThreadAsync(() => actionCalled = true);

            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadExecutesUsingDispatcherIfNotCurrent()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            Execute.OnUIThread(() => actionCalled = true);

            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
        }

        [Test]
        public void OnUIThreadAsyncExecutesAsynchronouslyIfDispatcherIsNotNull()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            bool actionCalled = false;
            var task = Execute.OnUIThreadAsync(() => actionCalled = true);

            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(actionCalled);
            passedAction();
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadSyncPropagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            var ex = new Exception("testy");
            sync.Setup(x => x.Send(It.IsAny<Action>())).Callback<Action>(a => a());

            Exception caughtEx = null;
            try { Execute.OnUIThreadSync(() => { throw ex; }); }
            catch (Exception e) { caughtEx = e; }

            Assert.IsInstanceOf<System.Reflection.TargetInvocationException>(caughtEx);
            Assert.AreEqual(ex, caughtEx.InnerException);
        }

        [Test]
        public void OnUIThreadAsyncPropagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            var ex = new Exception("test");
            var task = Execute.OnUIThreadAsync(() => { throw ex; });

            passedAction();
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(ex, task.Exception.InnerExceptions[0]);
        }

        [Test]
        public void PostToUIThreadAsyncPrepagatesException()
        {
            var sync = new Mock<IDispatcher>();
            Execute.Dispatcher = sync.Object;

            Action passedAction = null;
            sync.Setup(x => x.Post(It.IsAny<Action>())).Callback((Action a) => passedAction = a);

            var ex = new Exception("test");
            var task = Execute.PostToUIThreadAsync(() => { throw ex; });

            passedAction();
            Assert.IsTrue(task.IsFaulted);
            Assert.AreEqual(ex, task.Exception.InnerExceptions[0]);
        }

        [Test]
        public void ThrowsIfPostToUIThreadCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.PostToUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfPostToUIThreadAsyncCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.PostToUIThreadAsync(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThread(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadSyncCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThreadSync(() => { }));
        }

        [Test]
        public void ThrowsIfOnUIThreadAsyncCalledWithNoDispatcher()
        {
            Execute.Dispatcher = null;
            Assert.Throws<InvalidOperationException>(() => Execute.OnUIThreadAsync(() => { }));
        }

        [Test]
        public void PostToUIThreadExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            Execute.PostToUIThread(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void PostToUIThreadAsyncExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            var task = Execute.PostToUIThreadAsync(() => called = true);
            Assert.True(called);
            Assert.True(task.IsCompleted);
        }

        [Test]
        public void OnUIThreadSyncExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            Execute.OnUIThreadSync(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void OnUIThreadAsyncExecutesSynchronouslyIfTestExecuteSynchronouslySet()
        {
            Execute.TestExecuteSynchronously = true;

            Execute.Dispatcher = null;
            bool called = false;
            Execute.OnUIThreadAsync(() => called = true);
            Assert.True(called);
        }

        [Test]
        public void InDesignModeReturnsFalse()
        {
            Assert.False(Execute.InDesignMode);
        }
    }
}
