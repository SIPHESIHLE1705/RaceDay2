
using Microsoft.AspNetCore.Mvc;
using Race.Controllers;
using Xunit;

namespace Racetest
{
    public class CategoryRetrievalTests
    {
        [Fact]
        public void GetCategories_InvalidEventID_ReturnsNotFound()
        {
            var controller = new CategoriesController();

            var result = controller.GetCategories(0);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
