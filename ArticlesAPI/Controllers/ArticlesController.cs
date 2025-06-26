using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Net;

namespace WebsiteAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly Container container;
        public ArticlesController(CosmosClient client)
        {
            this.container = client.GetContainer("articles", "articlescontainer");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllArticles()
        {
            IOrderedQueryable<Article> queryableArticles = this.container.GetItemLinqQueryable<Article>();
            FeedIterator<Article> feed = queryableArticles.ToFeedIterator();
            
            List<Article> articles = new List<Article>();
            while(feed.HasMoreResults)
            {
                FeedResponse<Article> response = await feed.ReadNextAsync();

                foreach(Article art in response)
                {
                    articles.Add(art);
                }
            }

            return this.Ok(articles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle(string id)
        {
            ItemResponse<Article>? response = null;

            try
            {
                response = await this.container.ReadItemAsync<Article>(id, new PartitionKey(id));
            }
            catch (CosmosException ex)
            {
                if(ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.NotFound();
                }
            }

            return this.Ok(response?.Resource);
        }

        [HttpPost]
        public async Task<IActionResult> PostArticle(Article article)
        {
            ItemResponse<Article>? response = null;

            try
            {
                article.Id = Guid.NewGuid().ToString();
                response = await this.container.CreateItemAsync(article);
            }
            catch (CosmosException)
            {
                return this.Problem();
            }

            return this.Ok(response?.Resource.Id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            ItemResponse<Article>? response = null;

            try
            {
                response = await this.container.DeleteItemAsync<Article>(id, new PartitionKey(id));
            }
            catch (CosmosException)
            {
                return this.Problem();
            }

            return this.Ok();
        }
    }
}
