using SoapCore;
using System.ServiceModel;
using UKHO.SAP.MockAPIService;
using UKHO.SAP.MockAPIService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ISampleService, SampleService>();


var app = builder.Build();

app.UseMyMiddleware();


app.UseRouting();//error message suggested to implement this


app.UseEndpoints(endpoints => {
    endpoints.UseSoapEndpoint<ISampleService>("/ServicePath.asmx", new SoapEncoderOptions(), SoapSerializer.DataContractSerializer);
});
app.Run();
