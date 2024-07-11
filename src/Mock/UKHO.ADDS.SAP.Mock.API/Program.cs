using System.Text.Json;
using SoapCore;
using UKHO.ADDS.SAP.Mock.API.Services;
using UKHO.ADDS.SAP.Mock.ErpCallback;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSoapCore();
builder.Services.AddSingleton<CustomDummyData>(
    _ => 
        JsonSerializer.Deserialize<CustomDummyData>(
            File.OpenText("ProductDummyData.json").ReadToEnd()
            ) ?? throw new InvalidOperationException("Could not load test data config from ProductDummyData.json"));
builder.Services.AddSingleton<ErpFacadePriceEndpointClient>();
builder.Services.AddSingleton<Iz_adds_mat_info, z_adds_mat_info>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseRouting();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<Iz_adds_mat_info>("/z_adds_mat_info.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
});
#pragma warning restore ASP0014

app.Run();

