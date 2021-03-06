﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using ServiceStack;

namespace ServiceStackVS.ExternalTemplateWizard
{
    public class IisExpressAddressWizard : IWizard
    {
        private const string iisPortToken = "$iisexpressport$";

        private string fullPathTokenReplaceFile = null;
        private string projectName = null;
        private string projectFilePath = null;

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (runKind != WizardRunKind.AsNewProject)
            {
                throw new InvalidOperationException("IisExpressAddressWizard only to be used with new web project templates");
            }
            projectName = replacementsDictionary["$safeprojectname$"];

            string wizardData = replacementsDictionary["$wizarddata$"];
            XElement element = XElement.Parse("<WizardData>" + wizardData + "</WizardData>");
            XElement iisExpressWizardDataRoot = null;
            if (element.Descendants().FirstOrDefault(x => x.Name.LocalName == "IISExpressAddress") != null)
            {
                iisExpressWizardDataRoot =
                    element.Descendants().First(x => x.Name.LocalName == "IISExpressAddress");
            }
            if (iisExpressWizardDataRoot == null)
            {
                return;
            }

            var relativeFilePath = iisExpressWizardDataRoot.GetAttributeValue("Name");
            var solutionPath = Path.GetDirectoryName(replacementsDictionary["$destinationdirectory$"]);
            if (solutionPath == null)
            {
                throw new WizardCancelledException("Failed to generated project, IISExpressAddressWizard failed to obtain solution path.");
            }
            var projectPath = Path.Combine(solutionPath, projectName);
            projectFilePath = Path.Combine(projectPath, projectName + ".csproj");


            fullPathTokenReplaceFile = Path.Combine(projectPath, relativeFilePath);
        }

        public void ProjectFinishedGenerating(Project project)
        {
            
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            
        }

        public void RunFinished()
        {
            Task.Run(() =>
            {
                // IIS express auto assigned port is not ready until project has been opened so a delay is needed.
                System.Threading.Thread.Sleep(5000);
                // Read port from csproj
                XElement element = XElement.Parse(File.ReadAllText(projectFilePath));
                var webProperties = element.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "ProjectExtensions")?.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "VisualStudio")?.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName == "WebProjectProperties")?.Descendants();

                var iisUrl = webProperties?
                    .FirstOrDefault(x => x.Name.LocalName == "IISUrl");
                if (iisUrl != null)
                {
                    var originalContent = File.ReadAllText(fullPathTokenReplaceFile);
                    File.WriteAllText(fullPathTokenReplaceFile, 
                        originalContent.ReplaceAll("http://localhost:$iisexpressport$", iisUrl.Value));
                    return;
                }

                var devServerPortElement = webProperties?
                    .FirstOrDefault(x => x.Name.LocalName == "DevelopmentServerPort");
                if (devServerPortElement != null)
                {
                    var devPort = devServerPortElement.Value;
                    var originalContent = File.ReadAllText(fullPathTokenReplaceFile);
                    File.WriteAllText(fullPathTokenReplaceFile,
                        originalContent.ReplaceAll(iisPortToken, devPort));
                }
            });
        }
    }
}
