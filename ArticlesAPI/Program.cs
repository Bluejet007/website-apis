using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;

namespace WebsiteAPIs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAzureClients(clBuilder =>
            {
                clBuilder.AddBlobServiceClient(builder.Configuration.GetValue<string>("Storage:connectionString"));

                clBuilder.AddQueueServiceClient(builder.Configuration.GetValue<string>("Storage:connectionString"))
                .ConfigureOptions(op => op.MessageEncoding = QueueMessageEncoding.Base64);

                clBuilder.AddClient((CosmosClientOptions op) => new CosmosClient(builder.Configuration.GetValue<string>("Cosmos:connectionString")));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
