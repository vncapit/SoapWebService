using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoapCore;
using Webservice.ServiceContract;
using Webservice.ServiceImpl;
using static SoapCore.DocumentationWriter.SoapDefinition;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSoapCore();
builder.Services.AddSingleton<IOrderService, OrderService>();
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<IOrderService>("/OrderService.asmx", new SoapEncoderOptions(), SoapSerializer.XmlSerializer);
});


app.UseHttpsRedirection();
app.MapControllers();
app.Run();
