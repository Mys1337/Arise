using OpenQA.Selenium;

internal class SelectElement
{
    private IWebElement webElement;

    public SelectElement(IWebElement webElement)
    {
        this.webElement = webElement;
    }

    public object Options { get; internal set; }
}