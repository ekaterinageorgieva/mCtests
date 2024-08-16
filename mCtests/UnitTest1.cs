using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace mCtests
{
    public class Tests
    {

        private readonly static string BaseUrl = "http://moviecatalog-env.eba-ubyppecf.eu-north-1.elasticbeanstalk.com";
        private WebDriver driver;
        private Actions actions;
        private string? LastCreatedDescription;
        private string? LastCreatedTitle;

        [OneTimeSetUp]
        public void Setup()
        {
            
            var options = new ChromeOptions();
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument("--disable-search-engine-choice-screen");
            driver = new ChromeDriver(options);
            actions = new Actions(driver);
            driver.Navigate().GoToUrl(BaseUrl);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            driver.Navigate().GoToUrl($"{BaseUrl}/User/Login");
            var loginForm = driver.FindElement(By.XPath("//form[@method='post']"));
            actions.ScrollToElement(loginForm).Perform();

            driver.FindElement(By.Id("form2Example17")).SendKeys("ekaterinag@mail.com");
            driver.FindElement(By.Id("form2Example27")).SendKeys("123456");

            driver.FindElement(By.CssSelector(".btn")).Click();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }

        [Test, Order(1)]
        public void TestMovieWithoutTitle()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.CssSelector("input[name='Title']"));
            titleInput.Clear();
            titleInput.SendKeys("");

            var descriptionInput = driver.FindElement(By.CssSelector("textarea[name='Description']"));
            descriptionInput.Clear();
            descriptionInput.SendKeys("");

            driver.FindElement(By.XPath("//button[@class='btn warning']")).Click();

            var errorMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(errorMessage.Text, Is.EqualTo("The Title field is required."), "The error message for Title is not present");
        }

        [Test, Order(2)]
        public void TestMovieWithoutDescription()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            LastCreatedTitle = GenerateRandomString(6);
            titleInput.SendKeys(LastCreatedTitle);

            var descriptionInput = driver.FindElement(By.CssSelector("textarea[name='Description']"));
            descriptionInput.Clear();
            descriptionInput.SendKeys("");
            
            driver.FindElement(By.XPath("//button[@class='btn warning']")).Click();

            var errorMessage = driver.FindElement(By.XPath("//div[@class='toast-message']"));
            Assert.That(errorMessage.Text, Is.EqualTo("The Description field is required."), "The error message for Description is not present");
        }

        [Test, Order(3)]
        public void TestMovieWithRandomTitle()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/Add#add");

            var titleInput = driver.FindElement(By.XPath("//input[@name='Title']"));
            titleInput.Clear();
            LastCreatedTitle = GenerateRandomString(6);
            titleInput.SendKeys(LastCreatedTitle);

            var descriptionInput = driver.FindElement(By.CssSelector("textarea[name='Description']"));
            descriptionInput.Clear();
            LastCreatedDescription = GenerateRandomString(6);
            descriptionInput.SendKeys(LastCreatedDescription);

            driver.FindElement(By.XPath("//button[@class='btn warning']")).Click();

            //find last page
            IWebElement pagination = driver.FindElement(By.CssSelector("ul.pagination"));
            IList<IWebElement> pages = pagination.FindElements(By.CssSelector("li.page-item"));
            var lastPage = pages.Last();
            lastPage.FindElement(By.TagName("a")).Click();

            var movies = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieTitle = movies.Last().FindElement(By.XPath(".//h2")).Text;

            Assert.That(lastMovieTitle, Is.EqualTo(LastCreatedTitle.ToUpper()), "Title does not match");
        }

        [Test, Order(4)]
        public void EditLastAddedMovie_Test()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/All#all");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement pagination = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("ul.pagination")));

            IList<IWebElement> pages = pagination.FindElements(By.CssSelector("li.page-item"));
            var lastPage = pages.Last();
            lastPage.FindElement(By.TagName("a")).Click();

            wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='col-lg-4']")));

            var movies = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovie = movies.Last();

            var editButton = lastMovie.FindElement(By.XPath(".//a[@class='btn btn-outline-success']"));
            editButton.Click();

            var titleInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='Title']")));
            titleInput.Clear();

            string updatedTitle = "Updated title " + GenerateRandomString(6);
            titleInput.SendKeys(updatedTitle);

            var submitEditButton = driver.FindElement(By.XPath("//button[@class='btn warning' and @type='submit']"));
            submitEditButton.Click();

            IWebElement successfulMessageDescription = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='toast-message']")));

            Assert.That(successfulMessageDescription.Text.Trim(), Is.EqualTo("The Movie is edited successfully!"), "Successful message is missing");
        }

        [Test, Order(6)]
        public void DeleteLastAddedMovie_Test()
        {
            driver.Navigate().GoToUrl($"{BaseUrl}/Catalog/All#all");

            IWebElement pagination = driver.FindElement(By.CssSelector("ul.pagination"));
            IList<IWebElement> pages = pagination.FindElements(By.CssSelector("li.page-item"));
            var lastPage = pages.Last();
            lastPage.FindElement(By.TagName("a")).Click();

            var movies = driver.FindElements(By.XPath("//div[@class='col-lg-4']"));
            var lastMovieTitle = movies.Last().FindElement(By.XPath(".//h2"));

            var deleteButton = lastMovieTitle.FindElement(By.XPath("//div[@class='pt-1 mb-4']//a[@class='btn btn-danger']"));
            deleteButton.Click();

            var confirmDeleteButton = driver.FindElement(By.XPath("//div[@class='pt-1 mb-4']//button"));
            confirmDeleteButton.Click();

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement confirmMessageDeletion = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='toast toast-success']")));

            Assert.That(confirmMessageDeletion.Text.Trim(), Is.EqualTo("The Movie is deleted successfully!"), "Confirmation message is missing");
        }
        

        public string GenerateRandomString(int length)
        {
            const string chars = "qazwsxedcrfv";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}