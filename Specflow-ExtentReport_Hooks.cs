using TechTalk.SpecFlow;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Gherkin.Model;
using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using VismaIdella.Vips.EmployerPortalUITests.Infrastructure;

namespace VismaIdella.Vips.EmployerPortalUITests.Hooks
{
    [Binding]
    public class ReportHooks
    {
        private static ExtentTest featureName;
        private static ExtentTest scenario;
        private static ExtentReports extent;
        private static IWebDriver driver;
        private static string ScreenshotsDir;

        private const string Given = "Given";
        private const string When = "When";
        private const string Then = "Then";
        private const string And = "And";

        private string relativeScreenshotPath = ".\\Screenshots\\";
        public ReportHooks(IWebDriver driver)
        {
            ReportHooks.driver = driver;
        }
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            string endPath = "\\TestReports\\";
            var directoryInfo = Variables.ResultPath;
            var filePath = directoryInfo + endPath;
            ScreenshotsDir = filePath + "Screenshots\\";
            Console.WriteLine("test report directory: " + filePath);
            ExtentHtmlReporter htmlReporter = new ExtentHtmlReporter(filePath);
            htmlReporter.Config.ReportName = Variables.Environment;
            extent = new ExtentReports();
            extent.AttachReporter(htmlReporter);
            extent.AddSystemInfo("Environment", Variables.Environment);

        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            //CreateUploadFilesPath dynamic feature name
            featureName = extent.CreateTest<Feature>(featureContext.FeatureInfo.Title);
            Console.WriteLine("BeforeFeature");
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            Console.WriteLine("Scenario: " + scenarioContext.ScenarioInfo.Title);
            scenario = featureName.CreateNode<Scenario>(scenarioContext.ScenarioInfo.Title);
        }

        [AfterStep]
        public void InsertReportingSteps(ScenarioContext scenarioContext)
        {
            var stepType = ScenarioStepContext.Current.StepInfo.StepDefinitionType.ToString();
            if (scenarioContext.TestError == null)
            {
                if (stepType == Given)
                    scenario.CreateNode<Given>(ScenarioStepContext.Current.StepInfo.Text);
                else if(stepType == When)
                                scenario.CreateNode<When>(ScenarioStepContext.Current.StepInfo.Text);
                else if(stepType == Then)
                                scenario.CreateNode<Then>(ScenarioStepContext.Current.StepInfo.Text);
                else if(stepType == And)
                                scenario.CreateNode<And>(ScenarioStepContext.Current.StepInfo.Text);
            }
            else if(scenarioContext.TestError != null)
            {
                if (stepType == Given)
                {
                    scenario.CreateNode<Given>(ScenarioStepContext.Current.StepInfo.Text)
                        .Fail(scenarioContext.TestError.Message);
                    Report(scenarioContext);
                }
                else if(stepType == When)
                {
                    scenario.CreateNode<When>(ScenarioStepContext.Current.StepInfo.Text).Fail(scenarioContext.TestError.Message);
                    Report(scenarioContext);
                }
                else if(stepType == Then) {
                    scenario.CreateNode<Then>(ScenarioStepContext.Current.StepInfo.Text).Fail(scenarioContext.TestError.Message);
                    Report(scenarioContext);
                }
                else if(stepType == And)
                {
                    scenario.CreateNode<And>(ScenarioStepContext.Current.StepInfo.Text).Fail(scenarioContext.TestError.Message);
                    Report(scenarioContext);
                }
            }
        }
        [AfterScenario]
        public void AfterScenario()
        {
            Console.WriteLine("AfterScenario");
            //implement logic that has to run after executing each scenario
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            //kill the browser
            //Flush report once test completes
            extent.Flush();
        }

        public static string CreateScreenshotName(string context)
        {
            return $"{TimeStamp()}-{context}.png";
        }

        private static string CaptureScreenhot(IWebDriver driver, string fileName)
        {
            Directory.CreateDirectory(ScreenshotsDir);
            string screenshotPath = ScreenshotsDir + fileName;
            var screenshotDriver = driver as ITakesScreenshot;
            if (screenshotDriver != null)
            {
                OpenQA.Selenium.Screenshot screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);
            }
            return fileName;
        }

        private static string TimeStamp()
        {
            return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
        }

        private void Report(ScenarioContext scenarioContext)
        {
            string title = CreateScreenshotName(scenarioContext.ScenarioInfo.Title);
            string screenshot = CaptureScreenhot(driver, title);
            scenario.AddScreenCaptureFromPath(relativeScreenshotPath + screenshot);
            TestContext.AddTestAttachment(ScreenshotsDir + screenshot);
        }
    }
}
