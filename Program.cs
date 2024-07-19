using Redbean;

await Bootstrap.Setup();

var builder = WebApplication.CreateBuilder(args);
await AppProgram.Build(builder);
await AppProgram.Run(builder);