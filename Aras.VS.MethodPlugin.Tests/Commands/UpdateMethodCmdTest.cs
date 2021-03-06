﻿using System;
using System.IO;
using System.Threading;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
	[TestFixture]
	[Apartment(ApartmentState.STA)]
	public class UpdateMethodCmdTest
	{
		IAuthenticationManager authManager;
		IProjectManager projectManager;
		IDialogFactory dialogFactory;
		ProjectConfigurationManager projectConfigurationManager;
		UpdateMethodCmd updateMethodCmd;
		IVsUIShell iVsUIShell;
		ICodeProviderFactory codeProviderFactory;
		MessageManager messageManager;
		TemplateLoader templateLoader;
		ICodeProvider codeProvider;

		[SetUp]
		public void Init()
		{
			projectManager = Substitute.For<IProjectManager>();
			projectConfigurationManager = Substitute.For<ProjectConfigurationManager>();
			dialogFactory = Substitute.For<IDialogFactory>();
			authManager = Substitute.For<IAuthenticationManager>();
			codeProviderFactory = Substitute.For<ICodeProviderFactory>();
			codeProvider = Substitute.For<ICodeProvider>();
			codeProviderFactory.GetCodeProvider(null).ReturnsForAnyArgs(codeProvider);
			messageManager = Substitute.For<MessageManager>();
			UpdateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			updateMethodCmd = UpdateMethodCmd.Instance;
			iVsUIShell = Substitute.For<IVsUIShell>();
			var currentPath = AppDomain.CurrentDomain.BaseDirectory;
			projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
			projectConfigurationManager.Load(projectManager.ProjectConfigPath);
			projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
			templateLoader = new TemplateLoader();
			templateLoader.Load(projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath);
			projectManager.MethodPath.Returns(Path.Combine(currentPath, "TestData\\TestMethod.txt"));
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGetUpdateFromArasView()
		{
			// Arrange
			dialogFactory.GetUpdateFromArasView(null, null, null, null, null, null, null).ReturnsForAnyArgs(new UpdateFromArasViewAdapterTest());
			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());

			//Act
			updateMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			dialogFactory.Received().GetUpdateFromArasView(projectConfigurationManager, Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(),
				Arg.Any<MethodInfo>(), projectManager.ProjectConfigPath, string.Empty, string.Empty);
		}

		[Test]
		[Ignore("Should be updated")]
		public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
		{
			// Arrange
			var updateFromArasViewAdapterTest = Substitute.For<UpdateFromArasViewAdapterTest>();
			dialogFactory.GetUpdateFromArasView(null, null, null, null, null, null, null).ReturnsForAnyArgs(updateFromArasViewAdapterTest);
			codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
			var showDialogResult = updateFromArasViewAdapterTest.ShowDialog();

			//Act
			updateMethodCmd.ExecuteCommandImpl(null, null);

			// Assert
			codeProvider.Received().GenerateCodeInfo(Arg.Any<TemplateInfo>(), Arg.Any<EventSpecificDataType>(), showDialogResult.MethodName, showDialogResult.MethodCode, false);
		}

		public class UpdateFromArasViewAdapterTest : IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult>
		{
			public UpdateFromArasViewResult ShowDialog()
			{
				return new UpdateFromArasViewResult
				{
					DialogOperationResult = true,
					MethodCode = string.Empty,
					MethodComment = string.Empty,
					MethodName = string.Empty,
					MethodConfigId = string.Empty,
					MethodId = string.Empty,
					Package = new PackageInfo(string.Empty),
					MethodType = string.Empty,
					MethodLanguage = string.Empty,
					SelectedTemplate = new TemplateInfo { TemplateName = string.Empty },
					ExecutionIdentityId = string.Empty,
					ExecutionIdentityKeyedName = string.Empty,
					EventSpecificData = EventSpecificData.None,
					IsUseVSFormattingCode = false
				};
			}
		}
	}
}
