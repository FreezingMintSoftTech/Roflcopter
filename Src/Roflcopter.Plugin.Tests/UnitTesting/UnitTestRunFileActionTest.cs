﻿using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Components;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.DataContext;
using JetBrains.ReSharper.TestFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Criteria;
using NUnit.Framework;
using Roflcopter.Plugin.UnitTesting;

namespace Roflcopter.Plugin.Tests.UnitTesting
{
    [TestFixture]
    [TestNetFramework4]
    public class UnitTestRunFileActionTest : BaseTestWithSingleProject
    {
        // IDEA: Refactor to BaseTestWithTextControl and use the real test elements

        protected override string RelativeTestDataPath => Path.Combine(base.RelativeTestDataPath, "..");

        [Test]
        public void GetElementsToRun_ReturnsProjectFileCriterion()
        {
            Test(test =>
            {
                var dataContext = Add(CreateEmptyDataContext(), test.ProjectFile);

                //

                var result = test.Sut.GetElementsToRun(dataContext);

                //

                Assert.That(result, Is.Not.Null);
                var projectFileCriterion = (ProjectFileCriterion) result.Criterion;
#if RS20182
                Assert.That(projectFileCriterion.Location, Is.EqualTo(test.ProjectFile.Location));
#else
                Assert.That(projectFileCriterion.PersistentId, Is.EqualTo(test.ProjectFile.GetPersistentID()));
#endif
                Assert.That(result.Explicit, Is.Empty);
            });
        }

        [Test]
        public void GetElementsToRun_WithEmptyDataContext()
        {
            Test(test =>
            {
                var dataContext = CreateEmptyDataContext();

                //

                var result = test.Sut.GetElementsToRun(dataContext);

                //

                Assert.That(result, Is.Null);
            });
        }

        private void Test(Action<(TestUnitTestRunFileAction Sut, IProjectFile ProjectFile)> action)
        {
            WithSingleProject(
                GetTestDataFilePath2("SampleTest.cs").FullPath,
                (lifetime, solution, project) => RunGuarded(() =>
                {
                    var projectFile = project.GetAllProjectFiles().Single();

                    var sut = new TestUnitTestRunFileAction();

                    action((sut, projectFile));
                }));
        }

        private IDataContext CreateEmptyDataContext() => ShellInstance.GetComponent<DataContexts>().Empty;

        private IDataContext Add(IDataContext dataContext, IProjectFile projectFile)
        {
            return ShellInstance.GetComponent<DataContexts>().CloneWithAdditionalDataRules(
                TestFixtureLifetime,
                dataContext,
                DataRules.AddRule("<test data rule>", ProjectModelDataConstants.PROJECT_MODEL_ELEMENT, projectFile));
        }

        private class TestUnitTestRunFileAction : UnitTestRunFileAction
        {
            [CanBeNull]
            public new UnitTestElements GetElementsToRun(IDataContext context) => base.GetElementsToRun(context);
        }
    }
}
